using ReactiveUI;
using ReactiveUI.Maui;
using Serilog;
using Shiny.Notifications;
using Splat;
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

            });
        }

       
    }
}