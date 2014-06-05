using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;

namespace DotNetControlsEx
{
    public partial class ButtonOption : Control
    {
        public ButtonOption()
        {
            InitializeComponent();
        }

        public Action<uint> SelectionChanged;

        private List<string> _options = new List<string>();
        private List<SelectableButton> _buttons = new List<SelectableButton>(); 
        private uint _selectedOption = 0;
        private Color _selectedColor = Color.White;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string[] Options
        {
            get { return _options.ToArray(); }
            set
            {
                if (value != null)
                {
                    _options = value.ToList();
                    Reset();
                }
            }
        }

        public uint SelectedOption
        {
            get { return _selectedOption; }
            set
            {
                if (value < _options.Count)
                {
                    _selectedOption = value;
                    Reset();
                }
            }
        }

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                Reset();
            }
        }

        private void Reset()
        {
            if (_options.Count != _buttons.Count)
            {
                _buttons.Clear();
                this.Controls.Clear();
                for (int i = 0; i < _options.Count; ++ i)
                {
                    var btn = new SelectableButton();
                    btn.Click += BtnClicked;
                    _buttons.Add(btn);
                    this.Controls.Add(btn);
                }
            }

            if (_buttons.Count > 0)
            {
                int wBtn = (ClientRectangle.Width - 1)/_buttons.Count;
                int hBtn = ClientRectangle.Height - 1;

                for (int i = 0; i < _buttons.Count; ++i)
                {
                    var btn = _buttons[i];
                    btn.Font = Font;
                    btn.Text = _options[i];
                    btn.SetBounds(ClientRectangle.Left + wBtn*i,ClientRectangle.Top, wBtn, hBtn);
                    btn.Selected = _selectedOption == i;
                    btn.BackColor = this.BackColor;
                    btn.ForeColor = this.ForeColor;
                    btn.SelectedColor = this.SelectedColor;
                }
            }

            Invalidate();
        }

        private void BtnClicked(object sender, EventArgs e)
        {
            var btn = sender as SelectableButton;
            if (btn != null)
            {
                var sel = _buttons.IndexOf(btn);
                if (sel >= 0)
                {
                    if (_selectedOption != (uint) sel)
                    {
                        _selectedOption = (uint) sel;
                        Reset();

                        var t = SelectionChanged;
                        if (t != null)
                        {
                            t(_selectedOption);
                        }
                    }
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Reset();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Reset();
        }

        protected override void OnForeColorChanged(EventArgs e)
        {
            base.OnForeColorChanged(e);
            Reset();
        }
    }

    public class SelectableButton : AutoDoubleBufferedControl
    {
        private bool _selected = false;
        private Color _selectedColor = Color.White;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                Invalidate();
            }
        }

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                Invalidate();
            }
        }

        protected override void DoPaint(Graphics graphics)
        {
            var rect = ClientRectangle;
            rect.Height -= 1;
            rect.Width -= 1;
            using (var brush = new SolidBrush(this.BackColor))
            {
                graphics.FillRectangle(brush, rect);
            }
            using (var pen = new Pen(this.ForeColor))
            {
                graphics.DrawRectangle(pen, rect);
            }

            if (!_selected)
            {
                using (var brush = new SolidBrush(this.ForeColor))
                {
                    DrawText(graphics, brush, rect);
                }
            }
            else
            {
                rect.Inflate(-1, -1);
                using (var brush = new SolidBrush(this.SelectedColor))
                using (var pen = new Pen(this.SelectedColor, (float)1.3))
                using (var selectPath = CreateSelectedRect(rect, 3))
                {
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.DrawPath(pen, selectPath);
                    DrawText(graphics, brush, rect);
                }
            }
        }

        private GraphicsPath CreateSelectedRect(Rectangle rect, int radius)
        {
            int xw = rect.Left + rect.Width;
            int yh = rect.Top + rect.Height;
            int xwr = xw - radius;
            int yhr = yh - radius;
            int xr = rect.Left + radius;
            int yr = rect.Top + radius;
            int r2 = radius * 2;
            int xwr2 = xw - r2;
            int yhr2 = yh - r2;

            GraphicsPath p = new GraphicsPath();
            p.StartFigure();

            //Top Left Corner
            p.AddArc(rect.Left, rect.Top, r2, r2, 180, 90);

            //Top Edge
            p.AddLine(xr, rect.Top, xwr, rect.Top);

            //Top Right Corner
            p.AddArc(xwr2, rect.Top, r2, r2, 270, 90);

            //Right Edge
            p.AddLine(xw, yr, xw, yhr);

            //Bottom Right Corner
            p.AddArc(xwr2, yhr2, r2, r2, 0, 90);

            //Bottom Edge
            p.AddLine(xwr, yh, xr, yh);

            //Bottom Left Corner
            p.AddArc(rect.Left, yhr2, r2, r2, 90, 90);

            //Left Edge
            p.AddLine(rect.Left, yhr, rect.Left, yr);

            p.CloseFigure();
            return p;
        }

        private void DrawText(Graphics graphics, Brush brush, Rectangle rect)
        {
            float x;
            float y;

            int yMid = (rect.Top) + (rect.Height/2);
            int xMid = (rect.Left) + (rect.Width/2);

            y = yMid - (this.FontHeight / (float)2);
            x = xMid - (graphics.MeasureString(this.Text, this.Font).Width/(float) 2);

            graphics.DrawString(Text, this.Font, brush, x, y);
        }
    }
}
