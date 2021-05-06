using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ShadowKernel.helper
{
    class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Console.WriteLine("***********************************");
            if (value != null)
            {
                string imagename = value as string; Console.WriteLine(imagename);
                return new BitmapImage(new Uri(string.Format(@"..\{0}", imagename), UriKind.Relative));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Can not ConvertBack");
        }
    }
}
