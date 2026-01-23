using ReactiveUI.Maui;
using usbprison.maui.Pages;

namespace usbprison.maui
{
    public partial class RootPage : ReactiveNavigationPage<RootViewModel>
    {
        public RootPage() : base()
        {
            //ViewModel = viewmodel;
            InitializeComponent();
#if WINDOWS
            Navigation.PushAsync(new MainView());
#else
            this.PushAsync(new ReceiverView());
#endif
        }
    }
}