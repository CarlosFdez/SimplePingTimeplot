using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace SimplePingTimeplot
{
    public class BorderlessWindowBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(Color), typeof(BorderlessWindowBehavior));

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public BorderlessWindowBehavior()
        {
            BorderColor = Colors.Black;
        }
        
        protected override void OnAttached()
        {
            var window = AssociatedObject;

            var chrome = new WindowChrome()
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(0),
                ResizeBorderThickness = SystemParameters.WindowResizeBorderThickness,
                GlassFrameThickness = new Thickness(0),
                UseAeroCaptionButtons = false,
            };

            WindowChrome.SetWindowChrome(window, chrome);

            window.BorderThickness = new Thickness(1);
            window.BorderBrush = new SolidColorBrush();

            // todo: implement drop shadow by using a drop shadow window that sits directly behind this window

            // Add the left down event
            window.MouseLeftButtonDown += HandleLeftButtonDown;
            window.MouseMove += HandleMoveMouse;

            base.OnAttached();
        }

        private void HandleLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                AssociatedObject.DragMove();
            }
        }

        /// <summary> 
        /// This handles dragging to make a window not maximized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMoveMouse(object sender, MouseEventArgs e)
        {
            var window = AssociatedObject;

            // If the window is not maximized or left button is not pressed, don't bother
            if (window.WindowState != WindowState.Maximized) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;

            // Move the window to a sane position so it moves naturally.
            // We don't want the window to be in a strange position when its unmaximized
            var screenPoint = window.PointToScreen(Mouse.GetPosition(window));
            var mouseScreenX = screenPoint.X;
            var mouseScreenY = screenPoint.Y;
            var nonMaximizedWindowWidth = window.RestoreBounds.Width;
            var nonMaximizedWindowHeight = window.RestoreBounds.Height;
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var screenHeight = SystemParameters.VirtualScreenHeight;

            double left = mouseScreenX - (nonMaximizedWindowWidth / 2);
            double top = mouseScreenY - (nonMaximizedWindowHeight / 2);

            // make sure its not offscreen
            left = Math.Max(left, 0);
            top = Math.Max(top, 0);

            window.Left = left;
            window.Top = top;

            // un-maximize the window and start dragging
            window.WindowState = WindowState.Normal;
            window.DragMove();
        }
    }
}
