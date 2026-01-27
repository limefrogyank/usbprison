using DynamicData;
using DynamicData.Binding;
using DynamicData.List;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace usbprison
{

    public class GroupedItems<T, TKey, TGroupKey> : ObservableCollectionExtended<T>, IDisposable
        where T : class 
        where TKey : notnull
        where TGroupKey : notnull
    {
        //public static IEnumerable<T> GetBoundItems(IObservableCache<T,TKey> cache)
        //{
        //    cache.Connect()
        //        .Bind(out var list)
        //        .Subscribe();
        //    return list;
        //}

        public string Name { get; set; } = string.Empty;

        public GroupedItems(string name, IGroup<T, TKey, TGroupKey> data, IScheduler scheduler) 
        {
            Name = name;

            //load and sort the grouped list
            var dataLoader = data.Cache.Connect()
                .ObserveOn(scheduler)
                .Bind(this) //make the reset threshold large because xamarin is slow when reset is called (or at least I think it is @erlend, please enlighten me )
                .Subscribe();

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
