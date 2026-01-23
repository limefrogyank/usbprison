namespace usbprison.maui.Pages;

public class TestPage : ContentPage
{
	public TestPage()
	{
		Title = "TEst";
		Content = new VerticalStackLayout
		{
			Children = {
				new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to .NET MAUI!"
				}
			}
		};
	}
}