using System.Text;
using Serilog;
using Terminal.Gui.Text;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace usbprison
{
    public class CollectionFlattened : Collection<FlatDeviceLogViewModel>
    {
        public CollectionFlattened(DynamicData.Binding.ObservableCollectionExtended<FlatDeviceLogViewModel> items)
            : base(items)
        {
        }

        public override void Render(ListView container, bool selected, int item, int col, int line, int width, int viewportX = 0)
        {
            //container.Move(Math.Max(col - viewportX, 0), line);

            if (_items is null)
            {
                return;
            }

            FlatDeviceLogViewModel? t = _items[item];

            if (t is null)
            {
                RenderString(container, "", col, line, width);
            }
            else
            {
                if (t.Log.MachineId == "__GROUP__")
                {
                    Log.Information("Rendering group header for device: " + t.Log.DeviceId);
                    RenderString(container, t.Name, col, line, width, viewportX);
                }
                else
                {
                    Log.Information("Rendering log items for device: " + t.Log.DeviceId);
                    RenderString(container, "    " + t.Log.Timestamp.ToLocalTime().ToString("G") + " - " + t.Log.Status, col, line, width, viewportX);
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

    }
}