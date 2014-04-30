// NotifyWindow.cs
// Copyright © 2004 by Robert Misiak <rmisiak@users.sourceforge.net>
// All Rights Reserved.
//
// Permission is granted to use, modify and distribute this code, as long as credit is given to the original author, and the copyright notice
// is retained.
//
// Based on a similar implementation used in ChronosXP, an open-source project:  http://chronosxp.sourceforge.net

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DotNetControlsEx
{
	/// <summary>
	/// An MSN-Messenger-Style NotifyWindow.
	/// </summary>
	public class NotifyWindow2000 : System.Windows.Forms.Form
	{
		#region Public Variables
		public string Title;
		public System.Drawing.Font TitleFont, HoverFont, TitleHoverFont;
		public enum BackgroundStyles { BackwardDiagonalGradient, ForwardDiagonalGradient, HorizontalGradient, VerticalGradient, Solid };
		public enum ClockStates { Opening, Closing, Showing, CloseReady, None };
		public ClockStates ClockState;
		public BackgroundStyles BackgroundStyle;
		public int ActualHeight, ActualWidth, MaxMinutes;
		public System.Drawing.Drawing2D.Blend Blend;
		public System.Drawing.StringFormat StringFormat;
		public bool Sticky, WaitOnMouseOver;
		public Color TitleColor = SystemColors.ControlText, TextColor = SystemColors.ControlText, GradientColor, PressedColor;
		public event System.EventHandler TextClicked;
		public event System.EventHandler TitleClicked;
		#endregion

		#region Protected Variables
		protected int hMouseHook = 0, hKeyboardHook = 0;
        protected NativeMethods.HOOKPROC hpMouse, hpKeyboard;
		protected bool closePressed = false, textPressed = false, titlePressed = false, closeHot = false, textHot = false, titleHot = false, waiting = false;
		protected Rectangle rClose, rText, rTitle, rDisplay, rScreen, rGlobClose, rGlobText, rGlobTitle, rGlobDisplay;
		protected System.Windows.Forms.Timer viewClock;
		protected DateTime svTime;
		#endregion

		#region Constructor
		public NotifyWindow2000 (string title, string text) : this() { Title = title; Text = text; }
		public NotifyWindow2000 (string text) : this() { Text = text; }
		public NotifyWindow2000()
		{
			SetStyle (ControlStyles.UserMouse, true);
			SetStyle (ControlStyles.UserPaint, true);
			SetStyle (ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.DoubleBuffer, true);

			ShowInTaskbar = false;
			FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			StartPosition = System.Windows.Forms.FormStartPosition.Manual;

			// Defaults
			BackgroundStyle = BackgroundStyles.VerticalGradient;
			ClockState = ClockStates.None;
			BackColor = Color.SteelBlue;
			GradientColor = Color.WhiteSmoke;
			PressedColor = Color.Gray;
			Sticky = true;
			WaitOnMouseOver = false;
			MaxMinutes = 30;
			ActualWidth = 130;
			ActualHeight = 110;
		}
		#endregion

		#region Public Methods
		public void SetDimensions (int width, int height)
		{
			ActualWidth = width;
			ActualHeight = height;
		}

		public void Notify()
		{
			if (Text == null || Text.Length < 1)
				throw new System.Exception ("You must set NotifyWindow.Text before calling Notify()");

            hpMouse = new NativeMethods.HOOKPROC(mouseHookProc);
            hMouseHook = NativeMethods.SetWindowsHookEx((int)NativeMethods.WindowsHooks.WH_MOUSE_LL, hpMouse,
				Marshal.GetHINSTANCE (Assembly.GetExecutingAssembly().GetModules()[0]), 0);
			if (Sticky)
			{
				hpKeyboard = new NativeMethods.HOOKPROC (keyboardHookProc);
                hKeyboardHook = NativeMethods.SetWindowsHookEx((int)NativeMethods.WindowsHooks.WH_KEYBOARD_LL, hpKeyboard,
					Marshal.GetHINSTANCE (Assembly.GetExecutingAssembly().GetModules()[0]), 0);
			}

			Width = ActualWidth;
			rScreen = Screen.GetWorkingArea (Screen.PrimaryScreen.Bounds);
			Height = 0;
			Top = rScreen.Bottom;
			Left = rScreen.Width - Width - 11;

			if (HoverFont == null)
				HoverFont = new Font (Font, FontStyle.Underline);
			if (TitleFont == null)
				TitleFont = Font;
			if (TitleHoverFont == null)
				TitleHoverFont = new Font (TitleFont, FontStyle.Underline);
			if (this.StringFormat == null)
			{
				this.StringFormat = new StringFormat();
				this.StringFormat.Alignment = StringAlignment.Center;
				this.StringFormat.LineAlignment = StringAlignment.Center;
				this.StringFormat.Trimming = StringTrimming.EllipsisWord;
			}

			rDisplay = new Rectangle (0, 0, Width, ActualHeight);
			rClose = new Rectangle (Width - 21, 10, 13, 13);

			int offset;
			if (Title != null)
			{
				using (Graphics fx = CreateGraphics())
				{
					SizeF sz = fx.MeasureString (Title, TitleFont, ActualWidth - rClose.Width - 22, this.StringFormat);
					rTitle = new Rectangle (11, 15, (int) Math.Ceiling (sz.Width), (int) Math.Ceiling (sz.Height));
					offset = (int) Math.Max (Math.Ceiling (sz.Height + rTitle.Top + 2), rClose.Bottom + 5);
				}
			}
			else
			{
				offset = rClose.Bottom + 1;
				rTitle = new Rectangle (-1, -1, 1, 1);
			}

			rText = new Rectangle (11, offset, ActualWidth - 22, ActualHeight - (int)(offset * 1.5));
			rGlobClose = rClose;
			rGlobClose.Offset (Left, rScreen.Bottom - ActualHeight);
			rGlobText = rText;
			rGlobText.Offset (Left, rScreen.Bottom - ActualHeight);
			rGlobTitle = rTitle;
			if (Title != null)
				rGlobTitle.Offset (Left, rScreen.Bottom - ActualHeight);
			rGlobDisplay = rDisplay;
			rGlobDisplay.Offset (Left, rScreen.Bottom - ActualHeight);
			rGlobClose = rClose;
			rGlobClose.Offset (Left, rScreen.Bottom - ActualHeight);
			rGlobDisplay = rDisplay;
			rGlobDisplay.Offset (Left, rScreen.Bottom - ActualHeight);

			// Use unmanaged ShowWindow() and SetWindowPos() instead of the managed Show() to display the window - this method will display
			// the window TopMost, but without stealing focus (namely the SW_SHOWNOACTIVATE and SWP_NOACTIVATE flags)
            NativeMethods.ShowWindow(Handle, (int)NativeMethods.ShowWindowOptions.SW_SHOWNOACTIVATE);
			NativeMethods.SetWindowPos (Handle, NativeMethods.HWND_TOPMOST, rScreen.Width - ActualWidth - 11, rScreen.Bottom, ActualWidth, 0, (int)NativeMethods.SetWindowPosOptions.SWP_NOACTIVATE);

			viewClock = new System.Windows.Forms.Timer();
			viewClock.Tick += new System.EventHandler (viewTimer);
			viewClock.Interval = 1;
			viewClock.Start();

			ClockState = ClockStates.Opening;
		}
		#endregion

		#region Drawing
		protected override void OnPaint (System.Windows.Forms.PaintEventArgs e)
		{
			drawCloseButton (e.Graphics);
			Font useFont;  Color useColor;
			if (Title != null)
			{
				if (titleHot)
					useFont = TitleHoverFont;
				else
					useFont = TitleFont;
				if (titlePressed)
					useColor = PressedColor;
				else
					useColor = TitleColor;
				using (SolidBrush sb = new SolidBrush (useColor))
					e.Graphics.DrawString (Title, useFont, sb, rTitle, this.StringFormat);
			}
			if (textHot)
				useFont = HoverFont;
			else
				useFont = Font;
			if (textPressed)
				useColor = PressedColor;
			else
				useColor = TextColor;
			using (SolidBrush sb = new SolidBrush (useColor))
				e.Graphics.DrawString (Text, useFont, sb, rText, this.StringFormat);
		}

		protected override void OnPaintBackground (System.Windows.Forms.PaintEventArgs e)
		{
			// First paint the background
			if (BackgroundStyle == BackgroundStyles.Solid)
			{
				using (SolidBrush sb = new SolidBrush (BackColor))
					e.Graphics.FillRectangle (sb, rDisplay);
			}
			else
			{
				LinearGradientMode lgm;
				switch (BackgroundStyle)
				{
					case BackgroundStyles.BackwardDiagonalGradient:
						lgm = LinearGradientMode.BackwardDiagonal;
						break;
					case BackgroundStyles.ForwardDiagonalGradient:
						lgm = LinearGradientMode.ForwardDiagonal;
						break;
					case BackgroundStyles.HorizontalGradient:
						lgm = LinearGradientMode.Horizontal;
						break;
					default:
					case BackgroundStyles.VerticalGradient:
						lgm = LinearGradientMode.Vertical;
						break;
				}
				using (LinearGradientBrush lgb = new LinearGradientBrush (rDisplay, GradientColor, BackColor, lgm))
				{
					if (this.Blend != null)
						lgb.Blend = this.Blend;
					e.Graphics.FillRectangle (lgb, rDisplay);
				}
			}

			// Next draw borders...
			drawBorder (e.Graphics);
		}

		protected virtual void drawBorder (Graphics fx)
		{
			fx.DrawRectangle (Pens.Silver, 2, 2, Width - 4, ActualHeight - 4);
			
			// Top border
			fx.DrawLine (Pens.Silver, 0, 0, Width, 0);
			fx.DrawLine (Pens.White, 0, 1, Width, 1);
			fx.DrawLine (Pens.DarkGray, 3, 3, Width - 4, 3);
			fx.DrawLine (Pens.DimGray, 4, 4, Width - 5, 4);

			// Left border
			fx.DrawLine (Pens.Silver, 0, 0, 0, ActualHeight);
			fx.DrawLine (Pens.White, 1, 1, 1, ActualHeight);
			fx.DrawLine (Pens.DarkGray, 3, 3, 3, ActualHeight - 4);
			fx.DrawLine (Pens.DimGray, 4, 4, 4, ActualHeight - 5);

			// Bottom border
			fx.DrawLine (Pens.DarkGray, 1, ActualHeight - 1, Width - 1, ActualHeight - 1);
			fx.DrawLine (Pens.White, 3, ActualHeight - 3, Width - 3, ActualHeight - 3);
			fx.DrawLine (Pens.Silver, 4, ActualHeight - 4, Width - 4, ActualHeight - 4);

			// Right border
			fx.DrawLine (Pens.DarkGray, Width - 1, 1, Width - 1, ActualHeight - 1);
			fx.DrawLine (Pens.White, Width - 3, 3, Width - 3, ActualHeight - 3);
			fx.DrawLine (Pens.Silver, Width - 4, 4, Width - 4, ActualHeight - 4);
		}

		protected virtual void drawCloseButton (Graphics fx)
		{
			if (VisualStylesEnabled())
				drawThemeCloseButton (fx);
			else
				drawLegacyCloseButton (fx);
		}

		protected void drawThemeCloseButton (Graphics fx)
		{
            IntPtr hTheme = NativeMethods.OpenThemeData(Handle, "Window");
			if (hTheme == IntPtr.Zero)
			{
				drawLegacyCloseButton (fx);
				return;
			}
			int stateId;
			if (closePressed)
				stateId = (int) NativeMethods.States.CBS_PUSHED;
			else if (closeHot)
                stateId = (int)NativeMethods.States.CBS_HOT;
			else
                stateId = (int)NativeMethods.States.CBS_NORMAL;
            NativeMethods.RECT reClose = NativeMethods.RECT.FromRectangle(rClose);
			NativeMethods.RECT reClip = reClose;
			IntPtr hDC = fx.GetHdc();
            NativeMethods.DrawThemeBackground(hTheme, hDC, (int) NativeMethods.WINDOWPARTS.WP_CLOSEBUTTON, stateId, ref reClose, ref reClip);
			fx.ReleaseHdc (hDC);
            NativeMethods.CloseThemeData(hTheme);
		}

		protected void drawLegacyCloseButton (Graphics fx)
		{
			ButtonState bState;
			if (closePressed)
				bState = ButtonState.Pushed;
			else // the Windows 95 theme doesn't have a "hot" button
				bState = ButtonState.Normal;
			ControlPaint.DrawCaptionButton (fx, rClose, CaptionButton.Close, bState);
		}

		
		protected bool VisualStylesEnabled()
		{
			try
			{
                if (NativeMethods.IsThemeActive() == 1)
					return true;
				else
					return false;
			}
			catch (System.DllNotFoundException)  // pre-XP systems which don't have UxTheme.dll
			{
				return false;
			}
		}
		#endregion

		#region Timers, Hooks and EventHandlers
		protected void appExit (object sender, System.EventArgs e)
		{
			Close();	// Make sure hooks are Unhook'ed if application exits while NotifyWindow is open.
		}

		protected void viewTimer (object sender, System.EventArgs e)
		{
			switch (ClockState)
			{
				case ClockStates.Opening:
					if (Top - 2 <= rScreen.Height - ActualHeight)
					{
						Top = rScreen.Height - ActualHeight;
						Height = ActualHeight;
						ClockState = ClockStates.Showing;
						svTime = DateTime.Now;
						if (hKeyboardHook != 0 && hMouseHook != 0 && Sticky)
							waiting = true;
						viewClock.Interval = 11000;
					}
					else
					{
						Top -= 2;
						Height += 2;
					}
					break;
				case ClockStates.Showing:
					if (!waiting || (MaxMinutes > 0 && DateTime.Now.CompareTo (svTime.AddMinutes (MaxMinutes)) > 0))
					{
						viewClock.Interval = 5000;
						ClockState = ClockStates.CloseReady;
					}
					break;
				case ClockStates.CloseReady:
					if (WaitOnMouseOver || !rGlobDisplay.Contains (Cursor.Position))
					{
						viewClock.Interval = 1;
						ClockState = ClockStates.Closing;
					}
					break;
				case ClockStates.Closing:
					Top += 2;
					Height -= 2;
					if (Top >= rScreen.Height)
					{
						ClockState = ClockStates.None;
						viewClock.Stop();
						viewClock.Dispose();
						Close();
					}
					break;
			}
		}

		protected int mouseHookProc (int nCode, int wParam, IntPtr lParam)
		{
			if (nCode == NativeMethods.HC_ACTION)
			{
				if (waiting)
					waiting = false;

				switch (wParam)
				{
					case (int)NativeMethods.WindowMessages.WM_MOUSEMOVE:
						if (Title != null && rGlobTitle.Contains (Cursor.Position) && !textPressed && !closePressed)
						{
							Cursor = Cursors.Hand;
							titleHot = true;
							textHot = false;  closeHot = false;
							Invalidate();
						}
						else if (rGlobText.Contains (Cursor.Position) && !titlePressed && !closePressed)
						{
							Cursor = Cursors.Hand;
							textHot = true;
							titleHot = false;  closeHot = false;
							Invalidate();
						}
						else if (rGlobClose.Contains (Cursor.Position) && !titlePressed && !textPressed)
						{
							Cursor = Cursors.Hand;
							closeHot = true;
							titleHot = false;  textHot = false;
							Invalidate();
						}
						else if ((textHot || titleHot || closeHot) && (!titlePressed && !textPressed && !closePressed))
						{
							Cursor = Cursors.Default;
							titleHot = false;  textHot = false;  closeHot = false;
							Invalidate();
						}
						break;

                    case (int)NativeMethods.WindowMessages.WM_LBUTTONDOWN:
						if (rGlobClose.Contains (Cursor.Position))
						{
							closePressed = true;
							closeHot = false;
							Invalidate();
						}
						else if (rGlobText.Contains (Cursor.Position))
						{
							textPressed = true;
							Invalidate();
						}
						else if (Title != null && rGlobTitle.Contains (Cursor.Position))
						{
							titlePressed = true;
							Invalidate();
						}
						break;

                    case (int)NativeMethods.WindowMessages.WM_LBUTTONUP:
						if (closePressed)
						{
							Cursor = Cursors.Default;
							closePressed = false;
							closeHot = false;
							Invalidate();
							if (rGlobClose.Contains (Cursor.Position))
								Close();
						}
						else if (textPressed)
						{
							Cursor = Cursors.Default;
							textPressed = false;
							textHot = false;
							Invalidate();
							if (rGlobText.Contains (Cursor.Position))
							{
								Close();
								if (TextClicked != null)
									TextClicked (this, new System.EventArgs());
							}
						}
						else if (titlePressed)
						{
							Cursor = Cursors.Default;
							titlePressed = false;
							titleHot = false;
							Invalidate();
							if (rGlobTitle.Contains (Cursor.Position))
							{
								Close();
								if (TitleClicked != null)
									TitleClicked (this, new System.EventArgs());
							}
						}
						break;
				}
			}
            return NativeMethods.CallNextHookEx(hMouseHook, nCode, wParam, lParam);
		}

		protected int keyboardHookProc (int nCode, int wParam, IntPtr lParam)
		{
			if (nCode == NativeMethods.HC_ACTION && waiting)
				waiting = false;
            return NativeMethods.CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
		}

		protected override void OnClosing (System.ComponentModel.CancelEventArgs e)
		{
			if (hMouseHook != 0)
                NativeMethods.UnhookWindowsHookEx(hMouseHook);
			if (hKeyboardHook != 0)
                NativeMethods.UnhookWindowsHookEx(hKeyboardHook);
			Application.ApplicationExit -= new System.EventHandler (appExit);
			base.OnClosing (e);
		}
		#endregion
	}
}
