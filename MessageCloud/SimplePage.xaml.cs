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
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using MessageCloud.ServiceReference1;
using System.Xml.Linq;
using System.Collections;
using System.Net.Browser;
using System.Windows.Controls.Primitives;
using System.Windows.Browser;

namespace MessageCloud
{
	public partial class SimplePage : UserControl
	{

		public SimplePage()
		{
			InitializeComponent();
			Loaded += new RoutedEventHandler(SimplePage_Loaded);
		}

		StatusMessageViewModel MessageViewModel;

		void SimplePage_Loaded(object sender, RoutedEventArgs e)
		{
			MessageViewModel = messageView.ViewModel;
			//items.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
		}

		void ItemContainerGenerator_ItemsChanged(object sender, ItemsChangedEventArgs e)
		{
			//var item = items.ItemContainerGenerator.ContainerFromIndex(currentIndex);
			//if (item != null)
			//{
			//    MoveToItem(currentIndex);
			//}
		}

		private ObservableCollection<AnimatableMessage> messages = new ObservableCollection<AnimatableMessage>();

		private List<AnimatableMessage> messageList = new List<AnimatableMessage>();

		private void StatusView_Action(object sender, StatusClickEventArgs e)
		{
			if (e.Type == ActionType.ShowReplies)
			{
				TwitterMessage message = e.Message;
				FetchReply(message, (StatusView)sender);
			}
			else if (e.Type == ActionType.UserPrevious)
			{

				var previous = from m in messages
							   where m.TwitterMessage.Id < e.Message.Id && m.TwitterMessage.User.UserID == e.Message.User.UserID
							   orderby m.TwitterMessage.Id descending
							   select m;
				if (previous.Count() > 0)
				{
					var prev = previous.First();
					currentIndex = messages.IndexOf(prev);
					MoveToItem(currentIndex);
				}
			}
			else if (e.Type == ActionType.UserNext)
			{
				var next = from m in messages
							   where m.TwitterMessage.Id > e.Message.Id && m.TwitterMessage.User.UserID == e.Message.User.UserID
							   orderby m.TwitterMessage.Id
							   select m;
				if (next.Count() > 0)
				{
					var nextmess = next.First();
					currentIndex = messages.IndexOf(nextmess);
					MoveToItem(currentIndex);
				}

			}
			else if (e.Type == ActionType.RetweetUsernameClicked)
			{
				
			}
			else if (e.Type == ActionType.ReplyToThis)
			{
				makeTweet.InReplyTo = e.Message;
				makeTweet.StartReply();
			}
		}

		private void FetchReply(TwitterMessage message, StatusView statusView)
		{
			MessageViewModel.LoadingMessage = "Fetching previous message";
			if (message.InReplyToId > 0)
			{
				WebClient client = new WebClient();
				//TwitterClient client = new TwitterClient();
				client.DownloadStringCompleted += (o, e) =>
				{
					if (e.Error == null)
					{
						TwitterMessage newMessage = new TwitterMessage(XElement.Parse(e.Result));
						// Create a new
						var animatee = new AnimatableMessage() { TwitterMessage = newMessage };
						var parentMessage = messages.First(a => a.TwitterMessage == message);
						int index = messages.IndexOf(parentMessage);
						messages.Insert(index, animatee);
						animatee.X = parentMessage.X;
						animatee.Y = parentMessage.Y + 300;
						animatee.Z = parentMessage.Z;
						MessageViewModel.LoadedMessage = "Reply loaded";
						//MoveToItem(index);
						items.SelectedItem = animatee;
						items.InvalidateMeasure();
					}
					else
					{
						MessageViewModel.ErrorMessage = "Failed to load reply";
					}
				};
				client.DownloadStringAsync(new Uri(string.Format("http://api.twitter.com/1/statuses/show/{0}.xml",message.InReplyToId), UriKind.Absolute));
				//client.Credentials = new NetworkCredential("jimlynn", "xxxxxxxxxxxxxxxxx");
				//string result = client.DownloadString(string.Format("http://api.twitter.com/1/statuses/show/{0}.xml", statusID));
				//return result;

			}
		}
		Random rnd = new Random();

		private void FetchPosts(object sender, RoutedEventArgs e)
		{
			messageView.ViewModel.Message = "Fetching posts";
			messageView.ViewModel.State = "InProgress";
			var currentID = 15315276793L;
			//if (IsolatedStorageSettings.ApplicationSettings.Contains("CurrentMessage"))
			//{
			//    currentID = (Int64)IsolatedStorageSettings.ApplicationSettings["CurrentMessage"];
			//}

			//TwitterClient twitter = new TwitterClient();
			WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
			WebClient twitter = new WebClient();
			twitter.DownloadStringCompleted += (o, a) =>
			{
				if (a.Error == null)
				{
					XElement element = XElement.Parse(a.Result);
					TwitterTimeline timeline = new TwitterTimeline(element);
					double depth = -300;
					if (messages.Count > 0)
					{
						depth = messages.OrderBy(m => m.Z).First().Z - 1000;
					}
					var newmessages = (from newm in timeline
									   join old in messages on newm.ActualId equals old.TwitterMessage.ActualId
									   into joined
									   from leftjoin in joined.DefaultIfEmpty()
									   where leftjoin == null
									   orderby newm.ActualId
									   select newm).ToList();
					messageView.ViewModel.Message = string.Format("Loaded {0} messages", newmessages.Count);
					foreach (var mess in newmessages)
					{
						messages.Add(new AnimatableMessage 
						{
							TwitterMessage = mess,
							X = rnd.NextDouble() * 1200 - 600,
							Y = rnd.NextDouble() * 1200 - 600,
							Z = depth
						});
						depth -= 1000;
					}
					items.ItemsSource = messages;
					var curmess = messages.FirstOrDefault(m => m.TwitterMessage.Id == currentID);
					if (curmess != null)
					{
						currentIndex = messages.IndexOf(curmess);
						//MoveToItem(currentIndex);
					}
				}
				else
				{
					messageView.ViewModel.Message= "Error while fetching timeline";
				}
			};
			string url = "http://api.twitter.com/1/statuses/home_timeline.xml?count=200";
			twitter.DownloadStringAsync(new Uri(url, UriKind.Absolute));
			//twitter.GetTimelineSinceAsync(12345L);

		}

		int currentIndex = 0;

		private void UserControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Handled)
				return;
			if (e.OriginalSource.GetType().Name != "TextBox")
			{
				if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.Space)
				{
					currentIndex++;
					if (currentIndex == messages.Count)
					{
						currentIndex = 0;
					}
					MoveToItem(currentIndex);
				}
				else if (e.Key == Key.Left || e.Key == Key.Up)
				{
					currentIndex--;
					if (currentIndex < 0)
					{
						currentIndex = messages.Count - 1;
					}
					MoveToItem(currentIndex);
				}
				else if (e.Key == Key.Home)
				{
					currentIndex = 0;
					MoveToItem(currentIndex);
				}
				else if (e.Key == Key.End)
				{
					currentIndex = messages.Count - 1;
					MoveToItem(currentIndex);
				}
				else if (e.Key == Key.PageUp)
				{
					currentIndex -= 25;
					if (currentIndex < 0)
					{
						currentIndex = 0;
					}
					MoveToItem(currentIndex);
				}
				else if (e.Key == Key.PageDown)
				{
					currentIndex += 25;
					if (currentIndex >= messages.Count)
					{
						currentIndex = messages.Count - 1;
					}
					MoveToItem(currentIndex);
				}
			}
		}

		private void MoveToItem(int index)
		{
			//items.MoveToItem(currentIndex);
			items.SelectedIndex = currentIndex;
			SetCurrentItem(messages[index]);
		}

		private void SetCurrentItem(AnimatableMessage message)
		{
			var store = IsolatedStorageSettings.ApplicationSettings;
			store["CurrentMessage"] = message.TwitterMessage.Id;
			IsolatedStorageSettings.ApplicationSettings.Save();
		}

		private void items_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (items.SelectedItem != null)
			{
				var item = items.SelectedItem as AnimatableMessage;
				SetCurrentItem(item);
			}
		}

		private void FetchTestPosts(object sender, RoutedEventArgs e)
		{
			WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
			var currentID = 12345L;
			if (IsolatedStorageSettings.ApplicationSettings.Contains("CurrentMessage"))
			{
				currentID = (Int64)IsolatedStorageSettings.ApplicationSettings["CurrentMessage"];
			}
			WebClient client = new WebClient();
			//client.Credentials = new NetworkCredential("jimlynn", "xxxxxxxxxxxxxxxx");
			client.DownloadStringCompleted += (o, a) =>
			{
				if (a.Error == null)
				{
					//messages.Clear();
					XElement element = XElement.Parse(a.Result);
					TwitterTimeline timeline = new TwitterTimeline(element);
					double depth = -300;
					if (messages.Count > 0)
					{
						depth = messages.OrderBy(m => m.Z).First().Z - 1000;
					}
					var cut = timeline.Take(4);
					foreach (var mess in cut)
					{
						messages.Add(new AnimatableMessage
						{
							TwitterMessage = mess,
							X = rnd.NextDouble() * 1200 - 600,
							Y = rnd.NextDouble() * 1200 - 600,
							Z = depth
						});
						depth -= 1000;
					}
					items.ItemsSource = messages;

					var curmess = messages.FirstOrDefault(m => m.TwitterMessage.Id == currentID);
					if (curmess != null)
					{
						currentIndex = messages.IndexOf(curmess);
						//MoveToItem(currentIndex);
					}
				}
				else
				{

				}

			};
			//client.DownloadStringAsync(new Uri("home_timeline.xml", UriKind.Relative));
			client.DownloadStringAsync(new Uri("http://api.twitter.com/1/statuses/home_timeline.xml?since_id=" + currentID.ToString(), UriKind.Absolute));
		}

		private void ClearMessages(object sender, RoutedEventArgs e)
		{
			messages.Clear();
		}

		private void ShowTree(object sender, RoutedEventArgs e)
		{
			VisualTreeItem root = new VisualTreeItem(items);
			treeView1.ItemsSource = root;
		}

		private void OlderPosts(object sender, RoutedEventArgs e)
		{
			MessageViewModel.LoadingMessage = "Loading older posts";
			var earliestID = 0L;
			if (messages.Count == 0)
			{
				return;
			}

			earliestID = messages.Min(m => m.TwitterMessage.ActualId);

			//TwitterClient twitter = new TwitterClient();
			WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
			WebClient twitter = new WebClient();
			twitter.DownloadStringCompleted += (o, a) =>
			{
				if (a.Error == null)
				{
					XElement element = XElement.Parse(a.Result);
					TwitterTimeline timeline = new TwitterTimeline(element);
					var newmessages = (from newm in timeline
									   join old in messages on newm.ActualId equals old.TwitterMessage.ActualId
									   into joined
									   from leftjoin in joined.DefaultIfEmpty()
									   where leftjoin == null
									   orderby newm.ActualId descending
									   select newm).ToList();
					MessageViewModel.LoadedMessage = string.Format("Loaded {0} new messages", newmessages.Count);
					foreach (var mess in newmessages)
					{
						messages.Insert(0,new AnimatableMessage
						{
							TwitterMessage = mess,
							X = rnd.NextDouble() * 1200 - 600,
							Y = rnd.NextDouble() * 1200 - 600
						});
					}
					int depth = -300;
					foreach (var tmess in messages)
					{
						tmess.Z = depth;
						depth -= 1000;
					}
					items.ItemsSource = messages;
				}
				else
				{
					MessageViewModel.ErrorMessage = "Error while fetching timeline";
				}
			};
			string url = "http://api.twitter.com/1/statuses/home_timeline.xml?count=200&max_id=" + earliestID.ToString();
			twitter.DownloadStringAsync(new Uri(url, UriKind.Absolute));

		}

		private void OlderPostsUpTo(long oldestId, List<TwitterMessage> alreadyLoaded)
		{
			MessageViewModel.LoadingMessage = "Loading older posts";
			//TwitterClient twitter = new TwitterClient();
			WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
			WebClient twitter = new WebClient();
			twitter.DownloadStringCompleted += (o, a) =>
			{
				if (a.Error == null)
				{
					XElement element = XElement.Parse(a.Result);
					TwitterTimeline timeline = new TwitterTimeline(element);
					var newmessages = (from newm in timeline
									   join old in alreadyLoaded on newm.ActualId equals old.ActualId
									   into joined
									   from leftjoin in joined.DefaultIfEmpty()
									   where leftjoin == null
									   orderby newm.ActualId descending
									   select newm).ToList();
					alreadyLoaded.AddRange(newmessages);

					if (alreadyLoaded.Min(m => m.ActualId) <= oldestId || newmessages.Count == 0)
					{
						MessageViewModel.LoadedMessage = string.Format("Loaded {0} new messages", alreadyLoaded.Count);
						foreach (var mess in alreadyLoaded)
						{
							messages.Insert(0, new AnimatableMessage
							{
								TwitterMessage = mess,
								X = rnd.NextDouble() * 1200 - 600,
								Y = rnd.NextDouble() * 1200 - 600
							});
						}
						int depth = -300;
						foreach (var tmess in messages)
						{
							tmess.Z = depth;
							depth -= 1000;
						}
						var targetMessage = messages.FirstOrDefault(m=>m.TwitterMessage.ActualId == oldestId);
						if (targetMessage != null)
						{
							currentIndex = messages.IndexOf(targetMessage);
						}
						else
						{
							currentIndex = 0;
						}
						items.ItemsSource = messages;
						items.SelectedItem = targetMessage;
					}
					else
					{
						MessageViewModel.LoadingMessage = string.Format("Loaded {0} new messages, fetching more", alreadyLoaded.Count);
						OlderPostsUpTo(oldestId, alreadyLoaded);
					}
				}
				else
				{
					MessageViewModel.ErrorMessage = "Error while fetching timeline";
				}
			};
			string url = null;
			var earliestID = 0L;
			if (alreadyLoaded.Count == 0)
			{
				url = "http://api.twitter.com/1/statuses/home_timeline.xml?count=200";
			}
			else
			{
				earliestID = alreadyLoaded.Min(m => m.ActualId);
				url = "http://api.twitter.com/1/statuses/home_timeline.xml?count=200&max_id=" + earliestID.ToString();
			}
			twitter.DownloadStringAsync(new Uri(url, UriKind.Absolute));
		}

		private void NewerPosts(object sender, RoutedEventArgs e)
		{
			MessageViewModel.LoadingMessage = "Loading newer posts";
			if (messages.Count > 0)
			{
				var currentMax = messages.Max(m => m.TwitterMessage.ActualId);
				WebClient client = new WebClient();
				client.DownloadStringCompleted += (o, a) =>
				{
					if (a.Error != null)
					{
						MessageViewModel.ErrorMessage= "Error fetching new messages";
					}
					else
					{
						XElement element = XElement.Parse(a.Result);
						var timeline = new TwitterTimeline(element);
						double depth = -300;
						if (messages.Count > 0)
						{
							depth = messages.OrderBy(m => m.Z).First().Z - 1000;
						}
						var newmessages = (from newm in timeline
										   join old in messages on newm.ActualId equals old.TwitterMessage.ActualId
										   into joined
										   from leftjoin in joined.DefaultIfEmpty()
										   where leftjoin == null
										   orderby newm.ActualId
										   select newm).ToList();
						MessageViewModel.LoadedMessage = string.Format("Loaded {0} messages", newmessages.Count);
						foreach (var mess in newmessages)
						{
							messages.Add(new AnimatableMessage
							{
								TwitterMessage = mess,
								X = rnd.NextDouble() * 1200 - 600,
								Y = rnd.NextDouble() * 1200 - 600,
								Z = depth
							});
							depth -= 1000;
						}
						items.ItemsSource = messages;
					}
				};
				string url = "http://api.twitter.com/1/statuses/home_timeline.xml?count=200&since_id=" + currentMax.ToString();
				client.DownloadStringAsync(new Uri(url, UriKind.Absolute));
			}
		}

		private void FetchUpTo(object sender, RoutedEventArgs e)
		{

			var currentID = 15315276793L;
			if (IsolatedStorageSettings.ApplicationSettings.Contains("CurrentMessage"))
			{
				currentID = (Int64)IsolatedStorageSettings.ApplicationSettings["CurrentMessage"];
			}

			OlderPostsUpTo(currentID, new List<TwitterMessage>());

		}

		private void MoveToCurrent(object sender, RoutedEventArgs e)
		{
			items.BringIntoView(currentIndex);
		}

		private void SetMessage(object sender, RoutedEventArgs e)
		{
			MessageViewModel.LoadedMessage = "This is a loaded message";
		}



	}

	public class VisualTreeItem : IEnumerable<VisualTreeItem>
	{
		public VisualTreeItem(DependencyObject treeObject)
		{
			_treeObject = treeObject;
			int count = treeObject.CountVisualChildren();
			for (int i = 0; i < count; i++)
			{
				_items.Add(new VisualTreeItem(treeObject.VisualChild(i)));
			}
		}

		private List<VisualTreeItem> _items = new List<VisualTreeItem>();
		public List<VisualTreeItem> Items
		{
			get
			{
				return _items;
			}
		}

		private DependencyObject _treeObject;

		public string Name
		{
			get
			{
				string name = (string)_treeObject.GetValue(FrameworkElement.NameProperty);
				return name;
			}
		}

		public string TypeName
		{
			get
			{
				return _treeObject.GetType().Name;
			}
		}

		public string Coords
		{
			get
			{
				if (_treeObject.GetValue(Panel3D.XProperty) != null)
				{
					return string.Format("{0}, {1}, {2}", _treeObject.GetValue(Panel3D.XProperty).ToString(),
						_treeObject.GetValue(Panel3D.YProperty).ToString(),
						_treeObject.GetValue(Panel3D.ZProperty).ToString());
				}
				return null;
			}
		}

		public string Projection
		{
			get
			{
				FrameworkElement element = _treeObject as FrameworkElement;
				if (element != null)
				{
					if (element.Projection != null && element.Projection is PlaneProjection)
					{
						PlaneProjection projection = element.Projection as PlaneProjection;
						return string.Format("{0}, {1}, {2}", projection.GlobalOffsetX, projection.GlobalOffsetY, projection.GlobalOffsetZ);
					}
				}
				return null;
			}
		}

		public IEnumerator<VisualTreeItem> GetEnumerator()
		{
			return _items.GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return _items.GetEnumerator();
		}
	}
}