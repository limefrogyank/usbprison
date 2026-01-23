using Microsoft.Extensions.DependencyInjection;

namespace usbprison.maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new RootPage());
        }
    }
}