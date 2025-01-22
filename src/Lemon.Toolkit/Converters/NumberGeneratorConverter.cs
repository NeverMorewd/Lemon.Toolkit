using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Toolkit.Converters
{
    public class NumberGeneratorConverter : IMultiValueConverter
    {
        // 用于生成随机数的实例
        private static readonly Random _random = new Random();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is not null && values.Count > 0)
            {
                // 如果输入值是数字类型，使用它作为范围
                if (values.FirstOrDefault() is double maxValue)
                {
                    return _random.NextDouble() * maxValue;
                }

                // 默认返回0-100范围内的随机数
            }
            return _random.NextDouble() * 100;
        }

        public object[]? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // 反向转换不需要实现
            throw new NotImplementedException();
        }
    }
}
