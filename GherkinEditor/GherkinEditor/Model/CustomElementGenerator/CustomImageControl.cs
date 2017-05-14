using Gherkin.Util;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Model
{
    public class CustomImageControl : System.Windows.Controls.Image
    {
        public CustomImageControl(int cursorOffset, TextEditor editor)
        {
            CursorOffset = cursorOffset;
            TextEditor = editor;
            this.MouseDown += OnMouseDown;
        }

        public int CursorOffset { get; set; }
        public TextEditor TextEditor { get; set; }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            EventAggregator<CustomImageClickedArg>.Instance.Publish(this, new CustomImageClickedArg());
        }
    }
}
