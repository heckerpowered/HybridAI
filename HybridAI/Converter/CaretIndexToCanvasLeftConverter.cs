using System;
using System.Globalization;
using System.Windows.Data;

namespace HybridAI.Converter
{
    public class CaretIndexToCanvasLeftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int caretIndex && parameter is double charWidth ? caretIndex * charWidth : (object)0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
