
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

    public class TrackingView : ReactiveFrameView<TrackingViewModel>
    {
        private Terminal.Gui.Views.ListView _listView = new ListView();

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

                    
                }
            });
        }

        private void InitializeComponent()
        {
            _listView = new Terminal.Gui.Views.ListView();
            _listView.X = 0;
            _listView.Y = 0;
            _listView.Width = Dim.Fill();
            _listView.Height = Dim.Fill();
            this.Add(_listView);
        }
      

   }
}
