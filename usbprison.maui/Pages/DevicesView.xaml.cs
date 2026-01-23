using Splat;

namespace usbprison.maui.Pages
{
    public partial class DevicesView : PageBase<DevicesViewModel>
    {
        public DevicesView() : base()
        {
            InitializeComponent();
            ViewModel= Locator.Current.GetService<DevicesViewModel>();
        }
    }
}