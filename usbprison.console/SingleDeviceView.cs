
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
    using ReactiveMarbles.ObservableEvents;
    using System.Reactive;
    using ReactiveUI.SourceGenerators;

    public partial class SingleDeviceView : ReactiveFrameView<SingleDeviceViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();
        private DeviceListView deviceListView = new DeviceListView();
        //private ObservableCollection<string> data = new ObservableCollection<string> { "Device 1", "Device 2", "Device 3" };
        //public ObservableCollection<DeviceModel> Devices = new ObservableCollection<DeviceModel>();
        //        private Terminal.Gui.Views.Label label;
        //      private Terminal.Gui.Views.Button button1;

        private Terminal.Gui.Views.ListView listView = new ListView();
        private TextField custom;

        public SingleDeviceView()
        {
            //ViewModel = viewModel;
            InitializeComponent();

            // DOESN'T WORK
            this.WhenActivated(disposables =>
            {
                // Bindings go here
                Globals.App.Invoke(() =>
                {
                    Terminal.Gui.Views.MessageBox.Query(Globals.App, "3333", "Register 3333 clicked!", "Ok");
                });
            });

        }

        private void InitializeComponent()
        {
            var condition = this.WhenAnyValue(x=>x.ViewModel).WhereNotNull();
            condition.Subscribe(x => {
                this.Title = $"Device Details - {x.Device.Name}";
            }).DisposeWith(_disposable);
            
            var name = new Terminal.Gui.Views.Label();
            name.Y = 1;
            name.Text = "TEST";
            condition.Select(x=>x.Device.Name).Select(x => "Name: " + (x != null ? x : "")).BindTo(name, x => x.Text).DisposeWith(_disposable);


            //.Device.Name).Select(x => "Name: " + (x != null ? x : "")).BindTo(name, x => x.Text).DisposeWith(_disposable);
            var vid = new Terminal.Gui.Views.Label();
            vid.Y = 2;
            condition.Select(x => x.Device.Vid).Select(x => "VID: " + (x != 0 ? x.ToString() : "")).BindTo(vid, x => x.Text).DisposeWith(_disposable);
            //this.WhenAnyValue(x => x.ViewModel.Device.Vid).Select(x => "VID: " + (x != 0 ? x.ToString() : "")).BindTo(vid, x => x.Text).DisposeWith(_disposable);
            
            var pid = new Terminal.Gui.Views.Label();
            pid.Y = 3;
            //this.WhenAnyValue(x => x.ViewModel.Device.Pid).Select(x => "PID: " + (x != 0 ? x.ToString() : "")).BindTo(pid, x => x.Text).DisposeWith(_disposable);
            condition.Select(x => x.Device.Pid).Select(x => "PID: " + (x != 0 ? x.ToString() : "")).BindTo(pid, x => x.Text).DisposeWith(_disposable);
            
            var serial = new Terminal.Gui.Views.Label();
            serial.Y = 4;
            condition.Select(x => x.Device.SerialNumber).Select(x => "Serial: " + (x != null ? x : "")).BindTo(serial, x => x.Text).DisposeWith(_disposable);

            custom = new Terminal.Gui.Views.TextField();
            custom.Y = 5;
            custom.Width = Dim.Fill();
            //condition.Select(x => x.Device.CustomText).Select(x => "Serial: " + (x != null ? x : "")).BindTo(serial, x => x.Text).DisposeWith(_disposable);
            custom.TextChanged += (sender, args) =>
            {
                if (ViewModel != null)
                {
                    ViewModel.CustomText = custom.Text.ToString();
                }
            };



            var button = new Terminal.Gui.Views.Button();
            button.Y = 6;
            button.Text = "Track Device on Main Screen";
            condition.Select(x=>x.IsDeviceTracked).Select(x => !x).BindTo(button, x => x.Visible).DisposeWith(_disposable);
//            this.WhenAnyValue(x => x.ViewModel.IsDeviceTracked).Select(x => !x).BindTo(button, x => x.Visible).DisposeWith(_disposable);
            //button.Accepting += (sender, args) => {};
            button.Events().Accepting.Select(x => Unit.Default).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(this, x => x.ViewModel.ActivateDeviceCommand);

            var button2 = new Terminal.Gui.Views.Button();
            button2.Y = 7;
            button2.Text = "Untrack Device ";
            condition.Select(x => x.IsDeviceTracked).BindTo(button2, x => x.Visible).DisposeWith(_disposable);
            //this.WhenAnyValue(x => x.ViewModel.IsDeviceTracked).BindTo(button2, x => x.Visible).DisposeWith(_disposable);
            //button.Accepting += (sender, args) => {};
            button2.Events().Accepting.Select(x => Unit.Default).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(this, x => x.ViewModel.DeactivateDeviceCommand);




            this.Add(name, vid, pid, serial, custom, button, button2);
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
