﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace local_network_chat_app.Converter
{
    public class StringLengthToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value) > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
