
namespace usbprison
{
    using System;
    using Terminal.Gui;
    using Terminal.Gui.App;
    using Terminal.Gui.Drawing;
    using Terminal.Gui.Input;
    using Terminal.Gui.ViewBase;
    using Terminal.Gui.Views;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Text;
    using ReactiveUI;
    using System.Reactive.Linq;
    using System.Reactive.Disposables.Fluent;
    using System.Reactive.Disposables;
    using Serilog;
    using ReactiveMarbles.ObservableEvents;
    using System.Reactive;
    using ReactiveUI.SourceGenerators;
    using Splat;

    public partial class SingleDeviceView : ReactiveFrameView<SingleDeviceViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();
        private DeviceListView deviceListView = new DeviceListView();

        private Terminal.Gui.Views.ListView listView = new ListView();
        private Label _vid;
        private Label _name;
        private Label _pid;
        private Label _serial;
        private TextField _custom;
        private Button _enableButton;
        private Button _disableButton;

        public SingleDeviceView()
        {
            //ViewModel = viewModel;
            InitializeComponent();

            // var test = _activationFetcherCache;
            // var test2 = AppLocator.Current
            //            .GetServices<IActivationForViewFetcher?>()
            //            .Aggregate((count: 0, viewFetcher: default(IActivationForViewFetcher?)), (acc, x) =>
            //            {
            //                var score = x?.GetAffinityForView(this.GetType()) ?? 0;
            //                return score > acc.count ? (score, x) : acc;
            //            });
            // DOESN'T WORK
            this.WhenActivated(disposables =>
            {
                // Bindings go here
                // Globals.App.Invoke(() =>
                // {
                //     Terminal.Gui.Views.MessageBox.Query(Globals.App, "3333", "Register 3333 clicked!", "Ok");
                // });
                this.WhenAnyValue(x=>x.ViewModel.Device.Name).Select(x=> $"Device Details - {x}").BindTo(this, x=>x.Title).DisposeWith(disposables);
                //this.Title = $"Device Details - {ViewModel!.Device.Name}";

                this.WhenAnyValue(x=>x.ViewModel!.Device.Name).Select(x => "Name: " + (x != null ? x : "")).BindTo(_name, view=> view.Text).DisposeWith(disposables);
                this.WhenAnyValue(x=>x.ViewModel!.Device.Vid).Select(x => "VID: " + (x != 0 ? x.ToString() : "")).BindTo(_vid, view=> view.Text).DisposeWith(disposables);
                this.WhenAnyValue(x=>x.ViewModel!.Device.Pid).Select(x => "PID: " + (x != 0 ? x.ToString() : "")).BindTo(_pid, view=> view.Text).DisposeWith(disposables);
                this.WhenAnyValue(x=>x.ViewModel!.Device.SerialNumber).Select(x => "Serial: " + (x != null ? x : "")).BindTo(_serial, view=> view.Text).DisposeWith(disposables);
                this.WhenAnyValue(x=>x.ViewModel!.CustomText).BindTo(_custom, view=> view.Text).DisposeWith(disposables);
                
                this.WhenAnyValue(x => x.ViewModel!.IsDeviceTracked).Select(x => !x).BindTo(_enableButton, x => x.Visible).DisposeWith(disposables);
                _enableButton.Events().Accepting.Select(x => Unit.Default).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(this, x =>x.ViewModel!.ActivateDeviceCommand).DisposeWith(disposables);
                this.WhenAnyValue(x => x.ViewModel!.IsDeviceTracked).BindTo(_disableButton, x => x.Visible).DisposeWith(disposables);
                _disableButton.Events().Accepting.Select(x => Unit.Default).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(this, x => x.ViewModel!.DeactivateDeviceCommand).DisposeWith(disposables);

            });

            this.Activated.Subscribe(x =>
            {
                Log.Information("SingleDeviceView Activated");
            });

            this.Activate();
        }

        public override void Activate()
        {
            base.Activate();
        }


        // private static readonly MemoizingMRUCache<Type, IActivationForViewFetcher?> _activationFetcherCache =
        // new(
        //     (t, _) =>
        //         AppLocator.Current
        //                .GetServices<IActivationForViewFetcher?>()
        //                .Aggregate((count: 0, viewFetcher: default(IActivationForViewFetcher?)), (acc, x) =>
        //                {
        //                    var score = x?.GetAffinityForView(t) ?? 0;
        //                    return score > acc.count ? (score, x) : acc;
        //                }).viewFetcher,
        //     64);

        private void InitializeComponent()
        {
            // var condition = this.WhenAnyValue(x=>x.ViewModel).WhereNotNull();
            // condition.Subscribe(x => {
            //     this.Title = $"Device Details - {x.Device.Name}";

            //     this.WhenAnyValue(x=>x.ViewModel!.Device.Name).Select(x => "Name: " + (x != null ? x : "")).BindTo(_name, view=> view.Text).DisposeWith(_disposable);
            //     this.WhenAnyValue(x=>x.ViewModel!.Device.Vid).Select(x => "VID: " + (x != 0 ? x.ToString() : "")).BindTo(_vid, view=> view.Text).DisposeWith(_disposable);
            //     this.WhenAnyValue(x=>x.ViewModel!.Device.Pid).Select(x => "PID: " + (x != 0 ? x.ToString() : "")).BindTo(_pid, view=> view.Text).DisposeWith(_disposable);
            //     this.WhenAnyValue(x=>x.ViewModel!.Device.SerialNumber).Select(x => "Serial: " + (x != null ? x : "")).BindTo(_serial, view=> view.Text).DisposeWith(_disposable);
            //     this.WhenAnyValue(x=>x.ViewModel!.CustomText).BindTo(_custom, view=> view.Text).DisposeWith(_disposable);
                
            //     this.WhenAnyValue(x => x.ViewModel!.IsDeviceTracked).Select(x => !x).BindTo(_enableButton, x => x.Visible).DisposeWith(_disposable);
            //     _enableButton.Events().Accepting.Select(x => Unit.Default).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(this, x =>x.ViewModel!.ActivateDeviceCommand).DisposeWith(_disposable);
            //     this.WhenAnyValue(x => x.ViewModel!.IsDeviceTracked).BindTo(_disableButton, x => x.Visible).DisposeWith(_disposable);
            //     _disableButton.Events().Accepting.Select(x => Unit.Default).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(this, x => x.ViewModel!.DeactivateDeviceCommand).DisposeWith(_disposable);


            // }).DisposeWith(_disposable);
            
            _name = new Terminal.Gui.Views.Label();
            _name.Y = 1;
            _name.Text = "TEST";
            //condition.Select(x=>x.Device.Name).Select(x => "Name: " + (x != null ? x : "")).BindTo(name, x => x.Text).DisposeWith(_disposable);


            //.Device.Name).Select(x => "Name: " + (x != null ? x : "")).BindTo(name, x => x.Text).DisposeWith(_disposable);
            _vid = new Terminal.Gui.Views.Label();
            _vid.Y = 2;
           // condition.Select(x => x.Device.Vid).Select(x => "VID: " + (x != 0 ? x.ToString() : "")).BindTo(vid, x => x.Text).DisposeWith(_disposable);
            //this.WhenAnyValue(x => x.ViewModel.Device.Vid).Select(x => "VID: " + (x != 0 ? x.ToString() : "")).BindTo(vid, x => x.Text).DisposeWith(_disposable);
            
            _pid = new Terminal.Gui.Views.Label();
            _pid.Y = 3;
            //this.WhenAnyValue(x => x.ViewModel.Device.Pid).Select(x => "PID: " + (x != 0 ? x.ToString() : "")).BindTo(pid, x => x.Text).DisposeWith(_disposable);
            //condition.Select(x => x.Device.Pid).Select(x => "PID: " + (x != 0 ? x.ToString() : "")).BindTo(pid, x => x.Text).DisposeWith(_disposable);
            
            _serial = new Terminal.Gui.Views.Label();
            _serial.Y = 4;
            //condition.Select(x => x.Device.SerialNumber).Select(x => "Serial: " + (x != null ? x : "")).BindTo(serial, x => x.Text).DisposeWith(_disposable);

            _custom = new Terminal.Gui.Views.TextField();
            _custom.Y = 5;
            _custom.Width = Dim.Fill();
            //condition.Select(x => x.Device.CustomText).Select(x => "Serial: " + (x != null ? x : "")).BindTo(serial, x => x.Text).DisposeWith(_disposable);
            _custom.TextChanged += (sender, args) =>
            {
                if (ViewModel != null)
                {
                    ViewModel.CustomText = _custom.Text.ToString();
                }
            };



            _enableButton = new Terminal.Gui.Views.Button();
            _enableButton.Y = 6;
            _enableButton.Text = "Track Device on Main Screen";
            //this.OneWayBind(ViewModel, vm=> vm.IsDeviceTracked, view=> button.Visible).DisposeWith(_disposable);
            //this.WhenAnyValue(x => x.ViewModel.IsDeviceTracked).Select(x => !x).BindTo(button, x => x.Visible).DisposeWith(_disposable);
            //button.Accepting += (sender, args) => {};
            

            _disableButton = new Terminal.Gui.Views.Button();
            _disableButton.Y = 6;
            _disableButton.Text = "Untrack Device ";
            //condition.Select(x => x.IsDeviceTracked).BindTo(button2, x => x.Visible).DisposeWith(_disposable);
            //this.OneWayBind(ViewModel, vm=> vm.IsDeviceTracked, view=> button2.Visible).DisposeWith(_disposable);
            //this.WhenAnyValue(x => x.ViewModel.IsDeviceTracked).BindTo(button2, x => x.Visible).DisposeWith(_disposable);
            //button.Accepting += (sender, args) => {};
            



            this.Add(_name, _vid, _pid, _serial, _custom, _enableButton, _disableButton);
        }

        private void OnRegisterDevice()
        {
            Terminal.Gui.Views.MessageBox.Query(Globals.App,"Register", "Register Device clicked!", "Ok");
        }
        private void OnUnregisterDevice()
        {
            Terminal.Gui.Views.MessageBox.Query(Globals.App, "Unregister", "Unregister Device clicked!", "Ok");
        }
    }
}
