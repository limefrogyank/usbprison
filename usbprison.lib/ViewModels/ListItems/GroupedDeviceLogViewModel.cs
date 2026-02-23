using DynamicData;
using DynamicData.Binding;
using DynamicData.List;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using usbprison.lib.Services;

namespace usbprison
{

    public partial class GroupedDeviceLogViewModel : ObservableCollectionExtended<DeviceLogViewModel>, INotifyPropertyChanged, IDisposable
    {
        //private ISettingsService? _settingsService;
        private DatabaseService? _databaseService;
        private CompositeDisposable _compositeDisposable;
        
        //public static IEnumerable<T> GetBoundItems(IObservableCache<T,TKey> cache)
        //{
        //    cache.Connect()
        //        .Bind(out var list)
        //        .Subscribe();
        //    return list;
        //}

        private string _name = string.Empty;
        public string Name { get=> _name; set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            } }
        //public ReadOnlyObservableCollection<DeviceLogViewModel> Logs { get; }

        public GroupedDeviceLogViewModel(IGroup<PrisonLog, string> data) 
        {
            //_settingsService = Locator.Current.GetService<ISettingsService>();
            _databaseService = Locator.Current.GetService<DatabaseService>();
            _compositeDisposable = new CompositeDisposable();

            //var device = _settingsService!.TrackedDevicesList.FirstOrDefault(x=>x.Id == data.GroupKey);
            //if (device == null)
            //{
            //    Name = "Unknown";
            //}
            //else
            //{
            //    Name = device.CustomText != null ? device.CustomText : device.Name;
            //}

            _ = Task.Run(async () =>
            {
                var device = await _databaseService!.DB.Table<TrackedDeviceModel>().Where(x => x.Id == data.GroupKey).FirstOrDefaultAsync();
                if (device == null)
                {
                    Name = "Unknown Device";
                }
                else
                {
                    Name = device.CustomText != null ? device.CustomText : device.Name;
                }
            });

            //load and sort the grouped list
            var dataLoader = data.List.Connect()
                .Transform(x=>new DeviceLogViewModel(x))
                .Sort(SortExpressionComparer<DeviceLogViewModel>.Ascending(x=>x.PrisonLog.Timestamp))
                .ObserveOn(RxSchedulers.MainThreadScheduler)
                .Bind(this) 
                .Subscribe()
                .DisposeWith(_compositeDisposable);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
