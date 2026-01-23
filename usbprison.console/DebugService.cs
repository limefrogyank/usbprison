using System.Reactive.Subjects;

namespace usbprison
{
    public class DebugService
    {
        private Subject<string> _debugMessage = new Subject<string>();
        public IObservable<string> DebugMessage => _debugMessage;
        public void Log(string message)
        {
            // Implement logging logic here, e.g., write to console or a file
            _debugMessage.OnNext(message);
        }
    }
}