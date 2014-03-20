///
/// From http://www.codeproject.com/Articles/12870/Don-t-Flicker-Double-Buffer
/// 

using System;
using System.Drawing;
using System.Windows.Forms;

namespace DotNetControlsEx
{
    public partial class ManualDoubleBufferedControl : Control
    {
        const BufferedGraphics NO_MANAGED_BACK_BUFFER = null;

        BufferedGraphicsContext GraphicManager;
        BufferedGraphics ManagedBackBuffer;

        public ManualDoubleBufferedControl()
        {
            InitializeComponent();

            Application.ApplicationExit +=
               new EventHandler(MemoryCleanup);

            //Set this style to achieve double buffering correctly
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            GraphicManager = BufferedGraphicsManager.Current;
            GraphicManager.MaximumBuffer =
                   new Size(this.Width + 1, this.Height + 1);
            ManagedBackBuffer =
                GraphicManager.Allocate(this.CreateGraphics(),
                                               ClientRectangle);

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        private void MemoryCleanup(object sender, EventArgs e)
        {
            // clean up the memory
            if (ManagedBackBuffer != NO_MANAGED_BACK_BUFFER)
                ManagedBackBuffer.Dispose();
        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            // we draw the progressbar into the image in the memory
            DoPaint(ManagedBackBuffer.Graphics);

            // now we draw the image into the screen
            ManagedBackBuffer.Render(pe.Graphics);
        }

        private void DoubleBufferedControl_Resize(object sender,
                                                      EventArgs e)
        {
            if (ManagedBackBuffer != NO_MANAGED_BACK_BUFFER)
                ManagedBackBuffer.Dispose();

            GraphicManager.MaximumBuffer =
                  new Size(this.Width + 1, this.Height + 1);

            ManagedBackBuffer =
                GraphicManager.Allocate(this.CreateGraphics(),
                                                ClientRectangle);

            this.Refresh();
        }

        protected virtual void DoPaint(Graphics graphics) { }
    }
}
