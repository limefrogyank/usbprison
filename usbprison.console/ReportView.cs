
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

    public class ReportView : ReactiveFrameView<ReportViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();
        private Terminal.Gui.Views.ListView _listView = new ListView();
        
        public ReportView(ReportViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

            this.WhenActivated(d =>
            {
                ViewModel
               .WhenAnyValue(x => x.FlattenedLogs)
               
               .Select(x => new Collection<PrisonLog>(x, s => s.DeviceId + " - " + s.MachineId + " - " + s.Timestamp.ToString("G")))
               .Cast<IListDataSource>()
               .BindTo(this._listView, x => x.Source)
               .DisposeWith(_disposable);
            });
        }

        private void InitializeComponent()
        {
            this._listView.Width = Dim.Fill();
            this._listView.Height = Dim.Fill();
            this._listView.X = 2;
            this._listView.Y = 2;
            this._listView.Visible = true;
            //// FIX THIS!!!!
            // ViewModel
            //    .WhenAnyValue(x => x.FlattenedLogs)
               
            //    .Select(x => new Collection<PrisonLog>(x, s => s.DeviceId + " - " + s.MachineId + " - " + s.Timestamp.ToString("G")))
            //    .Cast<IListDataSource>()
            //    .BindTo(this._listView, x => x.Source)
            //    .DisposeWith(_disposable);
            //this.listView.Source  = new Collection<SingleDeviceViewModel>(ViewModel.Devices, s=> s.Name);

            this.Add(this._listView);

            



        }




    }
}
