using System.Drawing;
using System.Management.Automation;

namespace pscommander 
{
    public class TextDesktopWidget : DesktopWidget
    {
        public string Text { get; set; }
        public string Font { get; set; }
        public int FontSize { get; set; }
        public string FontColor { get; set; }
        public string BackgroundColor { get; set; }
    }

    public class ImageDesktopWidget : DesktopWidget
    {
        public string Image { get; set; }
    }

    public class WebpageDesktopWidget : DesktopWidget
    {
        public string Url { get; set; }
    }

    public class CustomDesktopWidget : DesktopWidget
    {
        public ScriptBlock LoadWidget { get; set; }
    }

        public class DataDesktopWidget : DesktopWidget
    {
        public ScriptBlock LoadWidget { get; set; }
        public string DataSource { get; set; }
    }

    public class MeasurementDesktopWidget : DesktopWidget
    {
        public ScriptBlock LoadMeasurement { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public int Frequency { get; set; }
        public int History { get; set; }
        public string Theme { get; set; }
    }

    public class DesktopWidget
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public bool Transparent { get; set; }
    }
}