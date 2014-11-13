using System;
using System.Linq;
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

        public bool AllowLower { get; set; }
        public bool AllowUpper { get; set; }
        public bool AllowDigits { get; set; }
        public bool AllowSymbols { get; set; }
        public bool AllowPunctuation { get; set; }
        public bool AllowWhiteSpace { get; set; }

        public bool AllowChars
        {
            get { return AllowUpper || AllowLower; }
            set
            {
                if (value)
                {
                    if (! (AllowUpper || AllowLower))
                    {
                        AllowLower = AllowUpper = true;
                    }
                }
                else
                {
                    AllowLower = AllowUpper = false;
                }
            }
        }

        public TextBoxEx()
        {
            AllowLower = true;
            AllowUpper = true;
            AllowDigits = true;
            AllowSymbols = true;
            AllowPunctuation = true;
            AllowWhiteSpace = true;
        }
 
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
            //remove characters not allowed
            var remove = pText.Where(c => !IsCharAllowed(c)).Distinct();
            pText = remove.Aggregate(pText, (current, c) => current.Replace("" + c, ""));

            //transform characters that are allowed
            var allowed = pText.Distinct();
            pText = allowed.Aggregate(pText, (current, c) => current.Replace("" + c, "" + Transform(c)));
            
            var args = new PasteEventArgs(pText);
            var eHandler = TextPasted;
            if (eHandler != null)
            {
                eHandler(this, args);
            }
            this.SelectedText = args.PastedText;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (IsCharAllowed(e.KeyChar))
            {
                e.KeyChar = Transform(e.KeyChar);
            }
            else
            {
                e.Handled = true;
            }
            base.OnKeyPress(e);
        }

        private bool IsCharAllowed(char c)
        {
            if (Char.IsControl(c)) return true;
            if (Char.IsDigit(c)) return AllowDigits;
            if (Char.IsLetter(c)) return AllowChars;
            if (Char.IsSymbol(c)) return AllowSymbols;
            if (Char.IsPunctuation(c)) return AllowPunctuation;
            if (Char.IsWhiteSpace(c)) return AllowWhiteSpace;

            return true;
        }

        private char Transform(char c)
        {
            if (! Char.IsLetter(c)) return c;

            if (AllowLower && !AllowUpper)
            {
                return Char.ToLower(c);
            }
            if (AllowUpper && ! AllowLower)
            {
                return Char.ToUpper(c);
            }
            return c;
        }
    }
}
