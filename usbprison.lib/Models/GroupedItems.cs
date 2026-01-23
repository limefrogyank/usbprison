using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

namespace usbprison
{
    
    public class GroupedItems<T> : ObservableCollection<T> where T : class
    {
        public string Name { get; set; } = string.Empty;

        public GroupedItems(string name, ObservableCollection<T> data) : base(data)
        {
            Name = name;


            
        }
    }
}
