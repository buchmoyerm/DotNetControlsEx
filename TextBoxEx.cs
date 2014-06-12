using System;
using System.Drawing;
using System.Windows.Forms;


namespace DotNetControlsEx
{
    public class PasteEventArgs : EventArgs
    {
        public PasteEventArgs(string pastedText)
        {
            PastedText = pastedText;
        }

        public string PastedText { get; set; }
    }

    public class TextBoxEx : TextBox
    {
        public event EventHandler TextPasted;
 
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                // Trap WM_PASTE:
                case (int) NativeMethods.WindowMessages.WM_PASTE:
                    if (Clipboard.ContainsText())
                    {
                        DoPaste(Clipboard.GetText());
                        return;
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        private void DoPaste(string pText)
        {
            var args = new PasteEventArgs(pText);
            var eHandler = TextPasted;
            if (eHandler != null)
            {
                eHandler(this, args);
            }
            this.SelectedText = args.PastedText;
        }
    }
}
