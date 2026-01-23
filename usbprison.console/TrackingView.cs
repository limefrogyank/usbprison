
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
    using DynamicData.Binding;
    using Serilog;

    public class TrackingView : Terminal.Gui.Views.FrameView, IViewFor<TrackingViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();
        private DeviceListView deviceListView = new DeviceListView();
        //private ObservableCollection<string> data = new ObservableCollection<string> { "Device 1", "Device 2", "Device 3" };
        //public ObservableCollection<DeviceModel> Devices = new ObservableCollection<DeviceModel>();
        //        private Terminal.Gui.Views.Label label;
        //      private Terminal.Gui.Views.Button button1;

        private Terminal.Gui.Views.ListView listView = new ListView();



        public TrackingViewModel? ViewModel { get; set; }
        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TrackingViewModel)value;
        }

        public TrackingView(TrackingViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            var listView = new Terminal.Gui.Views.ListView();
            listView.Width = Dim.Fill();
            listView.Height = Dim.Fill();
            ViewModel.WhenAnyValue(x => x.TrackedDevices)
                .Select(x =>
                {
                    var collection = new CollectionEx<TrackedDeviceViewModel>(x, s => s.Device.Name + (s.IsPluggedIn ? " (Plugged In)" : ""));
                    return collection;
                })

                .ObserveOn(RxApp.MainThreadScheduler)
                .BindTo(listView, x => x.Source)
                .DisposeWith(_disposable);

            ViewModel.ManualRefreshRequested.Subscribe(x =>
            {
                Log.Information("Manual refresh requested in TrackingView");
                listView.SetNeedsDraw();
            });

            this.Add(listView);
        }

        private void GetInfoForDevice(DeviceModel selectedDevice, FrameView rightView)
        {
            rightView.RemoveAll();
            var name = new Terminal.Gui.Views.Label();
            name.Text = selectedDevice.Name;
            name.Y = 1;
            var vid = new Terminal.Gui.Views.Label();
            vid.Text = $"VID: {selectedDevice.Vid:X4}";
            vid.Y = 2;
            var pid = new Terminal.Gui.Views.Label();
            pid.Text = $"PID: {selectedDevice.Pid:X4}";
            pid.Y = 3;


            var button = new Terminal.Gui.Views.Button();
            button.Text = "Track Device on Main Screen";


            rightView.Add(name, vid, pid);
        }

        private void OnRegisterDevice()
        {
            Terminal.Gui.Views.MessageBox.Query(Globals.App, "Register", "Register Device clicked!", "Ok");
        }
        private void OnUnregisterDevice()
        {
            Terminal.Gui.Views.MessageBox.Query(Globals.App, "Unregister", "Unregister Device clicked!", "Ok");
        }
    }
}
