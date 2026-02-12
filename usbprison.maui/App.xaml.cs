using Microsoft.Extensions.DependencyInjection;

namespace usbprison.maui
{
    public partial class App : Application
    {
#if WINDOWS

        public App()
        {
            InitializeComponent();

        }
        //// Failed notification attempt, left here in case I want to try something else.
        //public App(WebAppHost webAppHost)
        //{
        //    InitializeComponent();

        //    // Start web app server.
        //    _ = Task.Run(() => webAppHost.StartAsync());
        //}
#else
        public App()
        {
            InitializeComponent();

        }
#endif

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new RootPage());
        }
    }
}