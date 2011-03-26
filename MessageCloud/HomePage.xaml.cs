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
using MiniTwitter;
using System.IO.IsolatedStorage;

namespace MessageCloud
{
	public partial class HomePage : UserControl
	{

		private string oauth_token = string.Empty;
		private string oauth_token_secret = string.Empty;

		AuthViewModel authViewModel;

		public HomePage()
		{
			InitializeComponent();
			if (OAuthDetails.IsAuthenticated == false)
			{
				var settings = IsolatedStorageSettings.ApplicationSettings;
				if (settings.Contains("oauth_token"))
				{
					OAuthDetails.Token = settings["oauth_token"].ToString();
					OAuthDetails.TokenSecret = settings["oauth_token_secret"].ToString();
					OAuthDetails.IsAuthenticated = true;
				}
			}
			authViewModel = new AuthViewModel();
			authGrid.DataContext = authViewModel;
			authViewModel.AuthorizationSucceeded += new EventHandler(authViewModel_AuthorizationSucceeded);
			authViewModel.CheckAuthorization();
			Loaded += new RoutedEventHandler(HomePage_Loaded);
		}

		void authViewModel_AuthorizationSucceeded(object sender, EventArgs e)
		{
			authGrid.Visibility = Visibility.Collapsed;
			SetTimelines();
		}

		private void SetTimelines()
		{
			var settings = IsolatedStorageSettings.ApplicationSettings;
			var oauth_token = settings["oauth_token"].ToString();
			var	oauth_token_secret = settings["oauth_token_secret"].ToString();
			OAuthHelper helper = new OAuthHelper();
			//helper.ConsumerKey = CONSUMER_KEY;
			//helper.ConsumerSecret = CONSUMER_SECRET;
			//helper.Token = oauth_token;
			//helper.TokenSecret = oauth_token_secret;
			var defaulttimeline = new MainTimelineViewModel(helper); 
			AddTimeline(defaulttimeline);

			helper = new OAuthHelper();
			//helper.ConsumerKey = CONSUMER_KEY;
			//helper.ConsumerSecret = CONSUMER_SECRET;
			//helper.Token = oauth_token;
			//helper.TokenSecret = oauth_token_secret;

			//AddTimeline(new UserTimelineViewModel("jimlynn",helper));
			timelines.ItemsSource = timelineList;
			twitterPanel1.DataContext = defaulttimeline;
			CurrentTimeline = defaulttimeline;

			maketweet.OAuthHelper = new OAuthHelper();

			foreach (var tl in timelineList)
			{
				tl.MakeTweet += new EventHandler<MakeTweetEventArgs>(tl_MakeTweet);
			}
		}

		void tl_MakeTweet(object sender, MakeTweetEventArgs e)
		{
			if (e.TweetType == TweetType.Reply)
			{
				maketweet.InReplyTo = e.Message;
				maketweet.StartReply();
			}
			else
			{
				maketweet.StartMessage(e.InitialText);
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.Handled)
				return;

			if (e.OriginalSource is TextBox)
			{
				return;
			}

			if (CurrentTimeline != null)
			{
				CurrentTimeline.OnKeyPress(e);
			}

			//switch (e.Key)
			//{
			//    case Key.Left:
			//    case Key.Up:
			//        if (twitterPanel1.SelectedIndex > 0)
			//        {
			//            twitterPanel1.SelectedIndex--;
			//        }
			//        e.Handled = true;
			//        break;
			//    case Key.Down:
			//    case Key.Right:
			//        if ((twitterPanel1.SelectedIndex +1) < twitterPanel1.Items.Count)
			//        {
			//            twitterPanel1.SelectedIndex++;
			//        }
			//        e.Handled = true;
			//        break;
			//    default:
			//        break;
			//}
		}

		void HomePage_Loaded(object sender, RoutedEventArgs e)
		{
			MessageViewModel = statusMessageView.ViewModel;
		}

		StatusMessageViewModel MessageViewModel;

		private void AddTimeline(TimelineViewModel vm)
		{
			timelineList.Add(vm);
			vm.StatusChanged += new EventHandler<StatusChangedEventArgs>(vm_StatusChanged);
		}

		void vm_StatusChanged(object sender, StatusChangedEventArgs e)
		{
			switch (e.Type)
			{
				case StatusMessageType.Loading:
					MessageViewModel.LoadingMessage = e.Message;
					break;
				case StatusMessageType.Loaded:
					MessageViewModel.LoadedMessage = e.Message;
					break;
				case StatusMessageType.Error:
					MessageViewModel.ErrorMessage = e.Message;
					break;
				case StatusMessageType.Normal:
					MessageViewModel.Message = e.Message;
					break;
				default:
					break;
			}
		}

		ObservableCollection<TimelineViewModel> timelineList = new ObservableCollection<TimelineViewModel>();

		TimelineViewModel CurrentTimeline;

		private void StatusView_Action(object sender, StatusClickEventArgs e)
		{
			if (CurrentTimeline != null)
			{
				CurrentTimeline.OnStatusViewAction(e);
			}
			if (e.Handled)
			{
				return;
			}
			if (e.Type == ActionType.ShowImage)
			{
				imagePopup.Source = new Uri(e.ImageUri, UriKind.Absolute);
			}
		}

		private void NewTimelineSelected(object sender, SelectionChangedEventArgs e)
		{
			//CurrentTimeline = (TimelineViewModel)timelines.SelectedItem;
			//twitterPanel1.ItemsSource = CurrentTimeline.Messages;
		}

		private void KeyHandler(object sender, KeyEventArgs e)
		{
			//if (e.OriginalSource.GetType().Name != "TextBox")
			//{
			//    if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.Space)
			//    {
			//        if (twitterPanel1.SelectedItem == null)
			//        {
			//            twitterPanel1.SelectedIndex = 0;
			//        }
			//        else
			//        {
			//            twitterPanel1.SelectedIndex++;
			//        }
			//        e.Handled = true;
			//    }
			//    if (e.Key == Key.Left || e.Key == Key.Up)
			//    {
			//        if (twitterPanel1.SelectedItem == null)
			//        {
			//            twitterPanel1.SelectedIndex = 0;
			//        }
			//        else
			//        {
			//            twitterPanel1.SelectedIndex--;
			//        }
			//    }
			//}
		}

		private void SelectTimeline_Click(object sender, RoutedEventArgs e)
		{
			CurrentTimeline = ((Button)sender).DataContext as TimelineViewModel;
			twitterPanel1.DataContext = CurrentTimeline;
		}
	}
}
