
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
    using Splat;

    public partial class SingleTrackedDeviceView : ReactiveFrameView<TrackedDeviceViewModel>
    {
        readonly CompositeDisposable _disposable = new CompositeDisposable();


        private Terminal.Gui.Views.ListView listView = new ListView();
        private Button _disableButton = null!;

        public SingleTrackedDeviceView()
        {
            InitializeComponent();
            
            this.WhenActivated(disposables =>
            {
                //this.WhenAnyValue(x=>x.ViewModel!.Device.Name).Select(x=> $"Device Details - {x}").BindTo(this, x=>x.Title).DisposeWith(disposables);

                this.WhenAnyValue(x=>x.ViewModel!.DisplayName).Select(x => "Name: " + (x != null ? x : "")).BindTo(this, view=> view.Title).DisposeWith(disposables);
                
                this.WhenAnyValue(x=>x.ViewModel).Select(x=>x != null).BindTo(this, view=>view._disableButton.Visible).DisposeWith(disposables);
                _disableButton.Events().Accepting.Select(x => Unit.Default).ObserveOn(RxSchedulers.MainThreadScheduler).InvokeCommand(this, x => x.ViewModel!.RemoveCommand).DisposeWith(disposables);

            });

        }


        private void InitializeComponent()
        {
           this.Border?.Thickness = new Terminal.Gui.Drawing.Thickness(1);
           
            var label = new FaintReverseLabel();
            label.X = 0;
            label.Y = 0;
            label.Height = 1;
            label.Width = Dim.Fill();
            label.Text = "Select a device to get more options here.  Use Tab to switch to this section.";

            // _name = new Terminal.Gui.Views.Label();
            // _name.Y = Pos.Bottom(label) + 1;
            // _name.Text = "";

            _disableButton = new Terminal.Gui.Views.Button();
            _disableButton.X = 0;
            _disableButton.Y = Pos.Bottom(label) + 1;
            _disableButton.Text = "Remove Tracked Device ";
            _disableButton.Visible = true;

            this.Add(label,  _disableButton);
        }

        
    }
}
