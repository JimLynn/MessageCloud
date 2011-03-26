using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace MessageCloud
{
	public class RichTextConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}
			return ExpandUrls(value.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public string ExpandUrls(string Text)
		{
			//string pattern = @"[""'=]?(http://|ftp://|https://|www\.|ftp\.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])";
			string pattern = @"(http://|ftp://|https://|www\.|ftp\.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])";
			// *** Expand embedded hyperlinks

			RegexOptions options =
				RegexOptions.IgnorePatternWhitespace |
				RegexOptions.Multiline |
				RegexOptions.IgnoreCase;
			Regex reg = new Regex(pattern, options);

			StringBuilder result = new StringBuilder();
			result.Append("<Section xml:space=\"preserve\" HasTrailingParagraphBreakOnPaste=\"False\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph FontSize=\"11\" FontFamily=\"Portable User Interface\" Foreground=\"#FF000000\" FontWeight=\"Normal\" FontStyle=\"Normal\" FontStretch=\"Normal\" TextAlignment=\"Left\">");
			var r = reg.Match(Text);
			int index = 0;
			while (r.Success)
			{
				if (r.Index > index)
				{
					result.Append("<Run Text=\"");
					result.Append(Text.Substring(index, r.Index - index));
					result.Append("\"/>");
				}
				result.Append("<Hyperlink><Run Text=\"");
				result.Append(r.Value);
				result.Append("\"/></Hyperlink>");
				index = r.Index + r.Length;
				r = r.NextMatch();
			}
			if (index < Text.Length)
			{
				result.Append("<Run Text=\"");
				result.Append(Text.Substring(index, Text.Length - index));
				result.Append("\"/>");
			}
			result.Append("</Paragraph></Section>");
			return result.ToString();
		}

	}
}
