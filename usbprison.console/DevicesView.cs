
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

    public class DevicesView : ReactiveFrameView<DevicesViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();
        private Terminal.Gui.Views.ListView listView = new ListView();
        
        public DevicesView(DevicesViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            var label = new FaintReverseLabel();
            label.Text = "Use Tab to switch to right side detail view. Use Up/Down to select a device.";
            label.X = 0;
            label.Y = 0;
            label.Width = Dim.Fill();
            this.Add(label);

            this.listView.Y = 2;
            this.listView.Width = Dim.Percent(40);
            this.listView.Height = Dim.Fill();
            // this.listView.X = 2;
            // this.listView.Y = 10;
            this.listView.Visible = true;
            //// FIX THIS!!!!
            ViewModel
               .WhenAnyValue(x => x.Devices)
               .Select(x => new Collection<SingleDeviceViewModel>(x, s => s.Name))
               .Cast<IListDataSource>()
               .BindTo(this.listView, x => x.Source)
               .DisposeWith(_disposable);
            //this.listView.Source  = new Collection<SingleDeviceViewModel>(ViewModel.Devices, s=> s.Name);

            this.Add(this.listView);


            var singleDeviceView = Splat.Locator.Current.GetService<SingleDeviceView>();
            if (singleDeviceView == null)
            {
                throw new Exception("Could not resolve SingleDeviceView from the Splat locator.");
            }
            singleDeviceView.X = Pos.Right(this.listView) + 1;
            singleDeviceView.Y = 2;
            singleDeviceView.CanFocus = true;
            singleDeviceView.Width = Dim.Fill();
            singleDeviceView.Height = Dim.Fill();
            singleDeviceView.TabStop = TabBehavior.TabStop;
            ViewModel
                 .WhenAnyValue(x => x.SelectedDevice)
                 .WhereNotNull()
                 .BindTo(singleDeviceView, x => x.ViewModel)
                 .DisposeWith(_disposable);

            this.Add(singleDeviceView);

            listView.Events().ValueChanged.Select(x => x.NewValue).InvokeCommand(this, x => x.ViewModel!.ListViewSelectionChangedCommand);

        }




    }
}
