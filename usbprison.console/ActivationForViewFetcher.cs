using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace ReactiveUI.TerminalGui;

public class ActivationForViewFetcher : IActivationForViewFetcher
{
    /// <inheritdoc/>
    public int GetAffinityForView(Type view) =>
       typeof(FrameView).GetTypeInfo().IsAssignableFrom(view.GetTypeInfo()) ||
       typeof(View).GetTypeInfo().IsAssignableFrom(view.GetTypeInfo()) 
       ? 10 : 0;

    /// <inheritdoc/>
    public IObservable<bool> GetActivationForView(IActivatableView view)
    {
        var activation =
            //GetActivationFor(view as ICanActivate) ??
            GetActivationFor(view as FrameView) ??
            GetActivationFor(view as View) ??

            Observable.Create<bool>(observer => { observer.OnCompleted(); return Disposable.Empty; });

        return activation.DistinctUntilChanged();
    }

    private static IObservable<bool>? GetActivationFor(ICanActivate? canActivate) =>
        canActivate?.Activated.Select(static _ => true).Merge(canActivate.Deactivated.Select(static _ => false));


    private static IObservable<bool>? GetActivationFor(FrameView? page)
    {
        if (page is null)
        {
            return null;
        }

        var appearing = Observable.FromEvent<EventHandler, bool>(
                                                                 eventHandler =>
                                                                 {
                                                                     void Handler(object? sender, EventArgs e) => eventHandler(true);
                                                                     return Handler;
                                                                 },
                                                                 x => page.Initialized += x,
                                                                 x => page.Initialized -= x);

        var disappearing = Observable.FromEvent<EventHandler, bool>(
                                                                    eventHandler =>
                                                                    {
                                                                        void Handler(object? sender, EventArgs e) => eventHandler(false);
                                                                        return Handler;
                                                                    },
                                                                    x => page.Disposing += x,
                                                                    x => page.Disposing -= x);

        return appearing.Merge(disappearing);
    }


    private static IObservable<bool>? GetActivationFor(View? view)
    {
        if (view is null)
        {
            return null;
        }

        var loaded = Observable.FromEvent<EventHandler, bool>(
                                                                eventHandler =>
                                                                {
                                                                    void Handler(object? sender, EventArgs e) => eventHandler(true);
                                                                    return Handler;
                                                                },
                                                                x => view.Initialized += x,
                                                                x => view.Initialized -= x);

        var unloaded = Observable.FromEvent<EventHandler, bool>(
                                                                  eventHandler =>
                                                                  {
                                                                      void Handler(object? sender, EventArgs e) => eventHandler(false);
                                                                      return Handler;
                                                                  },
                                                                  x => view.Disposing += x,
                                                                  x => view.Disposing -= x);

        return loaded
               .Merge(unloaded)
               .StartWith(view.IsInitialized)
               .DistinctUntilChanged();
    }

    
}