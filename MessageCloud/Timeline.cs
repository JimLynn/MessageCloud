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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using MessageCloud.Commanding;
using MiniTwitter;

namespace MessageCloud
{
	public class TwitterTimeline : ObservableCollection<TwitterMessage>
	{
		public void AddRange(IEnumerable<TwitterMessage> messages)
		{
			foreach (var message in messages)
			{
				this.Add(message);
			}
		}

		public TwitterTimeline(XElement element)
		{
			var statuses = (from status in element.Elements("status")
						   select new TwitterMessage(status)).Reverse();
			AddRange(statuses);
		}
	}

	public class TimelineTestCollection : List<UserTimelineViewModel>
	{
	
		public TimelineTestCollection()
		{
			if (DesignerProperties.IsInDesignTool)
			{
				AddRange(new[] { new UserTimelineViewModel(), new UserTimelineViewModel() });
			}
		}
	}

	public abstract class TimelineViewModel : INotifyPropertyChanged
	{
		public OAuthHelper OAuthHelper { get; set; }

		public event EventHandler<StatusChangedEventArgs> StatusChanged;

		protected void SendLoadingMessage(string message)
		{
			SendStatusMessage(message, StatusMessageType.Loading); 
		}

		protected void SendLoadedMessage(string message)
		{
			SendStatusMessage(message, StatusMessageType.Loaded);
		}

		protected IEnumerable<KeyValuePair<string, string>> Params(params string[] values)
		{
			List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
			for (int i = 0; i < values.Length; i = i + 2)
			{
				result.Add(new KeyValuePair<string, string>(values[i], values[i + 1]));
			}
			return result;
		}

		private TwitterMessage _currentMessage;
		public TwitterMessage CurrentMessage
		{
			get
			{
				return _currentMessage;
			}
			set
			{
				_currentMessage = value;
				FirePropertyChanged("CurrentMessage");
				if (_currentMessage != null)
				{
					var store = IsolatedStorageSettings.ApplicationSettings;
					store["CurrentMessage"] = _currentMessage.ActualId;
					IsolatedStorageSettings.ApplicationSettings.Save();
				}
			}
		}

		public virtual void OnKeyPress(KeyEventArgs e)
		{
			if (e.Handled || Messages.Count == 0)
				return;

			int currentIndex = Messages.IndexOf(CurrentMessage);
			if (currentIndex < 0)
			{
				CurrentMessage = Messages.First();
				return;
			}

			switch (e.Key)
			{
				case Key.Up:
				case Key.Left:
					if (currentIndex == 0)
					{
						return;
					}
					else
					{
						CurrentMessage = Messages[currentIndex - 1];
						return;
					}
				case Key.Down:
				case Key.Right:
					if (currentIndex+1 == Messages.Count)
					{
						return;
					}
					else
					{
						CurrentMessage = Messages[currentIndex + 1];
						return;
					}
				case Key.PageDown:
					currentIndex += 20;
					if (currentIndex >= Messages.Count)
					{
						currentIndex = Messages.Count - 1;
					}
					CurrentMessage = Messages[currentIndex];
					return;
				case Key.PageUp:
					currentIndex -= 20;
					if (currentIndex < 0)
					{
						currentIndex = 0;
					}
					CurrentMessage = Messages[currentIndex];
					return;
				case Key.Home:
					currentIndex = 0;
					CurrentMessage = Messages[currentIndex];
					return;
				case Key.End:
					currentIndex = Messages.Count - 1;
					CurrentMessage = Messages[currentIndex];
					return;
				default:
					return;
			}
		}

		protected void SendErrorMessage(string message)
		{
			SendStatusMessage(message, StatusMessageType.Error);
		}

		protected void SendMessage(string message)
		{
			SendStatusMessage(message, StatusMessageType.Normal);
		}

		private void SendStatusMessage(string message, StatusMessageType statusMessageType)
		{
			if (StatusChanged != null)
			{
				StatusChanged(this, new StatusChangedEventArgs(message, statusMessageType));
			}
		}

		protected IEnumerable<TwitterMessage> AddNewMessages(IEnumerable<TwitterMessage> newMessages)
		{
			// join this with the existing messages
			var newmessages = from newm in newMessages
							   join old in messages on newm.ActualId equals old.ActualId
							   into joined
							   from leftjoin in joined.DefaultIfEmpty()
							   where leftjoin == null
							   select newm;

			messages.AddRange(newmessages);

			return from newm in newMessages
					join old in messages on newm.ActualId equals old.ActualId
					into joined
					from right in joined.DefaultIfEmpty()
					select right;
		}
		
		private static List<TwitterMessage> messages = new List<TwitterMessage>();

		public string Description
		{ get; set; }
		
		protected ObservableCollection<TwitterMessage> _timelineMessages;
		public ObservableCollection<TwitterMessage> Messages
		{
			get { return _timelineMessages; }
			protected set
			{
				_timelineMessages = value;
				FirePropertyChanged("Messages");
			}
		}

		protected void FetchTimeline(string Uri, IEnumerable<KeyValuePair<string,string>> parameters, string error, bool ShowFirst = false)
		{
			SendLoadingMessage("Fetching messages");
			MyWebClient getUserMessages = new MyWebClient();
			getUserMessages.OAuthHelper = OAuthHelper;
			getUserMessages.AddParameters(parameters);
			bool showfirst = ShowFirst;
			getUserMessages.DoPostCompleted += (sender,e) =>
				{
					if (e.Error != null)
					{
						SendErrorMessage(error);
					}
					else
					{
						XElement element = XElement.Parse(e.Response);
						TwitterTimeline timeline = new TwitterTimeline(element);
						var added = AddNewMessages(timeline).ToList();
						foreach (var mess in added)
						{
							_timelineMessages.Add(mess);
						}
						if (added.Count > 0)
						{
							if (showfirst)
							{
								CurrentMessage = added.FirstOrDefault();
							} SendLoadedMessage(added.Count.ToString() + " Messages loaded");
						}
						else
						{
							SendLoadedMessage("No messages loaded");
						}
					}
				};

			getUserMessages.DoGetAsync(new Uri(Uri, UriKind.Absolute));

		}

		private ObservableCollection<TimelineCommand> _commands = new ObservableCollection<TimelineCommand>();
		public ObservableCollection<TimelineCommand> Commands
		{
			get
			{
				return _commands;
			}
			set 
			{
				_commands = value;
				FirePropertyChanged("Commands");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void FirePropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		public virtual void OnStatusViewAction(StatusClickEventArgs e)
		{
			if (e.Handled)
			{
				return;
			}
			if (e.Type == ActionType.ShowReplies)
			{
				FetchReply(e.Message);
				e.Handled = true;
			}
			else if (e.Type == ActionType.ReplyToThis)
			{
				if (MakeTweet != null)
				{
					MakeTweet(this, new MakeTweetEventArgs(e.Message));
				}
			}
			else if (e.Type == ActionType.UserPrevious)
			{
				var prev = Messages.LastOrDefault(m => m.User.UserID == e.Message.User.UserID && m.Id < e.Message.Id);
				if (prev != null)
				{
					CurrentMessage = prev;
				}
			}
			else if (e.Type == ActionType.UserNext)
			{
				var next = Messages.FirstOrDefault(m => m.User.UserID == e.Message.User.UserID && m.Id > e.Message.Id);
				if (next != null)
				{
					CurrentMessage = next;
				}
			}
		}

		protected void FetchReply(TwitterMessage twitterMessage)
		{
			if (twitterMessage.InReplyToId > 0)
			{
				SendLoadingMessage("Loading reply");
				MyWebClient client = new MyWebClient();
				client.OAuthHelper = OAuthHelper;
				//TwitterClient client = new TwitterClient();
				client.DoPostCompleted += (o, e) =>
				{
					if (e.Error == null)
					{
						TwitterMessage newMessage = new TwitterMessage(XElement.Parse(e.Response));
						// Create a new
						int index = Messages.IndexOf(twitterMessage);
						Messages.Insert(index, newMessage);
						SendLoadedMessage("Reply loaded");
						CurrentMessage = newMessage;
					}
					else
					{
						SendErrorMessage("Error Fetching Reply");
					}
				};
				client.DoGetAsync(new Uri(string.Format("http://api.twitter.com/1/statuses/show/{0}.xml", twitterMessage.InReplyToId), UriKind.Absolute));
			}
		}

		public event EventHandler<MakeTweetEventArgs> MakeTweet;
	}

	public class MakeTweetEventArgs : EventArgs
	{
		public long? InReplyTo { get; set; }
		public string InitialText { get; set; }
		public TweetType TweetType { get; set; }
		public TwitterMessage Message { get; set; }

		public MakeTweetEventArgs()
		{
			InitialText = string.Empty;
			TweetType = MessageCloud.TweetType.Normal;
		}

		public MakeTweetEventArgs(string text)
		{
			InitialText = text;
			TweetType = MessageCloud.TweetType.Normal;
		}

		public MakeTweetEventArgs(string text, long id)
		{
			InitialText = text;
			InReplyTo = id;
			TweetType = MessageCloud.TweetType.Normal;
		}

		public MakeTweetEventArgs(TwitterMessage message)
		{
			Message = message;
			TweetType = TweetType.Reply;
		}

		public MakeTweetEventArgs(TwitterMessage message, TweetType type)
		{
			Message = message;
			TweetType = type;
		}
	}

	public enum TweetType
	{
		Normal,
		Reply,
		ManualRT
	}

	public class TimelineCommand : INotifyPropertyChanged
	{
		private string _Label;
		public string Label
		{
			get
			{
				return _Label;
			}
			set
			{
				_Label = value;
				FirePropertyChanged("Label");
			}
		}

		private ICommand _Command;
		public ICommand Command
		{
			get
			{
				return _Command;
			}
			set
			{
				_Command = value;
				FirePropertyChanged("Command");
			}
		}

		private string _ToolTip;
		public string ToolTip
		{
			get
			{
				return _ToolTip;
			}
			set
			{
				_ToolTip = value;
				FirePropertyChanged("ToolTip");
			}
		}

		public TimelineCommand(string label, string description, ICommand command)
		{
			Label = label;
			ToolTip = description;
			Command = command;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void FirePropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}
	}

	public class StatusChangedEventArgs : EventArgs
	{
		public StatusChangedEventArgs(string message, StatusMessageType type)
		{
			Message = message;
			Type = type;
		}
		public string Message { get; set; }
		public StatusMessageType Type { get; set; }
	}

	public enum StatusMessageType
	{
		Loading,
		Loaded,
		Error,
		Normal
	}

	public class UserTimelineViewModel : TimelineViewModel
	{
		public UserTimelineViewModel()
		{
			if (DesignerProperties.IsInDesignTool)
			{
				XElement.Load("Data/home_timeline.xml");
			}

			Commands.Add(new TimelineCommand
				(
					"Older",
					"Fetch older messages from this user",
					new RelayCommand(FetchOlder)
				));

			Commands.Add(new TimelineCommand
				(
					"Newer",
					"Fetch newer messages from this user",
					new RelayCommand(FetchOlder)
				));
		}

		private void FetchOlder()
		{
		}

		public UserTimelineViewModel(string user_name, OAuthHelper helper) : this()
		{
			OAuthHelper = helper;
			this.Description = "by " + user_name;
			Messages = new ObservableCollection<TwitterMessage>();
			FetchTimeline(string.Format("http://api.twitter.com/1/statuses/user_timeline/{0}.xml", user_name), 
				Params("count","200"), "Failed to load user timeline");
		}

	}

	public class MainTimelineViewModel : TimelineViewModel
	{
		public MainTimelineViewModel(OAuthHelper helper)
		{
			OAuthHelper = helper;
			Description = "Main Timeline";

			Commands.Add(new TimelineCommand
			(
				"Older",
				"Fetch older messages from this user",
				new RelayCommand(FetchOlder)
			));

			Commands.Add(new TimelineCommand
			(
				"Newer",
				"Fetch newer messages from this user",
				new RelayCommand(FetchNewer)
			));

	
			if (DesignerProperties.IsInDesignTool)
			{
			}
			else
			{
				var currentID = 15315276793L;
				if (IsolatedStorageSettings.ApplicationSettings.Contains("CurrentMessage"))
				{
					currentID = (Int64)IsolatedStorageSettings.ApplicationSettings["CurrentMessage"];
				}
				_timelineMessages = new ObservableCollection<TwitterMessage>();
				fetchList.Clear();
				OlderPostsUpTo(currentID,0);
			}
		}

		private void FetchOlder()
		{
			long oldest = Messages.Min(m => m.ActualId);
			FetchTimeline("http://api.twitter.com/1/statuses/home_timeline.xml",
				Params("count","200",
						"max_id",oldest.ToString()),
				"No older posts available");
		}

		private void FetchNewer()
		{
			long newest = Messages.Max(m => m.ActualId);
			FetchTimeline("http://api.twitter.com/1/statuses/home_timeline.xml",
				Params("count", "200", "since_id", newest.ToString()), "No newer posts available");
		}
		List<TwitterMessage> fetchList = new List<TwitterMessage>();

		private void OlderPostsUpTo(long currentID, long maxID)
		{
			MyWebClient getUserMessages = new MyWebClient();
			getUserMessages.OAuthHelper = OAuthHelper;
			getUserMessages.DoPostCompleted += (sender, e) =>
			{
				if (e.Error != null)
				{
					SendErrorMessage(e.Error.Message);
				}
				else
				{
					int currentCount = fetchList.Count;
					XElement element = XElement.Parse(e.Response);
					TwitterTimeline timeline = new TwitterTimeline(element);
					//var newmessages = AddNewMessages(timeline).ToList();
					fetchList.AddRange(timeline);//newmessages);
					fetchList = fetchList.Distinct(new CompareTwitters()).ToList();
					//foreach (var mess in newmessages)
					//{
					//    _timelineMessages.Add(mess);
					//}



					if (fetchList.Count > currentCount)
					{
						if (fetchList.Min(m => m.ActualId) > currentID)
						{
							SendLoadingMessage("Loaded " + timeline.Count.ToString() + "... Still loading");
							long newMaxId = fetchList.Min(m => m.ActualId);
							OlderPostsUpTo(currentID, newMaxId);
							return;
						}
					}
					
					_timelineMessages.Clear();
					foreach (var mess in fetchList.OrderBy(m=>m.ActualId))
					{
						_timelineMessages.Add(mess);
					}
					SendLoadedMessage("Finished Loading timeline");
					FirePropertyChanged("Messages");
					CurrentMessage = Messages.FirstOrDefault(m => m.ActualId == currentID);
				}
			};
			if (maxID == 0)
			{
				getUserMessages.AddParameters(Params("count", "200","include_entities","true"));
				//uri = "http://api.twitter.com/1/statuses/home_timeline.xml?count=200";
			}
			else
			{
				getUserMessages.AddParameters(Params("count", "200", "max_id", maxID.ToString()));
				//uri = "http://api.twitter.com/1/statuses/home_timeline.xml?count=200&max_id=" + maxID.ToString();
			}
			getUserMessages.DoGetAsync(new Uri("http://api.twitter.com/1/statuses/home_timeline.xml", UriKind.Absolute));
		}
	}

	public class CompareTwitters : IEqualityComparer<TwitterMessage>
	{
		public bool Equals(TwitterMessage x, TwitterMessage y)
		{
			return x.ActualId == y.ActualId;
		}

		public int GetHashCode(TwitterMessage obj)
		{
			return obj.ActualId.GetHashCode();
		}
	}
}
