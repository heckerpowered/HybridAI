using System;
using System.Globalization;
using System.Windows.Data;

namespace HybridAI.Converter
{
    public class CaretIndexToCanvasLeftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int caretIndex && parameter is double charWidth)
            {
                return caretIndex * charWidth;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
