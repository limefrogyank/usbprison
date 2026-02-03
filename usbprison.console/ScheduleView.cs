
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
    using DynamicData;
    using ReactiveMarbles.ObservableEvents;

    public partial class ScheduleView : ReactiveFrameView<ScheduleViewModel>
    {
        private Label _startTimeLabel = null!;
        private Label _endTimeLabel = null!;
        private List<TimeField> _startTimeFields = new List<TimeField>();
        private List<TimeField> _endTimeFields = new List<TimeField>();

        public ScheduleView(ScheduleViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.WhenAnyValue(x => x.ViewModel!.TransformedCache).Subscribe(cache =>
                {
                    cache.Connect()
                    //.ToCollection()
                    .OnItemAdded(item =>
                    {
                        var index = (int)item.DayOfWeek;
                        var field = _startTimeFields[index];
                        field.Value = item.StartTime;
                        Observable.FromEventPattern<ValueChangedEventArgs<TimeSpan>>(
                            h => field.ValueChanged += h,
                            h => field.ValueChanged -= h
                        ).Subscribe(x =>
                        {
                            item.StartTime = x.EventArgs.NewValue;
                        });

                        field = _endTimeFields[index];
                        field.Value = item.EndTime;
                        Observable.FromEventPattern<ValueChangedEventArgs<TimeSpan>>(
                            h => field.ValueChanged += h,
                            h => field.ValueChanged -= h
                        ).Subscribe(x => item.EndTime = x.EventArgs.NewValue);
                    })
                    .Subscribe()
                    .DisposeWith(d);
                });

            });
        }

        private void InitializeComponent()
        {
            _startTimeLabel = new Terminal.Gui.Views.Label()
            {
                X = 20,
                Y = 1,
                Width = 15,
                Height = 1,
                Text = "Start Lockdown"
            };
            this.Add(_startTimeLabel);
            _endTimeLabel = new Terminal.Gui.Views.Label()
            {
                X = 40,
                Y = 1,
                Width = 15,
                Height = 1,
                Text = "End Lockdown"
            };
            this.Add(_endTimeLabel);

            for (var i = 0; i < 7; i++)
            {
                var dayLabel = new Terminal.Gui.Views.Label()
                {
                    X = 0,
                    Y = i * 2 + 3,
                    Width = 15,
                    Height = 1,
                    Text = ((DayOfWeek)i).ToString()
                };
                this.Add(dayLabel);

                var startTimeField = new TimeField()
                {
                    X = 20,
                    Y = i * 2 + 3,
                    Width = 15,
                    Height = 1,
                };
                _startTimeFields.Add(startTimeField);
                this.Add(startTimeField);

                var endTimeField = new TimeField()
                {
                    X = 40,
                    Y = i * 2 + 3,
                    Width = 15,
                    Height = 1,
                };
                _endTimeFields.Add(endTimeField);
                this.Add(endTimeField);
            }
        }


    }
}
