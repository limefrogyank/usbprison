
namespace usbprison
{
    using System;
    using Terminal.Gui;
    using Terminal.Gui.App;
    using Terminal.Gui.Drawing;
    using Terminal.Gui.Input;
    using Terminal.Gui.ViewBase;
    using Terminal.Gui.Views;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Text;
    using ReactiveUI;
    using System.Reactive.Linq;
    using System.Reactive.Disposables.Fluent;
    using System.Reactive.Disposables;
    using Serilog;

    public class MainView : Terminal.Gui.Views.Window, IViewFor<MainViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();

        //private ObservableCollection<string> data = new ObservableCollection<string> { "Device 1", "Device 2", "Device 3" };
        //public ObservableCollection<DeviceModel> Devices = new ObservableCollection<DeviceModel>();
        //        private Terminal.Gui.Views.Label label;
        //      private Terminal.Gui.Views.Button button1;

        private Terminal.Gui.Views.ListView listView = new ListView();

        

        public MainViewModel? ViewModel {get;set;}
        object IViewFor.ViewModel {
			get => ViewModel!;
#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
            set => ViewModel = (MainViewModel) value;
#pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
        }

        public MainView(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            
            var tabView = new Terminal.Gui.Views.TabView();
            tabView.X = 0;
            tabView.Y = 0;
            tabView.Width = Dim.Fill();
            tabView.Height = Dim.Fill();
            //tabView.Border.Thickness = new Thickness(0);
            this.Add(tabView);

            var mainTab = new Terminal.Gui.Views.Tab();
            //mainTab.Title = "_Main";
            mainTab.DisplayText = "Main";
            mainTab.View = new TrackingView(new TrackingViewModel());
            mainTab.View.Width = Dim.Fill();
            mainTab.View.Height = Dim.Fill();
            tabView.AddTab(mainTab, true);

            var devicesTab = new Terminal.Gui.Views.Tab();
            devicesTab.DisplayText = "Devices";
            //devicesTab.Border.Thickness = new Thickness(1);
            // var devicesView = new FrameView();
            // devicesView.Width = Dim.Fill();
            // devicesView.Height = Dim.Fill();
            //devicesView.Border?.Thickness = new Thickness(1);
            devicesTab.View = new DevicesView(new DevicesViewModel());
            devicesTab.View.Width = Dim.Fill();
            devicesTab.View.Height = Dim.Fill();
            tabView.AddTab(devicesTab, false);

            var scheduleTab = new Terminal.Gui.Views.Tab();
            scheduleTab.DisplayText = "Schedule";
            scheduleTab.View = new ScheduleView(new ScheduleViewModel());
            scheduleTab.View.Width = Dim.Fill();
            scheduleTab.View.Height = Dim.Fill();
            tabView.AddTab(scheduleTab, false);

             var reportTab = new Terminal.Gui.Views.Tab();
            reportTab.DisplayText = "Reports";
            reportTab.View = new ReportView(new ReportViewModel());
            reportTab.View.Width = Dim.Fill();
            reportTab.View.Height = Dim.Fill();
            tabView.AddTab(reportTab, false);

            // var textview = new Terminal.Gui.Views.TextView();
            // textview.X = 0;
            // textview.Y = Pos.Bottom(tabView);
            // textview.Width = Dim.Fill();    
            // textview.Height = 5;
            // Log.Information("Creating textView for debug output");
            // var debugService = Splat.Locator.Current.GetService(typeof(DebugService)) as DebugService;
            // debugService!.DebugMessage.ObserveOn(RxApp.MainThreadScheduler).Subscribe(message =>
            // {
            //     //Globals.App.Invoke(() => 
            //     //{
            //         textview.Text = message;
            //     //});
                
            // });
            // this.Add(textview);
            // textview.Text = "Debug Output:\n";
        }

        private void OnRegisterDevice()
        {
            Terminal.Gui.Views.MessageBox.Query(Globals.App,"Register", "Register Device clicked!", "Ok");
        }
        private void OnUnregisterDevice()
        {
            Terminal.Gui.Views.MessageBox.Query(Globals.App, "Unregister", "Unregister Device clicked!", "Ok");
        }
    }
}
