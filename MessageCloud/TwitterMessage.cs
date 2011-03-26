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
using System.Xml.Linq;
using System.Globalization;
using System.ComponentModel;
using MessageCloud.ServiceReference1;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Browser;
using MiniTwitter;

namespace MessageCloud
{
	public class TwitterMessage : INotifyPropertyChanged
	{
		public string Text { get; set; }
		public DateTime CreatedAt { get; set; }
		public Int64 Id { get; set; }
		public Int64 ActualId { get; set; }
		public Int64 InReplyToId { get; set; }

		public bool IsReply
		{
			get
			{
				return InReplyToId > 0;
			}
		}

		public bool IsReplyExpanded
		{
			get
			{
				if (InReplyToMessage != null)
				{
					return true;
				}
				return false;
			}
		}
		public Int64 InReplyToUserId { get; set; }
		public string InReplyToScreenName { get; set; }
		public TwitterUser User { get; set; }
		public bool IsRetweet { get; set; }
		private bool _IsRetweetByMe;
		public bool IsRetweetByMe
		{
			get
			{
				return _IsRetweetByMe;
			}
			set
			{
				_IsRetweetByMe = value;
				OnPropertyChanged("IsRetweetByMe");
			}
		}

		private long _MyRetweetId;
		public long MyRetweetId
		{
			get
			{
				return _MyRetweetId;
			}
			set
			{
				_MyRetweetId = value;
				OnPropertyChanged("MyRetweetId");
			}
		}

		private bool isFavourite;
		public bool IsFavourite
		{
			get
			{
				return isFavourite;
			}
			set
			{
				isFavourite = value;
				OnPropertyChanged("IsFavourite");
			}
		}
		public TwitterUser RetweetUser { get; set; }

		public TwitterMessage()
		{

		}

		private
			TwitterMessage inReplyToMessage;
		public TwitterMessage InReplyToMessage
		{
			get
			{
				return inReplyToMessage;
			}
			set
			{
				inReplyToMessage = value;
				OnPropertyChanged("InReplyToMessage");
				OnPropertyChanged("IsReplyExpanded");
			}
		}

		public string UriToStatus
		{
			get
			{
				return "http://twitter.com/" + this.User.ScreenName + "/status/" + Id.ToString();
			}
		}

		ObservableCollection<TwitterMessage> replies = new ObservableCollection<TwitterMessage>();
		public ObservableCollection<TwitterMessage> Replies
		{
			get
			{
				return replies;
			}
		}

		public TwitterMessage(XElement element)
		{
			ActualId = Convert.ToInt64(element.Element("id").Value);
			if (element.Element("retweeted_status") != null)
			{
				RetweetUser = new TwitterUser(element.Element("user"));
				element = element.Element("retweeted_status");
				IsRetweet = true;
			}
			CreatedAt = DateTime.ParseExact(element.Element("created_at").Value, "ddd MMM dd HH:mm:ss +0000 yyyy", CultureInfo.InvariantCulture);
			CreatedAt = CreatedAt.ToLocalTime();
			Text = HttpUtility.HtmlDecode(element.Element("text").Value);
			Id = Convert.ToInt64(element.Element("id").Value);
			IsFavourite = element.Element("favorited").Value == "true";
			if (element.Element("in_reply_to_status_id").Value != "")
			{
				InReplyToId = Convert.ToInt64(element.Element("in_reply_to_status_id").Value);
				InReplyToUserId = Convert.ToInt64(element.Element("in_reply_to_user_id").Value);
				InReplyToScreenName = element.Element("in_reply_to_screen_name").Value;
			}
			User = new TwitterUser(element.Element("user"));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		internal void Favourite()
		{
			//TwitterClient client = new TwitterClient();
			//client.FavoriteCompleted += new EventHandler<FavoriteCompletedEventArgs>(client_FavoriteCompleted);
			//client.FavoriteAsync(Id);
			MyWebClient client = new MyWebClient();
			client.OAuthHelper = new OAuthHelper();
			client.DoPostCompleted +=new EventHandler<DoPostCompletedEventArgs>(client_DoPostCompleted); //new UploadStringCompletedEventHandler(client_FavoriteCompleted);
			client.DoPostAsync(new Uri(string.Format("http://api.twitter.com/1/favorites/create/{0}.xml", Id), UriKind.Absolute));
		}

		void client_DoPostCompleted(object sender, DoPostCompletedEventArgs e)
		{
			if (e.Error == null)
			{
				string result = e.Response;
				IsFavourite = true;
			}
		}

		void client_FavoriteCompleted(object sender, UploadStringCompletedEventArgs e)
		{

			if (e.Error == null)
			{
				string result = e.Result;
				IsFavourite = true;
			}
		}

		internal void Expand()
		{
			if (InReplyToMessage == null && InReplyToId > 0)
			{
				WebClient client = new WebClient();
				client.DownloadStringCompleted += (o, a) =>
				{
					if (a.Error == null)
					{
						XElement element = XElement.Parse(a.Result);
						InReplyToMessage = new TwitterMessage(element);
						Replies.Add(InReplyToMessage);
						InReplyToMessage.Replies.CollectionChanged += new NotifyCollectionChangedEventHandler(Replies_CollectionChanged);
						InReplyToMessage.Expand();
					}
				};
				client.DownloadStringAsync(new Uri(string.Format("http://api.twitter.com/1/statuses/show/{0}.xml", InReplyToId),UriKind.Absolute));

			}
		}

		void Replies_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (var item in e.NewItems)
			{
				Replies.Add((TwitterMessage)item);
			}
		}
	}
}
