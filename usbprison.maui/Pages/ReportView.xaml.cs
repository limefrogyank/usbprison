using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using ReactiveUI.Maui;
using Serilog;
using Shiny.Notifications;
using Splat;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using usbprison.lib.Models;



namespace usbprison.maui.Pages
{
    public partial class ReportView : ReactiveContentPage<ReportViewModel>
    {
        public ReportView() : base()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<ReportViewModel>();

            this.WhenActivated(d =>
            {

                Observable.FromEventPattern<EventHandler<DateChangedEventArgs>, DateChangedEventArgs>(
                    handler => datePicker.DateSelected += handler,
                    handler => datePicker.DateSelected -= handler).Subscribe(x =>
                    {
                        if (x.EventArgs.NewDate.HasValue && ViewModel != null)
                            ViewModel.SetDate(x.EventArgs.NewDate.Value);
                        
                    }).DisposeWith(d);
            });
        }

        private void ImageButton_Clicked_Left(object sender, EventArgs e)
        {
            RxSchedulers.MainThreadScheduler.Schedule(datePicker, (scheduler,picker) =>
            {
                datePicker.Date = datePicker.Date - TimeSpan.FromDays(1);
                return Disposable.Empty;
            });
            //datePicker.Date = datePicker.Date - TimeSpan.FromDays(1);
        }

        private void ImageButton_Clicked_Right(object sender, EventArgs e)
        {
            
            RxSchedulers.MainThreadScheduler.Schedule(datePicker, (scheduler, picker) =>
            {
                datePicker.Date = datePicker.Date + TimeSpan.FromDays(1);
                return Disposable.Empty;
            });
        }
    }
}