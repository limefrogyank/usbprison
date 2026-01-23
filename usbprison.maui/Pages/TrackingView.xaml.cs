using ReactiveUI;
using ReactiveUI.Maui;
using Serilog;
using Shiny.Notifications;
using Splat;
using usbprison.lib.Models;

namespace usbprison.maui.Pages
{
    
    public partial class TrackingView : ReactiveContentPage<TrackingViewModel>
    {
        public TrackingView() : base()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<TrackingViewModel>();

            
            
        }

      

    }
}