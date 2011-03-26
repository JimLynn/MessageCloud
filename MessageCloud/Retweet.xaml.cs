using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Windows.Data;
using MiniTwitter;

namespace MessageCloud
{
	public partial class Retweet : UserControl
	{
		bool AlreadyRetweeted = false;

		public Retweet()
		{
			// Required to initialize variables
			InitializeComponent();
			VisualStateManager.GoToState(this, "Normal", false);
			SetBinding(MyDataContextProperty, new Binding());
		}

		private void textBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!AlreadyRetweeted)
			{
				VisualStateManager.GoToState(this, "Prompt", true);
			}
		}


		#region MyDataContext

		/// <summary> 
		/// Identifies the MyDataContext dependency property.
		/// </summary> 
		public static readonly DependencyProperty MyDataContextProperty =
					DependencyProperty.Register(
						  "MyDataContext",
						  typeof(object),
						  typeof(Retweet),
						  new PropertyMetadata(OnMyDataContextPropertyChanged));

		/// <summary>
		/// MyDataContextProperty property changed handler. 
		/// </summary>
		/// <param name="d">Retweet that changed its MyDataContext.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnMyDataContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Retweet _Retweet = d as Retweet;
			if (_Retweet != null)
			{
				VisualStateManager.GoToState(_Retweet, "Normal", false);
			}
		}
		#endregion MyDataContext


		private void DoRetweet_Click(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Retweeting", true);
			if (AlreadyRetweeted == false)
			{
				AlreadyRetweeted = true;
				TwitterMessage message = DataContext as TwitterMessage;
				if (message != null)
				{
					string uri = "http://api.twitter.com/1/statuses/retweet/" + message.Id + ".xml";
					MyWebClient client = new MyWebClient();
					client.OAuthHelper = new OAuthHelper();
					client.DoPostCompleted += new EventHandler<DoPostCompletedEventArgs>(RetweetCompleted);
					client.DoPostAsync(new Uri(uri, UriKind.Absolute));
					//HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri, UriKind.Absolute));
					//request.Method = "POST";
					//request.BeginGetResponse(new AsyncCallback(RetweetResponse), request);
				}
			}
		}

		void RetweetCompleted(object sender, DoPostCompletedEventArgs e)
		{
			if (e.Error == null)
			{
				string xml = e.Response;
				XElement element = XElement.Parse(xml);
				TwitterMessage myRetweet = new TwitterMessage(element);
				TwitterMessage message = DataContext as TwitterMessage;
				if (message != null)
				{
					message.IsRetweetByMe = true;
					message.MyRetweetId = myRetweet.ActualId;
				}
				VisualStateManager.GoToState(this, "Retweeted", true);
			}
		}

		private void RetweetResponse(IAsyncResult result)
		{
			HttpWebRequest request = result.AsyncState as HttpWebRequest;
			HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				string xml = reader.ReadToEnd();
				XElement element = XElement.Parse(xml);
				Dispatcher.BeginInvoke(() =>
					{
						TwitterMessage myRetweet = new TwitterMessage(element);
						TwitterMessage message = DataContext as TwitterMessage;
						if (message != null)
						{
							message.IsRetweetByMe = true;
							message.MyRetweetId = myRetweet.ActualId;
						}
						VisualStateManager.GoToState(this, "Retweeted", true);
					});
			}
		}

		private void CancelRetweet_Click(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Normal", true);
		}

		private void UndoRetweet_Click(object sender, RoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "Undoing", true);
			TwitterMessage message = DataContext as TwitterMessage;
			if (message != null && message.IsRetweetByMe)
			{
				string uri = "http://api.twitter.com/1/statuses/destroy/" + message.MyRetweetId + ".xml";
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(uri, UriKind.Absolute));
				request.Method = "POST";
				request.BeginGetResponse(new AsyncCallback(DestroyRetweet), request);

			}
		}

		private void DestroyRetweet(IAsyncResult result)
		{
			HttpWebRequest request = result.AsyncState as HttpWebRequest;
			HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				string xml = reader.ReadToEnd();
				XElement element = XElement.Parse(xml);
				TwitterMessage myRetweet = new TwitterMessage(element);
				Dispatcher.BeginInvoke(() =>
					{
						TwitterMessage message = DataContext as TwitterMessage;
						if (message != null)
						{
							message.IsRetweetByMe = false;
							message.MyRetweetId = 0;
						}
						VisualStateManager.GoToState(this, "Normal", true);
					});
			}
		}

		private void RT_Click(object sender, MouseButtonEventArgs e)
		{
			VisualStateManager.GoToState(this, "Retweeted", true);
		}
	}
}