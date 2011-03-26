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
using MiniTwitter;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using MessageCloud.Commanding;

namespace MessageCloud
{
	public class AuthViewModel : INotifyPropertyChanged
	{
		private string _OAuthToken;
		public string OAuthToken
		{
			get
			{
				return _OAuthToken;
			}
			set
			{
				_OAuthToken = value;
				FireChanged("OAuthToken");
			}
		}
		private string _OAuthTokenSecret;
		public string OAuthTokenSecret
		{
			get
			{
				return _OAuthTokenSecret;
			}
			set
			{
				_OAuthTokenSecret = value;
				FireChanged("OAuthTokenSecret");
			}
		}

		public AuthViewModel()
		{
			_AuthorizeCommand = new RelayCommand(AuthorizeCommand, CanAuthorizeCommand);
			var settings = IsolatedStorageSettings.ApplicationSettings;
			if (OAuthDetails.IsAuthenticated)
			{
				OAuthToken = OAuthDetails.Token;
				OAuthTokenSecret = OAuthDetails.TokenSecret;
				IsAuthorized = true;
			}
			else
			{
				MyWebClient client = new MyWebClient();
				OAuthHelper oauth = new OAuthHelper();

				oauth.Callback = "oob";
				oauth.ConsumerKey = OAuthDetails.ConsumerKey;
				oauth.ConsumerSecret = OAuthDetails.ConsumerSecret;
				client.OAuthHelper = oauth;
				client.DoPostCompleted += new EventHandler<DoPostCompletedEventArgs>(GotRequestToken);
				client.DoGetAsync(new Uri("https://api.twitter.com/oauth/request_token", UriKind.Absolute));

			}

		}

		public void CheckAuthorization()
		{
			if (IsAuthorized)
			{
				if (AuthorizationSucceeded != null)
				{
					AuthorizationSucceeded(this, new EventArgs());
				}
			}
		}

		void GotRequestToken(object sender, DoPostCompletedEventArgs e)
		{
			string result = e.Response;
			var elements = from element in result.Split('&')
						   let split = element.Split('=')
						   select new { Name = split[0], Value = split[1] };


			if (elements.Any(el => el.Name == "oauth_callback_confirmed" && el.Value == "true"))
			{
				var token = elements.Single(a => a.Name == "oauth_token").Value;
				var tokensecret = elements.Single(a => a.Name == "oauth_token_secret").Value;
				AuthUri = "https://api.twitter.com/oauth/authorize?oauth_token=" + token;
				OAuthToken = token;
				OAuthTokenSecret = tokensecret;
			}

		}

		private string _AuthUri;
		public string AuthUri
		{
			get
			{
				return _AuthUri;
			}
			set
			{
				_AuthUri = value;
				FireChanged("AuthUri");
			}
		}

		private string _Verifier;
		public string Verifier
		{
			get
			{
				return _Verifier;
			}
			set
			{
				_Verifier = value;
				FireChanged("Verifier");
				_AuthorizeCommand.RaiseCanExecuteChanged();
			}
		}

		private RelayCommand _AuthorizeCommand;

		public RelayCommand DoAuthorizeCommand
		{
			get { return _AuthorizeCommand; }
		}

		private bool CanAuthorizeCommand()
		{
			return !string.IsNullOrWhiteSpace(Verifier);
		}

		public event EventHandler AuthorizationSucceeded;

		private void AuthorizeCommand()
		{
			MyWebClient client = new MyWebClient();
			OAuthHelper oauth = new OAuthHelper();

			oauth.Token = OAuthToken;
			oauth.Verifier = Verifier;
			oauth.TokenSecret = OAuthTokenSecret;
			oauth.CreateSignature("https://api.twitter.com/oauth/access_token", "GET");
			string auth = oauth.AuthenticationHeader;
			client.SetHeader("Authorization", auth);
			client.DoPostCompleted += new EventHandler<DoPostCompletedEventArgs>(AuthCompleted);
			client.DoGetAsync(new Uri("https://api.twitter.com/oauth/access_token", UriKind.Absolute));

		}

		void AuthCompleted(object sender, DoPostCompletedEventArgs e)
		{
			string result = e.Response;
			var elements = from element in result.Split('&')
						   let split = element.Split('=')
						   select new { Name = split[0], Value = split[1] };


			if (elements.Any(el => el.Name == "oauth_token"))
			{
				OAuthToken = elements.Single(a => a.Name == "oauth_token").Value;
				OAuthTokenSecret= elements.Single(a => a.Name == "oauth_token_secret").Value;
				var settings = IsolatedStorageSettings.ApplicationSettings;
				settings["oauth_token"] = OAuthToken;
				settings["oauth_token_secret"] = OAuthTokenSecret;
				settings.Save();
				IsAuthorized = true;
				OAuthDetails.Token = OAuthToken;
				OAuthDetails.TokenSecret = OAuthTokenSecret;
				OAuthDetails.IsAuthenticated = true;
				if (AuthorizationSucceeded != null)
				{
					AuthorizationSucceeded(this, new EventArgs());
				}
			}
			
		}

		private bool _IsAuthorized;
		public bool IsAuthorized
		{
			get
			{
				return _IsAuthorized;
			}
			set
			{
				_IsAuthorized = value;
				FireChanged("IsAuthorized");
				FireChanged("IsAuthorizedVisible");
			}
		}
		public Visibility IsAuthorizedVisible
		{
			get { return IsAuthorized ? Visibility.Visible : Visibility.Collapsed; }
		}

		private void FireChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;

	}
}
