using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Browser;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using MessageCloud.ServiceReference1;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;

namespace MessageCloud
{
	public partial class MainPage : UserControl
	{
		ObservableCollection<Message> messages;
		int frontmost = 0;

		Random rnd = new Random();

		public MainPage()
		{
			InitializeComponent();
			//messages = new ObservableCollection<Message>()
			//{
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic"),
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic"),
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic"),
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic"),
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic"),
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic"),
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic"),
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic"),
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic"),
			//    new Message("Hello you"),
			//    new Message("This is a nice message"),
			//    new Message("The Quick Brown Fox"),
			//    new Message("Once upon a time in the west"),
			//    new Message("Don't Panic")
			//};

		}

		void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			double speed = 10;
			Message frontMessage = messages[frontmost];
			double zdistance = -frontMessage.Z;
			double delta, xOffset, yOffset;
			if (zdistance != 0)
			{
				delta = (zdistance - speed) / zdistance;

				xOffset = frontMessage.X - frontMessage.X * delta;
				yOffset = frontMessage.Y - frontMessage.Y * delta;
			}
			else
			{
				delta = 1;
				xOffset = 0;
				yOffset = 0;
			}
			double deepest = 0;

			Message topmost = null;

			for (int i = 0; i < messages.Count; i++)
			{
				Message mess = messages[i];
				mess.Z += speed;
				mess.X -= xOffset;
				mess.Y -= yOffset;

				if (mess.Z < deepest)
				{
					deepest = mess.Z;
				}
				if (mess.Z >= 1000)
				{
					topmost = mess;
				}
			}
			if (messages[frontmost].Z >= 0)
			{
				//messages[frontmost].Depth = deepest - 2000;
				frontmost -= 1;
				if (frontmost == -1)
				{
					frontmost = messages.Count - 1;
				}
			}
			if (topmost != null)
			{
				topmost.Z = deepest - 2000;
			}
		}

		private ObservableCollection<AnimatableMessage> animatemessages = new ObservableCollection<AnimatableMessage>();
		private void GetTimeline_Click(object sender, RoutedEventArgs e)
		{
			animatemessages = new ObservableCollection<AnimatableMessage>();
			mainBoard.Stop();
			mainBoard.Children.Clear();
			var currentID = 12345L;
			if (IsolatedStorageSettings.ApplicationSettings.Contains("CurrentMessage"))
			{
				currentID = (Int64)IsolatedStorageSettings.ApplicationSettings["CurrentMessage"];
			}
			TwitterClient twitter = new TwitterClient();
			twitter.GetTimelineSinceCompleted += (o, a) =>
				{
					if (a.Error == null)
					{
						XElement element = XElement.Parse(a.Result);
						TwitterTimeline timeline = new TwitterTimeline(element);
						int zorder = timeline.Count;
						foreach (var tmess in timeline)
						{
							var mess = new AnimatableMessage { TwitterMessage = tmess };
							animatemessages.Insert(0, mess);
							//animatemessages.Add(mess);
						}
						listbox.ItemsSource = null;
						listbox.ItemsSource = animatemessages;
						
						frontmost = animatemessages.Count - 1;
						//CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
						BuildStoryboards(animatemessages);
						currentItem = animatemessages.Count - 1;
						var curmess = animatemessages.FirstOrDefault(m => m.TwitterMessage.Id == currentID);
						if (curmess != null)
						{
							currentItem = animatemessages.IndexOf(curmess);
						}
						MoveToMessage(currentItem);
					}
				};
			twitter.GetTimelineSinceAsync(12345L);
		}

		private void GetTimelineSince(Int64 sinceID, Action<List<AnimatableMessage>> handler)
		{
			TwitterClient client = new TwitterClient();

			client.GetTimelineSinceCompleted += (o, a) =>
			{
				if (a.Error == null)
				{
					XElement element = XElement.Parse(a.Result);
					TwitterTimeline timeline = new TwitterTimeline(element);
					List<AnimatableMessage> list = new List<AnimatableMessage>();
					var x = from m in timeline
							select new AnimatableMessage { TwitterMessage = m };
					handler(x.ToList());
				}
			};
			client.GetTimelineSinceAsync(sinceID);
		}

		void client_GetTimelineSinceCompleted(object sender, GetTimelineSinceCompletedEventArgs e)
		{
			
		}

		private Storyboard mainBoard = new Storyboard();
		private void BuildStoryboards(IEnumerable<AnimatableMessage> messages)
		{
			BuildStoryboards(messages, -300);
		}

		private void BuildStoryboards(IEnumerable<AnimatableMessage> messages, double depth)
		{
			mainBoard.Stop();
			mainBoard.Duration = new Duration(TimeSpan.FromSeconds(2));
			int x = -3200;
			int y = -3200;
			foreach (var mess in messages.Reverse())
			{
				var timelines = mess.GetAnimations();
				mess.AnimateTo(rnd.Next(3200) - 1600, rnd.Next(3200) - 1600, depth);
				//mess.AnimateTo(x, y, depth);
				//y += 800;
				//if (y > 3200)
				//{
				//    y = -3200;
				//    x += 800;
				//    if (x > 3200)
				//    {

				//        depth -= 1000;
				//        x = -3200;
				//    }

				depth -= 1000;
				foreach (var timeline in mess.GetAnimations())
				{
					mainBoard.Children.Add(timeline);
				}
			}
		}

		int currentItem = 0;

		private void Next_Click(object sender, RoutedEventArgs e)
		{
			currentItem--;
			if (currentItem < 0)
			{
				currentItem = animatemessages.Count - 1;
			}
			MoveToMessage(currentItem);
		}

		private void MoveToMessageId(Int64 id)
		{
			AnimatableMessage message = animatemessages.FirstOrDefault(m => m.TwitterMessage.Id == id);
			int index = animatemessages.IndexOf(message);
			MoveToMessage(index);
		}

		private void MoveToMessage(int item)
		{
			var diffs = animatemessages[item].AnimateTo(0, 0, 0);
			for (int i = 0; i < animatemessages.Count; i++)
			{
				if (i != item)
				{
					animatemessages[i].AnimateBy(diffs.Item1, diffs.Item2, diffs.Item3);
				}
			}
			SetCurrentItem(animatemessages[item]);
			mainBoard.Begin();
		}

		private void SetCurrentItem(AnimatableMessage message)
		{
			var store = IsolatedStorageSettings.ApplicationSettings;
			store["CurrentMessage"] = message.TwitterMessage.Id;
			IsolatedStorageSettings.ApplicationSettings.Save();
		}

		private void Previous_Click(object sender, RoutedEventArgs e)
		{
			currentItem++;
			if (currentItem >= animatemessages.Count)
			{
				currentItem = 0;
			}
			MoveToMessage(currentItem);
		}

		private void UserControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Left)
			{
				Previous_Click(this, new RoutedEventArgs());
			}
			else if (e.Key == Key.Right)
			{
				Next_Click(this, new RoutedEventArgs());
			}
			else if (e.Key == Key.Home)
			{
				var reply = animatemessages.FirstOrDefault(a => a.TwitterMessage.IsReply);
				if (reply != null)
				{
					MoveToMessage(animatemessages.IndexOf(reply));
				}
			}
		}

		private void StatusView_ShowRepliesClicked(object sender, StatusClickEventArgs e)
		{
			TwitterMessage message = e.Message;
			FetchReply(message, (StatusView)sender);
		}

		private void FetchReply(TwitterMessage message, StatusView statusView)
		{
			if (message.InReplyToId > 0)
			{
				TwitterClient client = new TwitterClient();
				client.GetStatusCompleted += (o, e) =>
				{
					if (e.Error == null)
					{
						TwitterMessage newMessage = new TwitterMessage(XElement.Parse(e.Result));
						// Create a new
						var animatee = new AnimatableMessage() { TwitterMessage = newMessage };
						var timelines = animatee.GetAnimations();
						var parentMessage = animatemessages.First(a => a.TwitterMessage == message);
						int index = animatemessages.IndexOf(parentMessage);
						animatemessages.Insert(index, animatee);
						mainBoard.Stop();
						foreach (var timeline in timelines)
						{
							mainBoard.Children.Add(timeline);
						}
						animatee.AnimateTo(parentMessage.X, parentMessage.Y + 300, parentMessage.Z);
						MoveToMessage(index);
					}
				};
				client.GetStatusAsync(message.InReplyToId);
			}
		}

		private void Reverse_Click(object sender, RoutedEventArgs e)
		{
			//Int64 id = Convert.ToInt64(tbid.Text);
			//MoveToMessageId(id);
			//HttpWebRequest request = (HttpWebRequest)WebRequestCreator.ClientHttp.Create(new Uri("http://api.twitter.com/1/statuses/home_timeline.xml", UriKind.Absolute));
			//request.Method = "GET";
			//request.Credentials = new NetworkCredential("jimlynn", "xxxxxxxxxxxx");
			//request.BeginGetResponse(new AsyncCallback(GetHead), request);
			mainBoard.Stop();
		}

		private void GetHead(IAsyncResult result)
		{
			HttpWebRequest request = (HttpWebRequest)result.AsyncState;
			HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				string content = reader.ReadToEnd();
			}
		}

		private void Invert_Click(object sender, RoutedEventArgs e)
		{
			double depth = -300;
			int zorder = 1000000;
			foreach (var mess in animatemessages)
			{
				mess.AnimateTo(rnd.Next(3200) - 1600, rnd.Next(3200) - 1600, depth);
				mess.ZOrder = zorder;
				zorder -= 1;
				depth -= 1000;
			}
			MoveToMessage(0);
			
		}

		private void More_Click(object sender, RoutedEventArgs e)
		{
			var bottomMessage = animatemessages.OrderByDescending(m => m.TwitterMessage.Id).FirstOrDefault();
			var HighestID = bottomMessage.TwitterMessage.Id;
			GetTimelineSince(HighestID, (list) =>
				{
					double newdepth = bottomMessage.CurrentDepth - 1000;
					BuildStoryboards(list, newdepth);
					foreach (var tm in list)
					{
						animatemessages.Add(tm);
					}
					MoveToMessageId(HighestID);
				});
		}


	}

	internal class BoardItem
	{
		public Message Message { get; set; }
		public DoubleAnimation X { get; set; }
		public DoubleAnimation Y { get; set; }
		public DoubleAnimation Z { get; set; }

		public BoardItem(Message message)
		{
			Message = message;
			X = new DoubleAnimation();
			
		}
	}
}
