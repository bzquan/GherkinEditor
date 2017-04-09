using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Gherkin.View
{
    public class AutoFocusPopup : Popup
    {
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            // move the focus into the popup when it opens.
            this.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        protected override void OnLostKeyboardFocus(
              KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            // if the focus is still inside the popup, 
            // don't do anything.
            if (this.IsKeyboardFocusWithin)
                return;

            // if the popup is still open, keep the focus inside.
            if (this.IsOpen)
            {
                this.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
