using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace usbprison.maui.Pages
{
    public class ViewBase<TViewModel> : ReactiveUI.Maui.ReactiveContentView<TViewModel> where TViewModel : class, ReactiveUI.IReactiveObject
    {
        public ViewBase()
        {
            BindingContext = ViewModel;

            this.WhenActivated(disposables =>
            {
                //if (ViewModel != null)
                //{
                //    BindingContext = ViewModel;
                //}
            });
        }
    }
}
