﻿using PixiEditor.Models.Enums;
using System;

namespace PixiEditor.Helpers.Converters
{
  internal class EnumToStringConverter : SingleInstanceConverter<EnumToStringConverter>
  {
    public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      try
      {
        var type = value.GetType();
        if (type == typeof(SizeUnit))
        {
          var valueCasted = (SizeUnit)value;
          if (valueCasted == SizeUnit.Percentage)
            return "%";
          
          return "px";
        }
        return Enum.GetName((value.GetType()), value);
      }
      catch
      {
        return string.Empty;
      }
    }
  }
}
