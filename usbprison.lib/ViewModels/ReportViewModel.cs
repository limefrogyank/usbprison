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
        public ReadOnlyObservableCollection<GroupedDeviceLogViewModel> GroupedLogs { get; }
        public ReportViewModel() 
        {
            _reportService = Locator.Current.GetService<ReportService>();
            GroupedLogs = _reportService!.GroupedLogs;
        }
        
        public async Task InitializeAsync()
        {
            
        }

    }
}
