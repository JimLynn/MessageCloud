using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Browser;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using MessageCloud.Commanding;

namespace MessageCloud
{
	public partial class StatusView : UserControl
	{
		public StatusView()
		{
			// Required to initialize variables
			InitializeComponent();
			rtb.SetValue(StatusView.CommandProperty, new RelayCommand<string>(DisplayImage));
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			TwitterMessage message = (TwitterMessage)(sender as FrameworkElement).DataContext;
			message.Favourite();

		}

		//private void ShowInReply_Click(object sender, MouseButtonEventArgs e)
		//{
		//    e.Handled = true;
		//    TwitterMessage message = (TwitterMessage)(sender as FrameworkElement).DataContext;
		//    if (StatusViewAction != null)
		//    {
		//        StatusViewAction(this, new StatusClickEventArgs { Message = message });
		//    }
		//}
		private void InReplyTo_Click(object sender, RoutedEventArgs e)
		{
			TwitterMessage message = (TwitterMessage)(sender as FrameworkElement).DataContext;
			if (StatusViewAction != null)
			{
				StatusViewAction(this, new StatusClickEventArgs { Message = message, Type= ActionType.ShowReplies });
			}
		}



		public event EventHandler<StatusClickEventArgs> StatusViewAction;


		#region Xaml

		/// <summary> 
		/// Gets or sets the Xaml possible Value of the string object.
		/// </summary> 
		public string Xaml
		{
			get { return (string)GetValue(XamlProperty); }
			set { SetValue(XamlProperty, value); }
		}

		public static string GetXaml(DependencyObject obj)
		{
			return (string)obj.GetValue(XamlProperty);
		}

		public static void SetXaml(DependencyObject obj, string val)
		{
			obj.SetValue(XamlProperty, val);
		}

		/// <summary> 
		/// Identifies the Xaml dependency property.
		/// </summary> 
		public static readonly DependencyProperty XamlProperty =
					DependencyProperty.RegisterAttached(
						  "Xaml",
						  typeof(string),
						  typeof(StatusView),
						  new PropertyMetadata(OnXamlPropertyChanged));

		/// <summary>
		/// XamlProperty property changed handler. 
		/// </summary>
		/// <param name="d">StatusView that changed its Xaml.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnXamlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RichTextBox _rtb = d as RichTextBox;
			if (_rtb != null)
			{
				//string xaml = ExpandUrls(HttpUtility.HtmlEncode((string)e.NewValue));
				//_rtb.Xaml = xaml;
				SetRichTextContent(_rtb, (string)e.NewValue);
			}
		}

		public static void SetRichTextContent(RichTextBox rtb, string text)
		{
			rtb.Blocks.Clear();
			if (text == null)
			{
				return;
			}

			//string pattern = @"[""'=]?(http://|ftp://|https://|www\.|ftp\.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])";
			string pattern = @"(http://|ftp://|https://|www\.|ftp\.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])";
			// *** Expand embedded hyperlinks

			RegexOptions options =
				RegexOptions.IgnorePatternWhitespace |
				RegexOptions.Multiline |
				RegexOptions.IgnoreCase;
			Regex reg = new Regex(pattern, options);

			Paragraph result = new Paragraph();

			var r = reg.Match(text);
			int index = 0;
			while (r.Success)
			{
				if (r.Index > index)
				{
					MakeTextRunFromString(result, text.Substring(index, r.Index - index));
				}
				string path = r.Value;
				if (path.StartsWith("http") == false && path.StartsWith("ftp") == false)
				{
					path = "http://" + path;
				}

				try
				{
					Hyperlink link = new Hyperlink()
					{
						NavigateUri = new Uri(path, UriKind.Absolute),
						TargetName = "_blank"
					};
					link.Inlines.Add(new Run { Text = r.Value });
					ExpandShrunkLink(link, result, rtb);
					result.Inlines.Add(link);
				}
				catch (UriFormatException)
				{
					result.Inlines.Add(new Run { Text = r.Value });
				}
					//result.Append("<Hyperlink NavigateUri=\"" + r.Value + "\" TargetName=\"_blank\"><Run Text=\"");
				//result.Append(r.Value);
				//result.Append("\"/></Hyperlink>");
				index = r.Index + r.Length;
				r = r.NextMatch();
			}
			if (index < text.Length)
			{
				MakeTextRunFromString(result, text.Substring(index, text.Length - index));
			}
			//result.Append("</Paragraph></Section>");
			rtb.Blocks.Add(result);
		}

		private static void ExpandShrunkLink(Hyperlink link, Paragraph parent, RichTextBox rtb)
		{
			if (link.NavigateUri.AbsoluteUri.StartsWith("http://bit.ly"))
			{
				WebClient getBitly = new WebClient();
				getBitly.DownloadStringCompleted += (sender, e) =>
				{
					if (e.Error == null)
					{
						XElement result = XElement.Parse(e.Result);

						XElement long_url = GetPath(result, "data/entry/long_url");
						if (long_url != null)
						{
							string url = long_url.Value;

//							ToolTipService.SetToolTip(link, url);

							InlineUIContainer container = new InlineUIContainer();
							Ellipse ellipse = new Ellipse();
							ellipse.Width = 6;
							ellipse.Height = 6;
							ellipse.VerticalAlignment = VerticalAlignment.Center;
							ellipse.Margin = new Thickness(3);
							ellipse.Fill = new SolidColorBrush(Colors.Orange);
							ToolTipService.SetToolTip(ellipse, url);
							container.Child = ellipse;
							parent.Inlines.Insert(parent.Inlines.IndexOf(link) + 1, container);
							//link.Inlines.Add(container);
						}
					}
				};
				getBitly.DownloadStringAsync(new Uri("http://api.bit.ly/v3/expand?shortUrl=" + HttpUtility.UrlEncode(link.NavigateUri.AbsoluteUri) + "&login=jimlynn&apiKey=R_769e4a2d579cef3c9c18be33dc4b160c&format=xml", UriKind.Absolute));
			}
			else if (link.NavigateUri.AbsoluteUri.StartsWith("http://twitpic.com/"))
			{
				string url = link.NavigateUri.AbsoluteUri + "/full";
				WebClient getTwitpic = new WebClient();
				getTwitpic.DownloadStringCompleted += (sender, e) =>
					{
						if (e.Error == null)
						{
							//Regex getpic = new Regex("(<img(\\ [^=]+=\\\"[^\"]+\\\")*\\ *>)+");
							Regex getpic = new Regex(@"(?<=img\s+.*src\=[\x27\x22])(?<Url>[^\x27\x22]*)(?=[\x27\x22])");
							
							Match match = getpic.Match(e.Result);
							while (match.Success)
							{
								//if (match.Value.Contains("/images/logo-main.png") == false)
								if (match.Value.StartsWith("http://"))
								{
									var imgurl = match.Value;
									ICommand command = rtb.GetValue(StatusView.CommandProperty) as ICommand;

									if (command != null)
									{
										link.NavigateUri = null;
										link.Command = command;
										link.CommandParameter = imgurl;
									}
									break;
								}
								match = match.NextMatch();
							}
						}
					};
				getTwitpic.DownloadStringAsync(new Uri(url, UriKind.Absolute));
			}
			else if (link.NavigateUri.AbsoluteUri.StartsWith("http://yfrog.com/"))
			{
				string id = link.NavigateUri.AbsoluteUri.Substring("http://yfrog.com/".Length);
				WebClient getYfrog = new WebClient();
				getYfrog.DownloadStringCompleted+= (sender,e)=>
					{
						if (e.Error == null)
						{
							XElement xml = XElement.Parse(e.Result);
							XNamespace ns = "http://ns.imageshack.us/imginfo/7/";

							var links = xml.Element(ns + "links"); ;
							if (links != null)
							{
								string url = links.Element(ns + "image_link").Value;

								ICommand command = rtb.GetValue(StatusView.CommandProperty) as ICommand;

								if (command != null)
								{
									link.NavigateUri = null;
									link.Command = command;
									link.CommandParameter = url;
									//link.Command = new RelayCommand<string>(Dis
								}
								//Regex getimg = new Regex("<link rel=\"image_src\" href=\"([^\"]+)\" />");
								//Match match = getimg.Match(e.Result);
								//string url = match.Groups[1].Value;
								InlineUIContainer container = new InlineUIContainer();
								Ellipse ellipse = new Ellipse();
								ellipse.Width = 16;
								ellipse.Height = 16;
								ellipse.Fill = new SolidColorBrush(Colors.Orange);
								Image img = new Image();
								BitmapImage bmp = new BitmapImage(new Uri(url, UriKind.Absolute));
								img.Source = bmp;
								ToolTipService.SetToolTip(ellipse, img);
								container.Child = ellipse;
								parent.Inlines.Insert(parent.Inlines.IndexOf(link) + 1, container);
							}
						}
					};
				getYfrog.DownloadStringAsync(new Uri("http://yfrog.com/api/xmlInfo?path=" + id, UriKind.Absolute));
			}
			else if (link.NavigateUri.AbsoluteUri.StartsWith("http://tweetphoto.com/"))
			{
				string id = link.NavigateUri.AbsoluteUri.Substring("http://tweetphoto.com/".Length);
				WebClient getYfrog = new WebClient();
				getYfrog.DownloadStringCompleted += (sender, e) =>
				{
					if (e.Error == null)
					{
						XElement xml = XElement.Parse(e.Result);

						XNamespace ns = "http://tweetphotoapi.com";
						
						string url = xml.Element(ns + "BigImageUrl").Value;

						ICommand command = rtb.GetValue(StatusView.CommandProperty) as ICommand;

						if (command != null)
						{
							link.NavigateUri = null;
							link.Command = command;
							link.CommandParameter = url;
							//link.Command = new RelayCommand<string>(Dis
						}
						//Regex getimg = new Regex("<link rel=\"image_src\" href=\"([^\"]+)\" />");
						//Match match = getimg.Match(e.Result);
						//string url = match.Groups[1].Value;
						InlineUIContainer container = new InlineUIContainer();
						Ellipse ellipse = new Ellipse();
						ellipse.Width = 16;
						ellipse.Height = 16;
						ellipse.Fill = new SolidColorBrush(Colors.Orange);
						Image img = new Image();
						BitmapImage bmp = new BitmapImage(new Uri(url, UriKind.Absolute));
						img.Source = bmp;
						ToolTipService.SetToolTip(ellipse, img);
						container.Child = ellipse;
						parent.Inlines.Insert(parent.Inlines.IndexOf(link) + 1, container);
					}
				};
				getYfrog.DownloadStringAsync(new Uri("http://tweetphotoapi.com/api/tpapi.svc/photos/" + id, UriKind.Absolute));
			}
			else if (link.NavigateUri.AbsoluteUri.StartsWith("http://twitgoo.com/"))
			{
				string id = link.NavigateUri.AbsoluteUri.Substring("http://twitgoo.com/".Length);
				WebClient getYfrog = new WebClient();
				getYfrog.DownloadStringCompleted += (sender, e) =>
				{
					if (e.Error == null)
					{
						XElement xml = XElement.Parse(e.Result);

						string url = xml.Element("imageurl").Value;

						ICommand command = rtb.GetValue(StatusView.CommandProperty) as ICommand;

						if (command != null)
						{
							link.NavigateUri = null;
							link.Command = command;
							link.CommandParameter = url;
							//link.Command = new RelayCommand<string>(Dis
						}
						//Regex getimg = new Regex("<link rel=\"image_src\" href=\"([^\"]+)\" />");
						//Match match = getimg.Match(e.Result);
						//string url = match.Groups[1].Value;
						InlineUIContainer container = new InlineUIContainer();
						Ellipse ellipse = new Ellipse();
						ellipse.Width = 16;
						ellipse.Height = 16;
						ellipse.Fill = new SolidColorBrush(Colors.Orange);
						Image img = new Image();
						BitmapImage bmp = new BitmapImage(new Uri(url, UriKind.Absolute));
						img.Source = bmp;
						ToolTipService.SetToolTip(ellipse, img);
						container.Child = ellipse;
						parent.Inlines.Insert(parent.Inlines.IndexOf(link) + 1, container);
					}
				};
				getYfrog.DownloadStringAsync(new Uri("http://twitgoo.com/api/message/info/" + id, UriKind.Absolute));
			}
			else if (link.NavigateUri.AbsoluteUri.Contains("imgur.com") && link.NavigateUri.AbsoluteUri.EndsWith(".jpg"))
			{
				string url = link.NavigateUri.AbsoluteUri;
				ICommand command = rtb.GetValue(StatusView.CommandProperty) as ICommand;

				if (command != null)
				{
					link.NavigateUri = null;
					link.Command = command;
					link.CommandParameter = url;
					//link.Command = new RelayCommand<string>(Dis
				}

			}
		}

		private void DisplayImage(string imgUri)
		{
			if (StatusViewAction != null)
			{
				TwitterMessage message = DataContext as TwitterMessage;
				StatusViewAction(this, new StatusClickEventArgs { ImageUri = imgUri, Message = message, Type = ActionType.ShowImage });
			}
		}

		private static void GetImageFromUrl(Hyperlink link, Paragraph parent, string id)
		{
			WebClient getTwitpic = new WebClient();
			getTwitpic.DownloadStringCompleted += (sender, e) =>
			{
				if (e.Error == null)
				{
					GetImage(link, parent, e.Result, id);
				}
			};
			getTwitpic.DownloadStringAsync(link.NavigateUri);
		}

		private static void GetImage(Hyperlink link, Paragraph parent, string html, string id)
		{
			Regex getpic = new Regex("<img(\\ [^=]+=\\\"[^\"]+\\\")*(\\ id=\\\"" + id + "\\\")+(\\ [^=]+=\\\"[^\"]+\\\")*\\ *>");
			Match match = getpic.Match(html);
			if (match.Success)
			{
				var srcmatch = (from g in match.Groups.Cast<Group>()
								where g.Value.StartsWith(" src")
								select g).FirstOrDefault();

				if (srcmatch == null)
				{
					return;
				}
				var url = srcmatch.Value.Substring(" src=\"".Length).TrimEnd('"');
				//link.NavigateUri = new Uri(url, UriKind.Absolute);
				InlineUIContainer container = new InlineUIContainer();
				Ellipse ellipse = new Ellipse();
				ellipse.Width = 16;
				ellipse.Height = 16;
				ellipse.Fill = new SolidColorBrush(Colors.Orange);
				Image img = new Image();
				BitmapImage bmp = new BitmapImage(new Uri(url, UriKind.Absolute));
				img.Source = bmp;
				ToolTipService.SetToolTip(ellipse, img);
				container.Child = ellipse;
				parent.Inlines.Insert(parent.Inlines.IndexOf(link) + 1, container);
			}
		}

		private static XElement GetPath(XElement element, string path)
		{
			var elements = path.Split('/');
			var result = element;
			foreach (var name in elements)
			{
				result = result.Element(name);
				if (result == null)
				{
					break;
				}
			}
			return result;
		}

		//public static string ExpandUrls(string Text)
		//{
		//    //string pattern = @"[""'=]?(http://|ftp://|https://|www\.|ftp\.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])";
		//    string pattern = @"(http://|ftp://|https://|www\.|ftp\.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])";
		//    // *** Expand embedded hyperlinks

		//    RegexOptions options =
		//        RegexOptions.IgnorePatternWhitespace |
		//        RegexOptions.Multiline |
		//        RegexOptions.IgnoreCase;
		//    Regex reg = new Regex(pattern, options);

		//    StringBuilder result = new StringBuilder();
		//    result.Append("<Section xml:space=\"preserve\" HasTrailingParagraphBreakOnPaste=\"False\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Paragraph Foreground=\"#FF000000\" FontStretch=\"Normal\" TextAlignment=\"Left\">");
		//    var r = reg.Match(Text);
		//    int index = 0;
		//    while (r.Success)
		//    {
		//        if (r.Index > index)
		//        {
		//            MakeTextRun(result, Text.Substring(index, r.Index - index));
		//        }
		//        result.Append("<Hyperlink NavigateUri=\"" + r.Value + "\" TargetName=\"_blank\"><Run Text=\"");
		//        result.Append(r.Value);
		//        result.Append("\"/></Hyperlink>");
		//        index = r.Index + r.Length;
		//        r = r.NextMatch();
		//    }
		//    if (index < Text.Length)
		//    {
		//        MakeTextRun(result, Text.Substring(index, Text.Length - index));
		//    }
		//    result.Append("</Paragraph></Section>");
		//    return result.ToString();
		//}

		private static void MakeTextRunFromString(Paragraph result, string segment)
		{
			Regex GetTwitterBits = new Regex(@"[@#]\w+", RegexOptions.IgnorePatternWhitespace |
				RegexOptions.Multiline |
				RegexOptions.IgnoreCase);
			var m = GetTwitterBits.Match(segment);
			int i = 0;
			while (m.Success)
			{
				if (m.Index > i)
				{
					result.Inlines.Add(MakeClrRun(segment.Substring(i, m.Index - i)));
				}
				if (m.Value.StartsWith("@"))
				{
					result.Inlines.Add(UserLinkClr(m.Value));
				}
				else
				{
					result.Inlines.Add(HashtagClr(m.Value));
				}
				i = m.Index + m.Length;
				m = m.NextMatch();
			}
			if (i < segment.Length)
			{
				result.Inlines.Add(MakeClrRun(segment.Substring(i, segment.Length - i)));
			}
		}

		//private static void MakeTextRun(StringBuilder result, string segment)
		//{
		//    Regex GetTwitterBits = new Regex(@"[@#]\w+", RegexOptions.IgnorePatternWhitespace |
		//        RegexOptions.Multiline |
		//        RegexOptions.IgnoreCase);
		//    var m = GetTwitterBits.Match(segment);
		//    int i = 0;
		//    while (m.Success)
		//    {
		//        if (m.Index > i)
		//        {
		//            result.Append(MakeRun(segment.Substring(i, m.Index - i)));
		//        }
		//        if (m.Value.StartsWith("@"))
		//        {
		//            result.Append(UserLink(m.Value));
		//        }
		//        else
		//        {
		//            result.Append(Hashtag(m.Value));
		//        }
		//        i = m.Index + m.Length;
		//        m = m.NextMatch();
		//    }
		//    if (i < segment.Length)
		//    {
		//        result.Append(MakeRun(segment.Substring(i, segment.Length - i)));
		//    }
		//}

		private static Run MakeClrRun(string text)
		{
			return new Run()
			{
				Text = text
			};
		}
		//private static string MakeRun(string text)
		//{
		//    if (text.Contains("{"))
		//    {
		//        text = "{}";
		//    }
		//    return "<Run Text=\"" +
		//    text
		//    +"\"/>";

		//}

		private static Inline HashtagClr(string p)
		{
			var link = new Hyperlink()
			{
				NavigateUri =new Uri( "http://twitter.com/search?q=%23" + p.Substring(1),UriKind.Absolute),
				TargetName = "_blank"
			};
			link.Inlines.Add(MakeClrRun(p));
			return link;
		}
		//private static string Hashtag(string p)
		//{
		//    return string.Format("<Hyperlink NavigateUri=\"http://twitter.com/search?q=%23{0}\" TargetName=\"_blank\"><Run Text=\"{1}#{0}\"/></Hyperlink>", p.Substring(1),"{}");
		//}

		private static Inline UserLinkClr(string p)
		{
			var link = new Hyperlink()
			{
				NavigateUri = new Uri("http://twitter.com/" + p.Substring(1), UriKind.Absolute),
				TargetName = "_blank",
			};
			link.Inlines.Add(MakeClrRun(p));
			return link;

		}
		//private static string UserLink(string p)
		//{
		//    return string.Format("<Hyperlink NavigateUri=\"http://twitter.com/{0}\" TargetName=\"_blank\"><Run Text=\"{1}@{0}\"/></Hyperlink>", p.Substring(1),"{}");
		//}


		#endregion Xaml


		#region Command

		/// <summary> 
		/// Gets or sets the Command possible Value of the ICommand object.
		/// </summary> 
		public ICommand Command
		{
			get { return (ICommand)GetValue(CommandProperty); }
			set { SetValue(CommandProperty, value); }
		}

		/// <summary> 
		/// Identifies the Command dependency property.
		/// </summary> 
		public static readonly DependencyProperty CommandProperty =
					DependencyProperty.RegisterAttached(
						  "Command",
						  typeof(ICommand),
						  typeof(StatusView),
						  new PropertyMetadata(OnCommandPropertyChanged));

		/// <summary>
		/// CommandProperty property changed handler. 
		/// </summary>
		/// <param name="d">StatusView that changed its Command.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			StatusView _StatusView = d as StatusView;
			if (_StatusView != null)
			{
				//TODO: Handle new value. 
			}
		}
		#endregion Command


		private void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{

		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			if (StatusViewAction != null)
			{
				TwitterMessage message = (TwitterMessage)(sender as FrameworkElement).DataContext;
				StatusViewAction(this, new StatusClickEventArgs { Message = message, Type = ActionType.UserPrevious });
			}
		}

		private void NextUserTweet_Click(object sender, RoutedEventArgs e)
		{
			if (StatusViewAction != null)
			{
				TwitterMessage message = (TwitterMessage)(sender as FrameworkElement).DataContext;
				StatusViewAction(this, new StatusClickEventArgs { Message = message, Type = ActionType.UserNext });
			}
		}

		private void RetweetUsername_Click(object sender, RoutedEventArgs e)
		{
			TwitterMessage message = (TwitterMessage)(sender as FrameworkElement).DataContext;
			if (StatusViewAction != null)
			{
				StatusViewAction(this, new StatusClickEventArgs { Message = message, Type = ActionType.RetweetUsernameClicked });
			}
		}

		private void Username_Click(object sender, RoutedEventArgs e)
		{
			TwitterMessage message = (TwitterMessage)(sender as FrameworkElement).DataContext;
			if (StatusViewAction != null)
			{
				StatusViewAction(this, new StatusClickEventArgs { Message = message, Type = ActionType.UsernameClicked });
			}
		}

		private void ReplyToThis_Click(object sender, RoutedEventArgs e)
		{
			TwitterMessage message = (TwitterMessage)(sender as FrameworkElement).DataContext;
			if (StatusViewAction != null)
			{
				StatusViewAction(this, new StatusClickEventArgs { Message = message, Type = ActionType.ReplyToThis });
			}
		}

	}

		public enum ActionType
		{
			ShowReplies,
			UserPrevious,
			UserNext,
			RetweetUsernameClicked,
			UsernameClicked,
			ReplyToThis,
			ShowImage
		}

	public class StatusClickEventArgs : EventArgs
	{
		public TwitterMessage Message { get; set; }

		public ActionType Type { get; set; }

		public bool Handled { get; set; }

		public string ImageUri { get; set; }
	}

	public class SampleData
	{
		public TwitterMessage TwitterMessage { get; set; }
		public SampleData()
		{
			TwitterMessage = new TwitterMessage
			{
				Id = 12345678,
				CreatedAt = DateTime.Now.Subtract(TimeSpan.FromHours(1.5)),
				Text = "This is a sample message. http://www.bbc.co.uk",
				User = new TwitterUser
				{
					UserID = 12345,
					Description = "A user's description",
					FollowersCount = 423,
					Name = "Fredbloggs",
					ScreenName = "Fred Bloggs",
					ProfileImageUrl = new Uri("http://a1.twimg.com/profile_images/74621100/Picture0002_normal.jpg", UriKind.Absolute)
				},
				RetweetUser = new TwitterUser
				{
					UserID = 54321,
					Description = "Someone else",
					FollowersCount = 423,
					Name = "Johnsmith",
					ScreenName = "John Smith",
					ProfileImageUrl = new Uri("http://a1.twimg.com/profile_images/74621100/Picture0002_normal.jpg", UriKind.Absolute)
				},
				IsRetweet = true
			};
		}
	}
}