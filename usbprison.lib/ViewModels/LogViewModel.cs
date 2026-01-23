using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace usbprison
{
    public partial class LogViewModel : ReactiveObject
    {
        private string _filePath = string.Empty;
        private StreamReader? _streamReader = default;

        [Reactive] private string _logContents = "";
        [Reactive] private string _dateStr = "";

        public LogViewModel() 
        {
            
        }
        
        public async Task InitializeAsync(string directoryPath)
        {
            DateStr = DateTime.Now.ToString("yyyyMMdd");
            _filePath = Path.Combine(directoryPath, $"logs/logfile{_dateStr}.txt");
            //_filePath = $"logs/log-{datePart}.txt";
            var list = Directory.GetFiles(Path.Combine(directoryPath, "logs"));
            _streamReader = File.OpenText(_filePath);
            FileSystemWatcher watcher = new FileSystemWatcher(Path.Combine(directoryPath, "logs"))
            {
                Filter = Path.GetFileName(_filePath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };
            watcher.Changed += async (sender, e) =>
            {
                try
                {
                    await ReadMoreAsync();
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error reading file: {ex.Message}");
                }
            };
            watcher.EnableRaisingEvents = true;

            LogContents = await _streamReader.ReadToEndAsync();
        }

        private async Task ReadMoreAsync()
        {
            if (_streamReader != null)
                LogContents += await _streamReader.ReadToEndAsync();
        }
    }
}
