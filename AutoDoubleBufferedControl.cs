///
/// From http://www.codeproject.com/Articles/12870/Don-t-Flicker-Double-Buffer
/// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DotNetControlsEx
{
    public partial class AutoDoubleBufferedControl : Control
    {
        public AutoDoubleBufferedControl()
        {
            InitializeComponent();

            //control styles used for double buffering
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer, true);

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // we draw the progressbar normally with 
            // the flags sets to our settings
            DoPaint(pe.Graphics);
        }

        protected virtual void DoPaint(Graphics graphics) { }
    }
}
