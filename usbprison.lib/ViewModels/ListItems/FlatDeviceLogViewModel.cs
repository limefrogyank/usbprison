using DynamicData;
using DynamicData.Binding;
using DynamicData.List;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
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

    public partial class FlatDeviceLogViewModel : ReactiveObject
    {
        private DatabaseService? _databaseService;
        [Reactive] private string _name = string.Empty;

        public PrisonLog Log { get; }

        public FlatDeviceLogViewModel(PrisonLog log)
        {
            Log = log;

            if (log.MachineId == "__GROUP__")
            {
                _databaseService = Locator.Current.GetService<DatabaseService>();
                _ = Task.Run(async () =>
                {
                    var device = await _databaseService!.DB.Table<TrackedDeviceModel>().Where(x => x.Id == log.DeviceId).FirstOrDefaultAsync();
                    if (device == null)
                    {
                        Name = "Unknown Device";
                    }
                    else
                    {
                        Name = string.IsNullOrEmpty(device.CustomText) ?  device.Name : device.CustomText;
                    }
                });
            }




        }


    }
}
