using DynamicData;
using DynamicData.Binding;
using DynamicData.List;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace usbprison
{

    public class GroupedMultiTrackedViewModel: ObservableCollectionExtended<MultiTrackedDeviceViewModel>, IDisposable
    {
        //public static IEnumerable<T> GetBoundItems(IObservableCache<T,TKey> cache)
        //{
        //    cache.Connect()
        //        .Bind(out var list)
        //        .Subscribe();
        //    return list;
        //}

        public string Name { get; set; } = string.Empty;

        //public Func<T, T, bool> IgnoreUpdateFunction { get; set; } = (current, previous) => false;

        public GroupedMultiTrackedViewModel(string name, IGroup<MultiTrackedDeviceViewModel, string, string> data, IScheduler scheduler)
        {
            Name = name;
            //IgnoreUpdateFunction = ignoreUpdateFunction;
            //load and sort the grouped list
            data.Cache.Connect()
                //.IgnoreUpdateWhen(IgnoreUpdateFunction)

                .Do(x => Debug.WriteLine("New Items list triggered?"))
                .ObserveOn(scheduler)
                .Bind(this, updater: new CustomAdaptor()) //make the reset threshold large because xamarin is slow when reset is called (or at least I think it is @erlend, please enlighten me )
                .Subscribe();

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
