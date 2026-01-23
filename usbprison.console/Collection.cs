using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Terminal.Gui.Views;
using Terminal.Gui.Text;
using Terminal.Gui.ViewBase;
using System.Text;
using Serilog;

namespace usbprison
{
    public class Collection<T> : IListDataSource
    {
        protected DynamicData.Binding.ObservableCollectionExtended<T> _items;
        protected int _count;
        protected BitArray? _marks;
        protected Func<T, string>? _toStringFunc;

        public Collection(DynamicData.Binding.ObservableCollectionExtended<T> items, Func<T, string>? toStringFunc = null)
        {
            Log.Information("Created a new collection");
            if (items != null)
            {
                _count = items.Count;
                _marks = new(_count);
                _items = items;
                _items.CollectionChanged += OnCollectionChanged;
                Length = GetMaxLengthItem();

                _toStringFunc = toStringFunc;
            }
            else
            {
                _items = new DynamicData.Binding.ObservableCollectionExtended<T>();
                _items.CollectionChanged += OnCollectionChanged;
                _count = 0;
            }
            //_items.CollectionChanged += CollectionChangedpublic;
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected int GetMaxLengthItem()
        {
            if (_items is null || _items?.Count == 0)
            {
                return 0;
            }

            var maxLength = 0;

            for (var i = 0; i < _items!.Count; i++)
            {
                object? t = _items[i];

                if (t is null)
                {
                    continue;
                }

                int l;

                l = t is string u ? u.GetColumns() : t.ToString()!.Length;

                if (l > maxLength)
                {
                    maxLength = l;
                }
            }

            return maxLength;
        }


        public int Count => _items.Count;

        public int Length { get; private set; }

        public bool SuspendCollectionChangedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void Dispose()
        {
            _items.CollectionChanged -= OnCollectionChanged;
        }

        public bool IsMarked(int item)
        {
            if (item >= 0 && item < _count)
            {
                return _marks![item];
            }

            return false;
        }

        public virtual void Render(ListView container, bool selected, int item, int col, int line, int width, int viewportX = 0)
        {
            container.Move(Math.Max(col - viewportX, 0), line);

            if (_items is null)
            {
                return;
            }

            T? t = _items[item];

            if (t is null)
            {
                RenderString(container, "", col, line, width);
            }
            else
            {
                if (t is string s)
                {
                    RenderString(container, s, col, line, width, viewportX);
                }
                else
                {
                    RenderString(container,_toStringFunc != null ? _toStringFunc(t) : t.ToString()!, col, line, width, viewportX);
                }
            }
        }

        private static void RenderString(Terminal.Gui.ViewBase.View driver, string str, int col, int line, int width, int viewportX = 0)
        {
            if (string.IsNullOrEmpty(str) || viewportX >= str.GetColumns())
            {
                // Empty string or viewport beyond string - just fill with spaces
                for (var i = 0; i < width; i++)
                {
                    driver.AddRune((Rune)' ');
                }

                return;
            }

            int runeLength = str.ToRunes().Length;
            int startIndex = Math.Min(viewportX, Math.Max(0, runeLength - 1));
            string substring = str.Substring(startIndex);
            string u = TextFormatter.ClipAndJustify(substring, width >=0 ? width : 0, Alignment.Start);
            driver.AddStr(u);
            width -= u.GetColumns();

            while (width-- > 0)
            {
                driver.AddRune((Rune)' ');
            }
        }

        public void SetMark(int item, bool value)
        {
            if (item >= 0 && item < _count)
            {
                _marks![item] = value;
            }
        }

        public IList ToList()
        {
            return _items.ToList();
        }
    }
}