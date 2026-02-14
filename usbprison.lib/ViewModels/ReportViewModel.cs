using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using usbprison.lib.Services;

namespace usbprison
{
    public partial class ReportViewModel : ReactiveObject
    {
        private readonly ReportService? _reportService;
        public ReadOnlyObservableCollection<GroupedDeviceLogViewModel> GroupedLogs1 { get; }
        public ObservableCollectionExtended<GroupedDeviceLogViewModel> GroupedLogs {get; private set; }
        public ObservableCollectionExtended<PrisonLog> FlattenedLogs { get; private set; }

        public ReportViewModel() 
        {
            _reportService = Locator.Current.GetService<ReportService>();
            GroupedLogs = _reportService!.GroupedLogs;
            FlattenedLogs = _reportService.FlattenedLogs;
        }
        
        public async Task InitializeAsync()
        {
            
        }

    }
}
