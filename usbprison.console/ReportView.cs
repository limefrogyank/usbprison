
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
        private Terminal.Gui.Views.DatePicker _datePicker = new DatePicker();

        public ReportView(ReportViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this._datePicker.Events().ValueChanged.Subscribe(x => ViewModel.SetDate(x.NewValue)).DisposeWith(d);

                ViewModel.WhenAnyValue(x => x.FlattenedLogs)
                    .Select(x => new CollectionFlattened(x))
                    .Cast<IListDataSource>()
                    .BindTo(this._listView, x => x.Source)
                    .DisposeWith(_disposable);
            });
        }

        private void InitializeComponent()
        {
            var label = new FaintReverseLabel();
            label.Text = "Pick a date to view the logs for that day. Use Tab to switch between the date picker and the log list. Use Up/Down to scroll through the logs.";
            label.X = 0;
            label.Y = 0;
            label.Height = 2;
            label.Width = Dim.Fill();
            label.TextAlignment = Alignment.Start;
            
            this.Add(label);

            this._datePicker.X = 0;
            this._datePicker.Y = Pos.Bottom(label) + 1;

            this.Add(_datePicker);

            this._listView.Width = Dim.Fill();
            this._listView.Height = Dim.Fill();
            this._listView.X = Pos.Right(_datePicker)+1;
            this._listView.Y = Pos.Bottom(label) + 1;
            this._listView.Visible = true;

            this.Add(this._listView);





        }




    }
}
