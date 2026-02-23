using Serilog.Events;

namespace usbprison.blazor
{
    public class BrowserConsoleSink : Serilog.Core.ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;

        public BrowserConsoleSink(IFormatProvider formatProvider = null)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            // Example: Write to console with custom formatting
            var message = logEvent.RenderMessage(_formatProvider);
            Console.WriteLine(message);
        }
    }
}
