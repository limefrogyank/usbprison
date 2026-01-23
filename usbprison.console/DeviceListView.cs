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

    
    public class DeviceListView : Terminal.Gui.ViewBase.View {
        private Terminal.Gui.Views.Label label = new Label();
        private Terminal.Gui.Views.ListView listView = new ListView();
        private ObservableCollection<string> data;

        public DeviceListView() {
            InitializeComponent();
            
            // Handle selection change
            listView.SelectedItemChanged += (sender, args) =>
            {
                this.Title = $"Selected: {data[args.Item.Value]}";
            };
        }

        private void InitializeComponent()
        {
            // Set up default size
            Width = Dim.Auto();
            Height = Dim.Auto();

            
            // this.label.Width = Dim.Auto();
            // this.label.Height = Dim.Auto();
            // this.label.X = 0;
            // this.label.Y = 0;
            // this.label.Visible = true;
            // this.label.Arrangement = Terminal.Gui.ViewBase.ViewArrangement.Fixed;
            // this.label.CanFocus = false;
            // this.label.ShadowStyle = Terminal.Gui.ViewBase.ShadowStyle.None;
            // this.label.Data = "label";
            // this.label.Text = "OMG2!";
            // this.label.TextAlignment = Terminal.Gui.ViewBase.Alignment.Start;
            // this.Add(this.label);

            this.listView.Width = Dim.Auto();
            this.listView.Height = Dim.Auto();
            this.listView.X = 2;
            this.listView.Y = 2;
            this.listView.Visible = true;
            this.listView.Arrangement = Terminal.Gui.ViewBase.ViewArrangement.Fixed;
            this.listView.CanFocus = true;
            this.listView.ShadowStyle = Terminal.Gui.ViewBase.ShadowStyle.Opaque;
            this.listView.Data = "listView";
            
            data = new ObservableCollection<string>{"Device 1", "Device 2", "Device 3"};
            this.listView.SetSource(data);
            listView.SelectedItemChanged += (s, e) => { };
            this.Add(this.listView);
            
            

            // this.listView.RowRender += (sender, args) =>
            // {
            //     // Custom rendering logic here
            //     args.Row = $"[ ] {args.Row}";
            // };
        }
    }
}
