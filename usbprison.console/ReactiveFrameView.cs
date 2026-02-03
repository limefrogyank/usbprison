using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using ReactiveUI;
using Terminal.Gui.Views;

namespace usbprison
{
    public class ReactiveFrameView<T> : FrameView, IViewFor<T>, INotifyPropertyChanged, ICanActivate, IDisposable where T : class, INotifyPropertyChanged
    {
        private readonly Subject<Unit> _initSubject = new();
        private readonly Subject<Unit> _deactivateSubject = new();
        private readonly CompositeDisposable _compositeDisposable = [];
        private T? _viewModel;


        public T? ViewModel
        {
            get => _viewModel;
            set
            {
                if (EqualityComparer<T?>.Default.Equals(_viewModel, value))
                {
                    return;
                }

                _viewModel = value;
                OnPropertyChanged();
            }
        }

        object IViewFor.ViewModel
        {
            get => ViewModel!;
#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
            set => ViewModel = (T)value;
#pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
        }

        public IObservable<Unit> Activated => _initSubject.AsObservable();

        public IObservable<Unit> Deactivated => _deactivateSubject.AsObservable();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Activate()
        {
        }

        public virtual void Deactivate()
        {
        }

        public new virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ReactiveFrameView()
        {
            this.Border?.Thickness = new Terminal.Gui.Drawing.Thickness(0);

            if (ViewModel is IActivatableViewModel avm)
            {
                Activated.Subscribe(_ => avm.Activator.Activate()).DisposeWith(_compositeDisposable);
                Deactivated.Subscribe(_ => avm.Activator.Deactivate());
            }

            _initSubject.OnNext(Unit.Default);

            var viewModelChanged =
                this.WhenAnyValue<ReactiveFrameView<T>, T?>(nameof(ViewModel))
                    .WhereNotNull()
                    .Publish()
                    .RefCount(2);

            viewModelChanged
                //.Skip(1) // Skip the initial value to avoid unnecessary re-render when ViewModel changes
                .Subscribe(_ => this.SetNeedsDraw())
                .DisposeWith(_compositeDisposable);

            viewModelChanged
                .Select(x =>
                    Observable
                        .FromEvent<PropertyChangedEventHandler?, Unit>(
                            eventHandler =>
                            {
                                void Handler(object? sender, PropertyChangedEventArgs e) => eventHandler(Unit.Default);
                                return Handler;
                            },
                            eh => x.PropertyChanged += eh,
                            eh => x.PropertyChanged -= eh))
                .Switch()
                .Subscribe(_ => 
                {
                    this.SetNeedsDraw();    
                })
                .DisposeWith(_compositeDisposable);
        }

        
    }

}