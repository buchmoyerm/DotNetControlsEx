using System.Drawing;
using System.Windows.Forms;


namespace DotNetControlsEx
{
    public class TextBoxEx : TextBox
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if ( this.BackColor.GetBrightness() >= 0.5 )
            {
                PaintPopoutHandle(Color.Black);
            }
            else
            {
                PaintPopoutHandle(Color.LightGray);
            }
        }

        private void PaintPopoutHandle(Color color)
        {
            var r = ClientRectangle;
        }
    }
}
