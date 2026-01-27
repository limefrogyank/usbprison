using Splat;

namespace usbprison.maui.Pages
{
    public partial class ScheduleView : PageBase<ScheduleViewModel>
    {
        public ScheduleView() : base()
        {
            InitializeComponent();
            ViewModel = Locator.Current.GetService<ScheduleViewModel>();
        }
    }
}