using DynamicData;
using DynamicData.Binding;
using DynamicData.List;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace usbprison
{

    public class DeviceLogViewModel : ReactiveObject
    {
        private ISettingsService? _settingsService;
        public PrisonLog PrisonLog { get; set; }

        //public static IEnumerable<T> GetBoundItems(IObservableCache<T,TKey> cache)
        //{
        //    cache.Connect()
        //        .Bind(out var list)
        //        .Subscribe();
        //    return list;
        //}

        public string Name { get; set; } = string.Empty;

        public string LocalDateTime => PrisonLog.Timestamp.ToLocalTime().ToLongTimeString();
        

        public DeviceLogViewModel(PrisonLog prisonLog) 
        {
            _settingsService = Locator.Current.GetService<ISettingsService>();

            PrisonLog = prisonLog;

        }

        public void Dispose()
        {
            
        }
    }
}
