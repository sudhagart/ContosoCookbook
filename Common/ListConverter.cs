using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ContosoCookbook.Common
{
    public class ListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ObservableCollection<string> items = (ObservableCollection<string>)value;
            StringBuilder builder = new StringBuilder();

            foreach (var item in items)
            {
                builder.Append(item);
                builder.Append("\r\n");
            }

            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
