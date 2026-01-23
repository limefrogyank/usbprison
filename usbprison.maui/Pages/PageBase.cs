using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace usbprison.maui.Pages
{
    public class PageBase<TViewModel> : ReactiveUI.Maui.ReactiveContentPage<TViewModel> where TViewModel : class, ReactiveUI.IReactiveObject
    {
        public PageBase() :base()
        {
            BindingContext = ViewModel;

            this.WhenActivated(disposables =>
            {
                if (ViewModel != null)
                {
                    
                }
            });
        }
    }
}
