
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
    using ReactiveMarbles.ObservableEvents;
    using System.Reactive;
    using Serilog;
    using Splat;

    public class DevicesView : Terminal.Gui.Views.FrameView, IViewFor<DevicesViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();
        private Terminal.Gui.Views.ListView listView = new ListView();
        //private FrameView rightView;

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public DevicesViewModel ViewModel { get; set; }
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        object IViewFor.ViewModel
        {
            get => ViewModel;
#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
            set => ViewModel = (DevicesViewModel)value;
#pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
        }
        
        public DevicesView(DevicesViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.listView.Width = Dim.Percent(40);
            this.listView.Height = Dim.Fill();
            // this.listView.X = 2;
            // this.listView.Y = 10;
            this.listView.Visible = true;
            //// FIX THIS!!!!
            //ViewModel
            //    .WhenAnyValue(x => x.Devices)
            //    .Select(x => new Collection<SingleDeviceViewModel>(x, s => s.Name))
            //    .Cast<IListDataSource>()
            //    .BindTo(this.listView, x => x.Source)
            //    .DisposeWith(_disposable);
            //this.listView.Source  = new Collection<DeviceModel>(uSBService.Devices, s=> s.Name);

            this.Add(this.listView);

            // this.rightView = new FrameView();
            // rightView.X = Pos.Right(this.listView) + 1;
            // rightView.Y = 0;
            // rightView.CanFocus = true;
            // rightView.Width = Dim.Fill();
            // rightView.Height = Dim.Fill();
            // rightView.TabStop = TabBehavior.TabStop;
            var singleDeviceView = Splat.Locator.Current.GetService<SingleDeviceView>();
            if (singleDeviceView == null)
            {
                throw new Exception("Could not resolve SingleDeviceView from the Splat locator.");
            }
            singleDeviceView.X = Pos.Right(this.listView) + 1;
            singleDeviceView.Y = 0;
            singleDeviceView.CanFocus = true;
            singleDeviceView.Width = Dim.Fill();
            singleDeviceView.Height = Dim.Fill();
            singleDeviceView.TabStop = TabBehavior.TabStop;
            ViewModel
                 .WhenAnyValue(x => x.SelectedDevice)
                 .WhereNotNull()
                 //.Select(x =>
                 //{
                 //   //Log.Information($"Selected device changed in DevicesView: {x.Name}", x);
                 //    var viewModel = new SingleDeviceViewModel(x);
                 //       return viewModel;
                 //})
                 .BindTo(singleDeviceView, x => x.ViewModel)
                 .DisposeWith(_disposable);

            this.Add(singleDeviceView);

            listView.Events().ValueChanged.Select(x => x.NewValue).InvokeCommand(this, x => x.ViewModel.ListViewSelectionChangedCommand);



            // var name = new Terminal.Gui.Views.Label();
            // name.Y = 1;
            // ViewModel.WhenAnyValue(x => x.SelectedDevice).Where(x => x != null).Select(x => "Name: " + (x.Name != null ? x.Name : "")).BindTo(name, x => x.Text).DisposeWith(_disposable);
            // var vid = new Terminal.Gui.Views.Label();
            // vid.Y = 2;
            // ViewModel.WhenAnyValue(x => x.SelectedDevice).Where(x => x != null).Select(x => "VID: " + (x.Vid != 0 ? x.Vid.ToString() : "")).BindTo(vid, x => x.Text).DisposeWith(_disposable);
            // var pid = new Terminal.Gui.Views.Label();
            // pid.Y = 3;
            // ViewModel.WhenAnyValue(x => x.SelectedDevice).Where(x => x != null).Select(x => "PID: " + (x.Pid != 0 ? x.Pid.ToString() : "")).BindTo(pid, x => x.Text).DisposeWith(_disposable);
            // var serial = new Terminal.Gui.Views.Label();
            // serial.Y = 4;
            // ViewModel.WhenAnyValue(x => x.SelectedDevice).Where(x => x != null).Select(x => "Serial: " + (x.SerialNumber != null ? x.SerialNumber : "")).BindTo(serial, x => x.Text).DisposeWith(_disposable);

            // var custom = new Terminal.Gui.Views.TextField();
            // custom.Y = 5;
            // custom.Width = Dim.Fill();
            // this.Bind(ViewModel, x => x.SelectedDevice).Where(x => x != null).Select(x => x.CustomText ?? "").BindTo(custom, x => x.Text).DisposeWith(_disposable);


            // var button = new Terminal.Gui.Views.Button();
            // button.Y = 6;
            // button.Text = "Track Device on Main Screen";
            // ViewModel.WhenAnyValue(x => x.SelectedDevice, x=>x.IsSelectedDeviceTracked).Select(x => x.Item1 != null && !x.Item2).BindTo(button, x => x.Visible).DisposeWith(_disposable);
            // //button.Accepting += (sender, args) => {};
            // button.Events().Accepting.Select(x => Unit.Default).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(this, x => x.ViewModel.ActivateDeviceCommand);

            // var button2 = new Terminal.Gui.Views.Button();
            // button2.Y = 6;
            // button2.Text = "Untrack Device ";
            // ViewModel.WhenAnyValue(x => x.SelectedDevice, x=>x.IsSelectedDeviceTracked).Select(x => x.Item1 != null && x.Item2).BindTo(button2, x => x.Visible).DisposeWith(_disposable);
            // //button.Accepting += (sender, args) => {};
            // button2.Events().Accepting.Select(x => Unit.Default).ObserveOn(RxApp.MainThreadScheduler).InvokeCommand(this, x => x.ViewModel.DeactivateDeviceCommand);




            // rightView.Add(name, vid, pid, serial, button, button2);

        }




    }
}
