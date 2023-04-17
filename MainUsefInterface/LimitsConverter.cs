using System;
using System.Windows.Data;

namespace MainUserInterface
{
    [ValueConversion(typeof(string), typeof(double[]))]
    public class LimitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, System.Globalization.CultureInfo culture)
        {
            double[] limits = (double[])value;
            return limits[0].ToString() + "; " + limits[1].ToString();
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, System.Globalization.CultureInfo culture)
        {
            string text = (string)value;
            string[] limitsString = text.Split(';');
            if (limitsString.Length != 2)
            {
                return new double[2] { 0, -1 };
            }
            double[] limits = new double[2];
            try
            {
                limits[0] = System.Convert.ToDouble(limitsString[0]);
                limits[1] = System.Convert.ToDouble(limitsString[1]);
            }
            catch (Exception)
            {
                limits[0] = 0;
                limits[1] = -1;
            }
            return limits;
        }
    }
}
