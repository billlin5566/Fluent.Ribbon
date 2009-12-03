﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Fluent
{
    public class ExtendedPopup:Popup
    {     
        private bool ignoreNextDeactivate;

        #region Properties

        private bool IgnoreNextDeactivate
        {
            get { return ignoreNextDeactivate; }
            set
            {
                ignoreNextDeactivate = value;
                if ((ignoreNextDeactivate) && (ParentPopup != null) && (ParentPopup.IsOpen)) ParentPopup.IgnoreNextDeactivate = true;
            }
        }

        internal ExtendedPopup ParentPopup { get; set; }

        #endregion

        #region Constructors

        public ExtendedPopup()
        {
            IgnoreNextDeactivate = false;
        }

        #endregion

        #region Overrides
        
        protected override void OnOpened(EventArgs e)
        {            
            PopupAnimation = PopupAnimation.None;
            IntPtr hwnd = ((HwndSource)PresentationSource.FromVisual(this.Child)).Handle;
            ((HwndSource)PresentationSource.FromVisual(this.Child)).AddHook(WindowProc);

            ParentPopup = FindParentPopup();
            if((ParentPopup!=null) &&(ParentPopup.IsOpen))
                ParentPopup.IgnoreNextDeactivate = true;
            
            SetActiveWindow(hwnd);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if(ParentPopup!=null)
                ignoreNextDeactivate = true;
            base.OnPreviewMouseDown(e);            
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            IntPtr hwnd = ((HwndSource)PresentationSource.FromVisual(this.Child)).Handle;
            SetActiveWindow(hwnd);
            base.OnMouseDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (Mouse.Captured == this)
            {
                Mouse.Capture(null);
                e.Handled = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            PopupAnimation = PopupAnimation.None;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsOpen = false;
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }

        #endregion

        #region Private methods

        private ExtendedPopup FindParentPopup()
        {
            UIElement element = this.Parent as UIElement;
            while (element != null)
            {
                if (element is ExtendedPopup) return element as ExtendedPopup;
                element = (UIElement)LogicalTreeHelper.GetParent(element as DependencyObject);
            }
            return null;
        }

        // Функиця окна
        private IntPtr WindowProc(
                   IntPtr hwnd,
                   int msg,
                   IntPtr wParam,
                   IntPtr lParam,
                   ref bool handled)
        {
            switch (msg)
            {
                case 0x0006:
                    {
                        if (((short)wParam.ToInt32()) == 0)
                        {
                            if ((lParam != hwnd) && (!IgnoreNextDeactivate))
                            {
                                if((ParentPopup!=null)&&(ParentPopup.IsOpen))
                                {
                                    IntPtr parentHwnd = ((HwndSource)PresentationSource.FromVisual(ParentPopup.Child)).Handle;
                                    SendMessage(parentHwnd, 0x0006, wParam.ToInt32(),lParam);
                                }
                                PopupAnimation = PopupAnimation.Fade;
                                IsOpen = false;
                            }
                            else IgnoreNextDeactivate = false;
                        }
                        else
                        {
                            if (ParentPopup == null)
                            {
                                IntPtr parentHwnd = (new WindowInteropHelper(Window.GetWindow(this))).Handle;
                                SendMessage(parentHwnd, 0x0086, 1, IntPtr.Zero);                                
                            }
                        }
                        handled = true;
                        break;
                    }
                case 0x0021:
                    {
                        if (ParentPopup == null)
                        {
                            IntPtr parentHwnd = (new WindowInteropHelper(Window.GetWindow(this))).Handle;
                            SendMessage(parentHwnd, 0x0086, 1, IntPtr.Zero);
                            handled = true;
                        }
                        break;
                    }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Послать сообщение
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="Msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int SendMessage(
            IntPtr hWnd, // handle to destination window 
            int Msg, // message 
            int wParam, // first message parameter 
            IntPtr lParam // second message parameter 
        );

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

        [DllImport("User32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public const int GWL_STYLE = (-16);
        public const int GWL_EXSTYLE = (-20);

        public const int WS_EX_DLGMODALFRAME = 0x00000001;
        public const int WS_EX_NOPARENTNOTIFY = 0x00000004;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_ACCEPTFILES = 0x00000010;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_MDICHILD = 0x00000040;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_WINDOWEDGE = 0x00000100;
        public const int WS_EX_CLIENTEDGE = 0x00000200;
        public const int WS_EX_CONTEXTHELP = 0x00000400;
        public const int WS_EX_RIGHT = 0x00001000;
        public const int WS_EX_LEFT = 0x00000000;
        public const int WS_EX_RTLREADING = 0x00002000;
        public const int WS_EX_LTRREADING = 0x00000000;
        public const int WS_EX_LEFTSCROLLBAR = 0x00004000;
        public const int WS_EX_RIGHTSCROLLBAR = 0x00000000;
        public const int WS_EX_CONTROLPARENT = 0x00010000;
        public const int WS_EX_STATICEDGE = 0x00020000;
        public const int WS_EX_APPWINDOW = 0x00040000;
        public const int WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
        public const int WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
        public const int WS_EX_LAYERED = 0x00080000;
        // Disable inheritence of mirroring by children
        public const int WS_EX_NOINHERITLAYOUT = 0x00100000;
        // Right to left mirroring
        public const int WS_EX_LAYOUTRTL = 0x00400000;
        public const int WS_EX_COMPOSITED = 0x02000000;
        public const int WS_EX_NOACTIVATE = 0x08000000;

        #endregion
    }
}
