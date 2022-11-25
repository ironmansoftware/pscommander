using System.Collections.Generic;

namespace pscommander
{
    public class MeasurementTheme 
    {
        public static Dictionary<string, MeasurementTheme> Themes { get; }

        static MeasurementTheme()
        {
            Themes = new Dictionary<string, MeasurementTheme>();

            Themes.Add("LightRed", new MeasurementTheme {
                Stroke = "White",
                Fill = "#4EFFFFFF",
                ChartBackground = "#CE2156",
                Title = "White",
                Subtitle = "#59FFFFFF",
                TextForeground = "#303030",
                TextBackground = "White"
            });

            Themes.Add("LightBlue", new MeasurementTheme {
                Stroke = "White",
                Fill = "#48cae4",
                ChartBackground = "#023e8a",
                Title = "White",
                Subtitle = "#59FFFFFF",
                TextForeground = "#303030",
                TextBackground = "White"
            });

            Themes.Add("LightGreen", new MeasurementTheme {
                Stroke = "White",
                Fill = "#95d5b2",
                ChartBackground = "#2d6a4f",
                Title = "White",
                Subtitle = "#59FFFFFF",
                TextForeground = "#303030",
                TextBackground = "White"
            });

            Themes.Add("DarkRed", new MeasurementTheme {
                Stroke = "White",
                Fill = "#4EFFFFFF",
                ChartBackground = "#CE2156",
                Title = "White",
                Subtitle = "#59FFFFFF",
                TextForeground = "#59FFFFFF",
                TextBackground = "Black"
            });

            Themes.Add("DarkBlue", new MeasurementTheme {
                Stroke = "White",
                Fill = "#48cae4",
                ChartBackground = "#023e8a",
                Title = "White",
                Subtitle = "#59FFFFFF",
                TextForeground = "#59FFFFFF",
                TextBackground = "Black"
            });

            Themes.Add("DarkGreen", new MeasurementTheme {
                Stroke = "White",
                Fill = "#95d5b2",
                ChartBackground = "#2d6a4f",
                Title = "White",
                Subtitle = "#59FFFFFF",
                TextForeground = "#59FFFFFF",
                TextBackground = "Black"
            });
        }

        public string Stroke { get; set; }
        public string Fill { get; set; }
        public string ChartBackground { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string TextBackground { get; set; }
        public string TextForeground { get; set; }
    }
}