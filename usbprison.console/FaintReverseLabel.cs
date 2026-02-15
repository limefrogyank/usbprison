using Terminal.Gui.Drawing;
using Terminal.Gui.Views;
using Attribute = Terminal.Gui.Drawing.Attribute;

public class FaintReverseLabel : Terminal.Gui.Views.Label
    {
        public FaintReverseLabel()
        {
            var attr = new Attribute (this.GetAttributeForRole (VisualRole.Normal))
            {
                Style =  TextStyle.Bold | TextStyle.Faint | TextStyle.Reverse
            };
                
            this.SetScheme(new Scheme{Normal = attr});
        }
    }