
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
    using Attribute = Terminal.Gui.Drawing.Attribute;
    using ReactiveMarbles.ObservableEvents;

    public class TrackingView : ReactiveFrameView<TrackingViewModel>
    {
        private Terminal.Gui.Views.ListView _listView = new ListView();
        
        private SingleTrackedDeviceView _subFrame= new SingleTrackedDeviceView();

        public TrackingView(TrackingViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

            this.WhenActivated(d =>
            {
                if (ViewModel != null)
                {
                    ViewModel.ManualRefreshRequested.ObserveOn(RxSchedulers.MainThreadScheduler).Subscribe(x =>
                    {
                        Log.Information("Manual refresh requested in TrackingView");
                        _listView.SetNeedsDraw();
                    }).DisposeWith(d);

                    ViewModel.WhenAnyValue(x => x.TrackedDevices)
                        .Select(x =>
                        {
                            var collection = new CollectionEx<TrackedDeviceViewModel>(x, s => (s.DisplayName ?? s.Device.Name) + (s.IsPluggedIn ? " (Plugged In)" : ""));
                            return collection;
                        })
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .BindTo(_listView, x => x.Source)
                        .DisposeWith(d);

                    //ViewModel.WhenAnyValue(x => x.SelectedDevice).WhereNotNull().ObserveOn(RxApp.MainThreadScheduler).BindTo(_listView, x => x.SelectedItem).DisposeWith(d);

                    this._listView.Events().ValueChanged.Where(x=>x.NewValue.HasValue).Select(x => {ViewModel.SelectedDevice = ViewModel.TrackedDevices[x.NewValue!.Value]; return x;} ).Subscribe().DisposeWith(d);
                    
                    ViewModel.WhenAnyValue(x=>x.SelectedDevice).Where(x=>x!=null).BindTo(this, x=>x._subFrame.ViewModel).DisposeWith(d);
                }
            });
        }

        private void InitializeComponent()
        {
            var label = new FaintReverseLabel();
            label.Text = "This tab shows the devices that are currently being tracked. Select devices to track in the Devices tab.";
            label.X = 0;
            label.Y = 0;
            label.Height = 2;
                      
            label.Width = Dim.Fill();
            this.Add(label);

            _listView.X = 0;
            _listView.Y = Pos.Bottom(label) + 1;
            _listView.Width = Dim.Fill();
            _listView.Height = Dim.Fill() - 6;
            this.Add(_listView);

            _subFrame = new SingleTrackedDeviceView();
            _subFrame.TabStop = TabBehavior.TabStop;
            _subFrame.X = 0;
            _subFrame.Y = Pos.Bottom(_listView) + 1;
            _subFrame.Width = Dim.Fill();
            _subFrame.Height = Dim.Fill();
            this.Add(_subFrame);

           
        }
      

   }
}
