using System.Text;
using Terminal.Gui.Text;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace usbprison
{
    public class CollectionDays : Collection<DailyScheduleViewModel>
    {
        public CollectionDays(DynamicData.Binding.ObservableCollectionExtended<DailyScheduleViewModel> items)
            : base(items)
        {
        }

        public override void Render(ListView container, bool selected, int item, int col, int row, int width, int viewportX = 0)
        {
            container.Move(Math.Max(col - viewportX, 0), row);

            if (_items is null)
            {
                return;
            }

            DailyScheduleViewModel? t = _items[item];

            if (t is null)
            {
                RenderString(container, "", col, row, width);
            }
            else
            {
                RenderString(container,_toStringFunc != null ? _toStringFunc(t) : t.ToString()!, col, row, width, viewportX);
            }
        }

        private static void RenderString(Terminal.Gui.ViewBase.View driver, string str, int col, int row, int width, int viewportX = 0)
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

    }
}