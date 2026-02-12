using ReactiveUI;
using ReactiveUI.Maui;
using Splat;

namespace usbprison.maui.Pages
{
    public partial class MainView : ReactiveTabbedPage<MainViewModel>
    {
        public MainView():base()
        {
            InitializeComponent();
            //ViewModel = viewmodel;
            
            this.Children.Add(new TrackingView());
            this.Children.Add(new DevicesView());
            this.Children.Add(new ScheduleView());
            this.Children.Add(new ReportView());
        }
    }
}