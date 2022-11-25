using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;

namespace pscommander
{
    public class DesktopService
    {
        private Desktop _desktop;
        private readonly PowerShellService _powerShellService;
        private readonly DataSourceService  _dataSourceService;
        private readonly List<Window> _windows;
        private readonly MouseHook _mouseHook = new MouseHook();

        public DesktopService(PowerShellService powerShellService, DataSourceService  dataSourceService)
        {
            _powerShellService = powerShellService;
            _dataSourceService = dataSourceService;
            _windows = new List<Window>();
            //_mouseHook.Install();

            // _mouseHook.LeftButtonUp += (e) => {
            //     foreach(var window in _windows)
            //     {
            //          // Retrieve the coordinate of the mouse position.
            //         Point pt = new Point(e.pt.x - window.Left, e.pt.y - window.Top + 50);

            //         // Perform the hit test against a given portion of the visual object tree.
            //         HitTestResult result = VisualTreeHelper.HitTest(window, pt);

            //         if (result != null)
            //         {
                        
            //             var windowHandle = new WindowInteropHelper(window).Handle;
            //             PostMessage(windowHandle, (int)MK_LBUTTON, IntPtr.Zero, (IntPtr)(((int)pt.Y << 16) | ((int)pt.X & 0xffff)));
            //         }
            //     }
            // };
        }

        public void SetDesktop(Desktop desktop)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate{
                ClearDesktop();
                _desktop = desktop;
                foreach(var widget in _desktop.Widgets)
                {
                    SetDesktopWidget(widget);
                }
            });
        }

        private void SetDesktopWidget(DesktopWidget widget)
        {
            Window widgetWindow = new Widget();
            widgetWindow.Top = widget.Top;
            widgetWindow.Left = widget.Left;
            widgetWindow.Height = widget.Height;
            widgetWindow.Width = widget.Width;            

            if (widget is TextDesktopWidget textDesktopWidget)
            {
                widgetWindow.Content = SetDesktopWidgetText(textDesktopWidget);
            }

            if (widget is ImageDesktopWidget imageDesktopWidget)
            {
                widgetWindow.Content = SetDesktopWidgetImage(imageDesktopWidget);
            }

            if (widget is WebpageDesktopWidget webpageDesktopWidget)
            {
                widgetWindow.Content = SetDesktopWidgetWebpage(webpageDesktopWidget);
            }
            
            if (widget is CustomDesktopWidget customDesktopWidget)
            {
                widgetWindow = SetDesktopWidgetCustom(customDesktopWidget);
                if (widgetWindow == null) return;
                widgetWindow.Top = widget.Top;
                widgetWindow.Left = widget.Left;
                widgetWindow.Height = widget.Height;
                widgetWindow.Width = widget.Width;
            }

            if (widget is MeasurementDesktopWidget measurementDesktopWidget)
            {
                widgetWindow.Content = SetDesktopWidgetMeasurement(measurementDesktopWidget);
            }

            if (widget is DataDesktopWidget dataDesktopWidget)
            {
                var dataSource = _dataSourceService.DataSources.FirstOrDefault(m => m.Name.Equals(dataDesktopWidget.DataSource, StringComparison.OrdinalIgnoreCase));
                if (dataSource == null) return;
                widgetWindow = SetDesktopWidgetData(dataDesktopWidget, dataSource);
                if (widgetWindow == null) return;
                widgetWindow.Top = widget.Top;
                widgetWindow.Left = widget.Left;
                widgetWindow.Height = widget.Height;
                widgetWindow.Width = widget.Width;
                widgetWindow.DataContext = dataSource;
            }

            widgetWindow.AllowsTransparency = widget.Transparent;
            if (widget.Transparent)
            {
                widgetWindow.WindowStyle = WindowStyle.None;
            }
            
            widgetWindow.WindowState = WindowState.Minimized;
            widgetWindow.Show();

            var windowHandle = new WindowInteropHelper(widgetWindow).Handle;
            SetDesktopWidgetParent(windowHandle);
            _windows.Add(widgetWindow);
        }

        private MeasurementCard SetDesktopWidgetMeasurement(MeasurementDesktopWidget measurementDesktopWidget)
        {
            return new MeasurementCard(measurementDesktopWidget, _powerShellService);
        }

        private Window SetDesktopWidgetData(DataDesktopWidget dataDesktopWidget, DataSource dataSource)
        {
            return _powerShellService.Execute<Window>(dataDesktopWidget.LoadWidget, dataSource).FirstOrDefault();
        }

        private Window SetDesktopWidgetCustom(CustomDesktopWidget customDesktopWidget)
        {
            return _powerShellService.Execute<Window>(customDesktopWidget.LoadWidget).FirstOrDefault();
        }

        private System.Windows.Controls.WebBrowser SetDesktopWidgetWebpage(WebpageDesktopWidget webpageDesktopWidget)
        {
            var webBrowser = new System.Windows.Controls.WebBrowser();
            webBrowser.Source = new Uri(webpageDesktopWidget.Url);
            return webBrowser;
        }

        private Image SetDesktopWidgetImage(ImageDesktopWidget widget)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri(widget.Image));
            return image;
        }

        private TextBlock SetDesktopWidgetText(TextDesktopWidget widget)
        {
            var textBlock = new TextBlock();
            textBlock.Text = widget.Text;

            if (!string.IsNullOrWhiteSpace(widget.Font))
            {
                textBlock.FontFamily = new FontFamily(widget.Font);
            }

            textBlock.FontSize = widget.FontSize;

            if (!string.IsNullOrWhiteSpace(widget.BackgroundColor))
            {
                textBlock.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(widget.BackgroundColor));
            }
            else 
            {
                VisualBrush vBrush = new VisualBrush();  
                vBrush.Opacity = 0;
                textBlock.Background = vBrush;
            }

            if (!string.IsNullOrWhiteSpace(widget.FontColor))
            {
                textBlock.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(widget.FontColor));
            }

            return textBlock;
        }

        private void SetDesktopWidgetParent(IntPtr widgetHandle)
        {
            IntPtr workerw = IntPtr.Zero;
            IntPtr result = IntPtr.Zero;
            IntPtr progman = FindWindow("Progman", null);

            // Send 0x052C to Progman. This message directs Progman to spawn a 
            // WorkerW behind the desktop icons. If it is already there, nothing 
            // happens.
            SendMessageTimeout(progman, 
                                0x052C, 
                                new IntPtr(0), 
                                IntPtr.Zero, 
                                SendMessageTimeoutFlags.SMTO_NORMAL, 
                                1000, 
                                out result);

            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
            // as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            EnumWindows(new EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = FindWindowEx(tophandle, 
                                            IntPtr.Zero, 
                                            "SHELLDLL_DefView", 
                                            null);

                if (p != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one.
                    workerw = FindWindowEx(IntPtr.Zero, 
                                            tophandle, 
                                            "WorkerW", 
                                            null);
                }

                return true;
            }), IntPtr.Zero);

            SetParent(widgetHandle, workerw);
        }

        public void ClearDesktop()
        {
            _desktop = null;
            IntPtr progman = FindWindow("Progman", null);
            SendMessage(progman, 0x0034, 4, IntPtr.Zero);

            foreach(var window in _windows)
            {
                window.Close();
            }

            _windows.Clear();
        }

        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            SendMessageTimeoutFlags fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);

            [Flags]
            enum SendMessageTimeoutFlags : uint
            {
                SMTO_NORMAL             = 0x0,
                SMTO_BLOCK              = 0x1,
                SMTO_ABORTIFHUNG        = 0x2,
                SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
                SMTO_ERRORONEXIT = 0x20
            }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError=true)]
        static extern int CloseWindow (IntPtr hWnd);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DestroyWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        const int WM_MOUSEMOVE = 0x0200;
        const int MK_LBUTTON = 0x0001;

    }
}

//New-CommanderDesktop  
//New-CommanderDesktopWidget
//Clear-CommanderDesktop
//Set-CommanderDesktop