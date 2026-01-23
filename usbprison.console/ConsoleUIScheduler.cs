using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Terminal.Gui.App;

namespace usbprison
{
    public class ConsoleUIScheduler : IScheduler
    {
        public DateTimeOffset Now => DateTimeOffset.Now;

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            var innerDisp = new SingleAssignmentDisposable();
            // Run immediately on the ui thread

            Globals.App.Invoke(() =>
            {
                if (!innerDisp.IsDisposed)
                {
                    innerDisp.Disposable = action(this, state);
                }
            });
            return innerDisp;
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var innerDisp = new SingleAssignmentDisposable();
            // Delay execution
            var timer = new System.Threading.Timer(_ => {
                
                Globals.App.Invoke(() =>
                {
                    if (!innerDisp.IsDisposed)
                    {
                        innerDisp.Disposable = action(this, state);
                    }
                });
            }, null, dueTime, TimeSpan.FromMilliseconds(-1));
            innerDisp.Disposable = timer;
            return innerDisp;
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var span = dueTime - DateTimeOffset.Now; 
            var innerDisp = new SingleAssignmentDisposable();
            // Delay execution
            var timer = new System.Threading.Timer(_ => {
                Globals.App.Invoke(() =>
                {
                    if (!innerDisp.IsDisposed)
                    {
                        innerDisp.Disposable = action(this, state);
                    }
                });
            }, null, span, TimeSpan.FromMilliseconds(-1));
            innerDisp.Disposable = timer;
            return innerDisp;
        }
    }
}