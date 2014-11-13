using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DotNetControlsEx.Properties;

// This control is a combination of two other collapsiblepanels
// http://jfblier.wordpress.com/2011/02/16/window-form-expander/
// http://www.codeproject.com/Articles/53318/C-Custom-Control-Featuring-a-Collapsible-Panel

namespace DotNetControlsEx
{
    [Designer(typeof(CollapsiblePanelExDesigner))]
    [ToolboxBitmap(typeof(CollapsiblePanel), "DotNetControlsEx.CollapsiblePanel.bmp")]
    [DefaultProperty("HeaderText")]
    public partial class CollapsiblePanelEx : Panel
    {
        #region "Private members"

        private bool collapse = false;
        private int originalHight = 0;
        private bool useAnimation;
        private bool showHeaderSeparator = true;
        private bool roundedCorners;
        private int headerCornersRadius = 10;
        private RectangleF toolTipRectangle = new RectangleF();
        private bool useToolTip = false;

        #endregion

        #region Events

        public event EventHandler StateChanged;
        public event CancelEventHandler StateChanging;

        #endregion

        #region "Public Properties"

        //public new Color BackColor
        //{
        //    get { return base.BackColor; }
        //    set 
        //    { 
        //        base.BackColor = value;
        //        Header.BackColor = value;
        //    }
        //}

        [DefaultValue(false)]
        [Description("Collapses the control when set to true")]
        [Category("CollapsiblePanel")]
        public bool Collapse
        {
            get { return collapse; }
            set
            {
                // If using animation make sure to ignore requests for collapse or expand while a previous
                // operation is in progress.
                if (useAnimation)
                {
                    // An operation is already in progress.
                    if (timerAnimation.Enabled)
                    {
                        return;
                    }
                }

                if (collapse != value)
                {
                    if (StateChanging != null)
                    {
                        CancelEventArgs args = new CancelEventArgs();
                        StateChanging(this, args);
                        if (args.Cancel)
                            return;
                    }
                    collapse = value;
                    CollapseOrExpand();
                    Refresh();
                }
            }
        }

        [DefaultValue(25)]
        [Category("CollapsiblePanel")]
        [Description("Specifies the speed (in ms) of Expand/Collapse operation when using animation. UseAnimation property must be set to true.")]
        public int AnimationInterval
        {
            get
            {
                return timerAnimation.Interval;
            }
            set
            {
                // Update animation interval only during idle times.
                if (!timerAnimation.Enabled)
                    timerAnimation.Interval = value;
            }
        }

        [DefaultValue(false)]
        [Category("CollapsiblePanel")]
        [Description("Indicate if the panel uses amination during Expand/Collapse operation")]
        public bool UseAnimation
        {
            get { return useAnimation; }
            set { useAnimation = value; }
        }

        [DefaultValue(true)]
        [Category("CollapsiblePanel")]
        [Description("When set to true draws panel borders, and shows a line separating the panel's header from the rest of the control")]
        public bool ShowHeaderSeparator
        {
            get { return showHeaderSeparator; }
            set
            {
                showHeaderSeparator = value;
                Refresh();
            }
        }

        [DefaultValue(false)]
        [Category("CollapsiblePanel")]
        [Description("When set to true, draws a panel with rounded top corners, the radius can bet set through HeaderCornersRadius property")]
        public bool RoundedCorners
        {
            get
            {
                return roundedCorners;
            }
            set
            {
                roundedCorners = value;
                Refresh();
            }
        }


        [DefaultValue(10)]
        [Category("CollapsiblePanel")]
        [Description("Top corners radius, it should be in [1, 15] range")]
        public int HeaderCornersRadius
        {
            get
            {
                return headerCornersRadius;
            }

            set
            {
                if (value < 1 || value > 15)
                    throw new ArgumentOutOfRangeException("HeaderCornersRadius", value, "Value should be in range [1, 90]");
                else
                {
                    headerCornersRadius = value;
                    Refresh();
                }
            }
        }

        [Category("CollapsiblePanel Default Header")]
        [Description("Background color of default header control.")]
        public Color HeaderBackColor
        {
            get { return defaultHeader.BackColor; }
            set { defaultHeader.BackColor = value; }
        }

        [Category("CollapsiblePanel Default Header")]
        [Description("Enables the automatic handling of text that extends beyond the width of the label control.")]
        public bool HeaderTextAutoEllipsis
        {
            get { return defaultHeader.TextAutoEllipsis; }
            set
            {
                defaultHeader.TextAutoEllipsis = value;
                //Refresh();
            }
        }

        [Category("CollapsiblePanel Default Header")]
        [Description("Text to show in panel's header (left justified)")]
        public string LeftHeaderText
        {
            get { return defaultHeader.LeftText; }
            set
            {
                defaultHeader.LeftText = value;
                //Refresh();
            }
        }

        [Category("CollapsiblePanel Default Header")]
        [Description("Text to show in panel's header (centered)")]
        public string CenterHeaderText
        {
            get { return defaultHeader.CenterText; }
            set
            {
                defaultHeader.CenterText = value;
                //Refresh();
            }
        }

        [Category("CollapsiblePanel Default Header")]
        [Description("Text to show in panel's header (right justified)")]
        public string RightHeaderText
        {
            get { return defaultHeader.RightText; }
            set
            {
                defaultHeader.RightText = value;
                //Refresh();
            }
        }

        [Category("CollapsiblePanel Default Header")]
        [Description("Color of text header, and panel's borders when ShowHeaderSeparator is set to true")]
        public Color HeaderTextColor
        {
            get { return defaultHeader.ForeColor; }
            set
            {
                defaultHeader.ForeColor = value;
                Refresh();
            }
        }


        [Category("CollapsiblePanel Default Header")]
        [Description("Image that will be displayed in the top left corner of the panel")]
        public Image HeaderImage
        {
            get { return defaultHeader.LeftImage; }
            set
            {
                defaultHeader.LeftImage = value;
                Refresh();
            }
        }


        [Category("CollapsiblePanel Default Header")]
        [Description("The font used to display text in the panel's header.")]
        public Font HeaderFont
        {
            get { return defaultHeader.Font; }
            set
            {
                defaultHeader.Font = value;
                //Refresh();
            }
        }

        #endregion 

        private CollapsiblePanelHeader defaultHeader;
        private Control header;
        public Control Header
        {
            get { return this.header; }

            set
            {
                if (this.header != null)
                {
                    this.Controls.Remove(this.header);
                }

                this.header = value;
                this.header.Dock = DockStyle.Top;
                this.header.Click += delegate { this.Toggle(); };
                this.Controls.Add(this.header);
                this.Controls.SetChildIndex(this.header, this.Controls.Count > 1 ? 1 : 0);
            }
        }

        public CollapsiblePanelEx()
        {
            defaultHeader = new CollapsiblePanelHeader();
            this.StateChanged += (sender, args) => SetDefaultExpandCollapseImage();
            Header = defaultHeader;
            InitializeComponent();
            SetDefaultExpandCollapseImage();
        }

        private void SetDefaultExpandCollapseImage()
        {
            defaultHeader.RightImage =
                        (collapse ? Resources.ExpandLight : Resources.CollapseLight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawHeaderSeparater(e);
            //DrawHeaderPanel(e);
        }

        public void DrawHeaderCorners(Graphics g, Brush brush, float x, float y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();

            gp.AddLine(x + radius, y, x + width - (radius * 2), y); // Line
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90); // Corner
            gp.AddLine(x + width, y + radius, x + width, y + height); // Line
            gp.AddLine(x + width, y + height, x, y + height); // Line
            gp.AddLine(x, y + height, x, y + radius); // Line
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90); // Corner
            gp.CloseFigure();
            g.FillPath(brush, gp);
            if (showHeaderSeparator)
            {
                g.DrawPath(new Pen(ForeColor), gp);
            }
            gp.Dispose();
        }

        public void DrawHeaderSeparater(PaintEventArgs e)
        {
            // Draw header separator
            if (showHeaderSeparator)
            {
                Point start = new Point(Header.Location.X, Header.Location.Y + Header.Height);
                Point end = new Point(Header.Location.X + Header.Width, Header.Location.Y + Header.Height);
                e.Graphics.DrawLine(new Pen(ForeColor, 2), start, end);
                // Draw rectangle lines for the rest of the control.
                //Rectangle bodyRect = this.ClientRectangle;
                //bodyRect.Y += this.Header.Height;
                //bodyRect.Height -= (this.Header.Height + 1);
                //bodyRect.Width -= 1;
                //e.Graphics.DrawRectangle(new Pen(ForeColor), bodyRect);
            }
        }

        //private void DrawHeaderPanel(PaintEventArgs e)
        //{
        //    Rectangle headerRect = pnlHeader.ClientRectangle;
        //    LinearGradientBrush headerBrush = new LinearGradientBrush(
        //        headerRect, Color.Snow, Color.LightBlue, LinearGradientMode.Horizontal);

        //    if (!roundedCorners)
        //    {
        //        e.Graphics.FillRectangle(headerBrush, headerRect);
        //        if (showHeaderSeparator)
        //        {
        //            e.Graphics.DrawRectangle(new Pen(ForeColor), headerRect);
        //        }
        //    }
        //    else
        //        DrawHeaderCorners(e.Graphics, headerBrush, headerRect.X, headerRect.Y,
        //            headerRect.Width, headerRect.Height, headerCornersRadius);


        //    // Draw header separator
        //    if (showHeaderSeparator)
        //    {
        //        Point start = new Point(pnlHeader.Location.X, pnlHeader.Location.Y + pnlHeader.Height);
        //        Point end = new Point(pnlHeader.Location.X + pnlHeader.Width, pnlHeader.Location.Y + pnlHeader.Height);
        //        e.Graphics.DrawLine(new Pen(ForeColor, 2), start, end);
        //        // Draw rectangle lines for the rest of the control.
        //        Rectangle bodyRect = this.ClientRectangle;
        //        bodyRect.Y += this.pnlHeader.Height;
        //        bodyRect.Height -= (this.pnlHeader.Height + 1);
        //        bodyRect.Width -= 1;
        //        e.Graphics.DrawRectangle(new Pen(ForeColor), bodyRect);
        //    }

        //    int headerRectHeight = pnlHeader.Height;
        //    // Draw header image.
        //    if (headerImage != null)
        //    {
        //        pictureBoxImage.Image = headerImage;
        //        pictureBoxImage.Visible = true;
        //    }
        //    else
        //    {
        //        pictureBoxImage.Image = null;
        //        pictureBoxImage.Visible = false;
        //    }


        //    // Calculate header string position.
        //    if (showHeaderText)
        //    {
        //        useToolTip = false;
        //        int delta = pictureBoxExpandCollapse.Width + 5;
        //        int offset = 0;
        //        if (headerImage != null)
        //        {
        //            offset = headerRectHeight;
        //        }
        //        PointF headerTextPosition = new PointF();
        //        Size headerTextSize = TextRenderer.MeasureText(leftHeaderText, headerFont);
        //        if (headerTextAutoEllipsis)
        //        {
        //            if (headerTextSize.Width >= headerRect.Width - (delta + offset))
        //            {
        //                RectangleF rectLayout =
        //                    new RectangleF((float)headerRect.X + offset,
        //                    (float)(headerRect.Height - headerTextSize.Height) / 2,
        //                    (float)headerRect.Width - delta,
        //                    (float)headerTextSize.Height);
        //                StringFormat format = new StringFormat();
        //                format.Trimming = StringTrimming.EllipsisWord;
        //                e.Graphics.DrawString(leftHeaderText, headerFont, new SolidBrush(headerTextColor),
        //                    rectLayout, format);

        //                toolTipRectangle = rectLayout;
        //                useToolTip = true;
        //            }
        //            else
        //            {
        //                headerTextPosition.X = (offset + headerRect.Width - headerTextSize.Width) / 2;
        //                headerTextPosition.Y = (headerRect.Height - headerTextSize.Height) / 2;
        //                e.Graphics.DrawString(leftHeaderText, headerFont, new SolidBrush(headerTextColor),
        //                    headerTextPosition);
        //            }
        //        }
        //        else
        //        {
        //            headerTextPosition.X = (offset + headerRect.Width - headerTextSize.Width) / 2;
        //            headerTextPosition.Y = (headerRect.Height - headerTextSize.Height) / 2;
        //            e.Graphics.DrawString(leftHeaderText, headerFont, new SolidBrush(headerTextColor),
        //                headerTextPosition);
        //        }

        //    }
        //}

        private void Toggle()
        {
            Collapse = !Collapse;
        }

        private void CollapseOrExpand()
        {
            if (!useAnimation)
            {
                if (collapse)
                {
                    originalHight = this.Height;
                    this.Height = header.Height + 3;
                }
                else
                {
                    this.Height = originalHight;
                }

                if (StateChanged != null)
                    StateChanged(this, null);
            }
            else
            {

                // Keep original height only in case of a collapse operation.
                if (collapse)
                    originalHight = this.Height;

                timerAnimation.Enabled = true;
                timerAnimation.Start();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Header.Width = this.Width - 1;
            Refresh();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Header.Width = this.Width - 1;
            Refresh();
        }

        private void timerAnimation_Tick(object sender, EventArgs e)
        {
            if (collapse)
            {
                if (this.Height <= header.Height + 3)
                {
                    timerAnimation.Stop();
                    timerAnimation.Enabled = false;
                }
                else
                {
                    int newHight = this.Height - 25;
                    if (newHight <= header.Height + 3)
                        newHight = header.Height + 3;
                    this.Height = newHight;
                }


            }
            else
            {
                if (this.Height >= originalHight)
                {
                    timerAnimation.Stop();
                    timerAnimation.Enabled = false;
                }
                else
                {
                    int newHeight = this.Height + 25;
                    if (newHeight >= originalHight)
                        newHeight = originalHight;
                    this.Height = newHeight;
                }
            }

            if (StateChanged != null)
                StateChanged(this, null);
        }
    }

    public class CollapsiblePanelHeader : Control
    {
        protected Label _leftJustifiedLabel;
        protected Label _rightJustifiedLabel;
        protected Label _centerJustifiedLabel;

        private Color _savedBackColor;

        private bool _autoElipses;

        public CollapsiblePanelHeader()
        {
            _leftJustifiedLabel = new Label();
            _rightJustifiedLabel = new Label();
            _centerJustifiedLabel = new Label();

            _leftJustifiedLabel.TextAlign = ContentAlignment.MiddleRight;
            _leftJustifiedLabel.ImageAlign = ContentAlignment.MiddleLeft;

            _centerJustifiedLabel.TextAlign = ContentAlignment.MiddleCenter;
            _centerJustifiedLabel.AutoSize = true;

            _rightJustifiedLabel.TextAlign = ContentAlignment.MiddleLeft;
            _rightJustifiedLabel.ImageAlign = ContentAlignment.MiddleRight;

            this.Controls.Add(_leftJustifiedLabel);
            this.Controls.Add(_centerJustifiedLabel);
            this.Controls.Add(_rightJustifiedLabel);

            PositionLabels();

            _savedBackColor = BackColor;
        }

        public bool TextAutoEllipsis
        {
            get { return _autoElipses; }
            set { _autoElipses = value; }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PositionLabels();
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            PositionLabels();
        }

        public override Font Font
        {
            get { return base.Font; }
            set
            {
                base.Font = value;
                base.Height = value.Height + 10; //update the height based on the font value
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            e.Control.Click += Control_Click;
            base.OnControlAdded(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _savedBackColor = BackColor;
            base.OnMouseEnter(e);

            BackColor = Utils.ColorUtil.LightenColor(BackColor, 0.1);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            BackColor = _savedBackColor;
            base.OnMouseLeave(e);
        }

        void Control_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void PositionLabels()
        {
            //set left ctrl on the left
            var lRect = _leftJustifiedLabel.ClientRectangle;
            lRect.Offset(0 - lRect.Left, 0);
            VCenterRect(ref lRect);
            _leftJustifiedLabel.SetBounds(lRect.Left, lRect.Top, lRect.Width, lRect.Height);

            //set right ctrol on the right
            var rRect = _rightJustifiedLabel.ClientRectangle;
            rRect.Offset( (Width - rRect.Width) - rRect.Left, 0);
            VCenterRect(ref rRect);
            _rightJustifiedLabel.SetBounds(rRect.Left, rRect.Top, rRect.Width, rRect.Height);

            if (! string.IsNullOrEmpty(_centerJustifiedLabel.Text))
            {
                var cRect = _centerJustifiedLabel.ClientRectangle;
                HCenterRect(ref cRect);
                VCenterRect(ref cRect);
                _centerJustifiedLabel.SetBounds(cRect.Left, cRect.Top, cRect.Width, cRect.Height);
            }
        }

        private void VCenterCtrl(Control ctrl)
        {
            var ctrlRect = ctrl.ClientRectangle;
            VCenterRect(ref ctrlRect);
            ctrl.SetBounds(ctrlRect.Left, ctrlRect.Top, ctrlRect.Width, ctrlRect.Height);
        }

        private void HCenterCtrl(Control ctrl)
        {
            var ctrlRect = ctrl.ClientRectangle;
            HCenterRect(ref ctrlRect);
            ctrl.SetBounds(ctrlRect.Left, ctrlRect.Top, ctrlRect.Width, ctrlRect.Height);
        }

        private void VCenterRect(ref Rectangle rect)
        {
            int topPos = (Height / 2) - (rect.Height / 2);
            rect.Offset(0, topPos - rect.Top);
        }

        private void HCenterRect(ref Rectangle rect)
        {
            int leftPos = (Width / 2) - (rect.Width / 2);
            rect.Offset(leftPos - rect.Left, 0);
        }

        public Color TextColor
        {
            get { return ForeColor; }
            set { ForeColor = value; }
        }

        public Image RightImage
        {
            get { return _rightJustifiedLabel.Image; }
            set
            {
                _rightJustifiedLabel.Image = value;
                UpdateLabeleSize(_rightJustifiedLabel);
            }
        }

        public Image LeftImage
        {
            get { return _leftJustifiedLabel.Image; }
            set
            {
                _leftJustifiedLabel.Image = value;
                UpdateLabeleSize(_leftJustifiedLabel);
            }
        }

        public string LeftText
        {
            get { return _leftJustifiedLabel.Text; }
            set
            {
                _leftJustifiedLabel.Text = value;
                UpdateLabeleSize(_leftJustifiedLabel);
            }
        }

        public string RightText
        {
            get { return _rightJustifiedLabel.Text; }
            set
            {
                _rightJustifiedLabel.Text = value;
                UpdateLabeleSize(_rightJustifiedLabel);
            }
        }

        public string CenterText
        {
            get { return _centerJustifiedLabel.Text; }
            set { _centerJustifiedLabel.Text = value; }
        }

        private void UpdateLabeleSize(Label lbl)
        {
            int gap = 5;
            lbl.AutoSize = true;
            if (lbl.Image != null)
            {
                int autoWidth = lbl.Width;
                lbl.AutoSize = false;
                lbl.Width = autoWidth + gap + lbl.Image.Width;
            }
            PositionLabels();
        }
    }
}

namespace Utils
{
}
