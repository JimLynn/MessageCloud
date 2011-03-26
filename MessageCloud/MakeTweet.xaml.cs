using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using System.Windows.Browser;
using System.Xml.Linq;
using MiniTwitter;

namespace MessageCloud
{
	public partial class MakeTweet : UserControl, INotifyPropertyChanged
	{
		public MakeTweet()
		{
			InitializeComponent();
			DataContext = this;
			Loaded += new RoutedEventHandler(MakeTweet_Loaded);
		}

		void MakeTweet_Loaded(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Tweeted", false);
		}

		private string _tweetText = string.Empty;

		public string TweetText
		{
			get
			{
				return _tweetText;
			}
			set
			{
				_tweetText = value;
				FirePropertyChanged("TweetText");
				FirePropertyChanged("CharsLeft");
				FirePropertyChanged("CharsLeftColour");
			}
		}

		public string CharsLeft
		{
			get
			{
				int left = 140-TweetText.Length;
				if (left == 1)
				{
					return "1 char left";
				}
				else
				{
					return string.Format("{0} chars left", left);
				}
			}
		}

		public Brush CharsLeftColour
		{
			get
			{
				int left = 140-TweetText.Length;
				if (left < 0)
				{
					return new SolidColorBrush(Colors.Orange);
				}
				else if (left < 10)
				{
					return new SolidColorBrush(Colors.Red);
				}
				else
				{
					return new SolidColorBrush(Colors.Black);
				}
			}
		}

		private TwitterMessage _InReplyTo;
		public TwitterMessage InReplyTo
		{
			get
			{
				return _InReplyTo;
			}
			set
			{
				_InReplyTo = value;
				FirePropertyChanged("InReplyTo");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void FirePropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{

			// Do not load your data at design time.
			// if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
			// {
			// 	//Load your data here and assign the result to the CollectionViewSource.
			// 	System.Windows.Data.CollectionViewSource myCollectionViewSource = (System.Windows.Data.CollectionViewSource)this.Resources["Resource Key for CollectionViewSource"];
			// 	myCollectionViewSource.Source = your data
			// }
		}

		public OAuthHelper  OAuthHelper { get; set; }

		private void MakeTweet_Click(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Tweeting", true);
			string uri = "http://api.twitter.com/1/statuses/update.xml";
			MyWebClient client = new MyWebClient();
			client.OAuthHelper = OAuthHelper;
			client.AddParameter("status", TweetText);

			if (InReplyTo != null)
			{
				client.AddParameter("in_reply_to_status_id", InReplyTo.Id.ToString());
			}
			client.DoPostCompleted += new EventHandler<DoPostCompletedEventArgs>(TweetCompleted);
			client.DoPostAsync(new Uri(uri, UriKind.Absolute));
		}

		void TweetCompleted(object sender, DoPostCompletedEventArgs e)
		{
			if (e.Error == null)
			{
				string xml = e.Response;
				XElement element = XElement.Parse(xml);
				TwitterMessage myRetweet = new TwitterMessage(element);
				tweetmessage.Text = "Your message was posted";
				TweetText = "";
				VisualStateManager.GoToState(this, "Tweeted", true);

			}
			else
			{
				if (ShowMessage != null)
				{
					ShowMessage(this, new TweetEventArgs("Failed to send message"));
				}
				tweetmessage.Text = "Failed to send message";
				VisualStateManager.GoToState(this, "Tweeted", true);
			}
		}

		private void MakeTweet_ClickOld(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Tweeting", true);
			string uri = "http://api.twitter.com/1/statuses/update.xml";
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri, UriKind.Absolute));
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.BeginGetRequestStream(new AsyncCallback(SendParams), request);
		}

		private void SendParams(IAsyncResult result)
		{
			HttpWebRequest request = (HttpWebRequest)result.AsyncState;
			Stream stream = request.EndGetRequestStream(result);
			using (StreamWriter writer = new StreamWriter(stream))
			{
				writer.Write("status=" + HttpUtility.UrlEncode(TweetText));
				if (InReplyTo != null)
				{
					writer.Write("&in_reply_to_status_id=" + HttpUtility.UrlEncode(InReplyTo.Id.ToString()));
				}
			}
			stream.Close();
			request.BeginGetResponse(new AsyncCallback(UpdateStatus), request);
		}


		private void UpdateStatus(IAsyncResult result)
		{
			HttpWebRequest request = (HttpWebRequest)result.AsyncState;
			try
			{
				HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
				using (StreamReader reader = new StreamReader(response.GetResponseStream()))
				{
					string xml = reader.ReadToEnd();
					XElement element = XElement.Parse(xml);
					Dispatcher.BeginInvoke(() =>
					{
						TwitterMessage myRetweet = new TwitterMessage(element);
						tweetmessage.Text = "Your message was posted";
						TweetText = "";
						VisualStateManager.GoToState(this, "Tweeted", true);
					});
				}
			}
			catch (WebException exc)
			{
				Dispatcher.BeginInvoke(() =>
					{
						if (ShowMessage != null)
						{
							ShowMessage(this, new TweetEventArgs("Failed to send message"));
						}
						tweetmessage.Text = "Failed to send message";
						VisualStateManager.GoToState(this, "Tweeted", true);
					});
			}
		}

		public event EventHandler<TweetEventArgs> ShowMessage;

		private void TweetAgain_Click(object sender, RoutedEventArgs e)
		{
			TweetText = "";
			InReplyTo = null;
			VisualStateManager.GoToState(this, "Normal", true);
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Tweeted", false);
		}

		internal void StartReply()
		{
			TweetText = "@" + InReplyTo.User.ScreenName + " ";
			VisualStateManager.GoToState(this, "Normal", true);
		}

		public void StartMessage(string text)
		{
			TweetText = text;
			VisualStateManager.GoToState(this, "Normal", true);
		}
	}

	public class TweetEventArgs : EventArgs
	{
		public TweetEventArgs(string message)
		{
			Message = message;
		}
		public string Message { get; set; }
	}
}
