using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace MiniTwitter
{
	public class OAuthHelper
	{
		public OAuthHelper()
		{
			ConsumerKey = OAuthDetails.ConsumerKey;
			ConsumerSecret = OAuthDetails.ConsumerSecret;
			if (OAuthDetails.IsAuthenticated)
			{
				Token = OAuthDetails.Token;
				TokenSecret = OAuthDetails.TokenSecret;
			}
		}

		private Dictionary<string, string> parameters = new Dictionary<string, string>();
		
		public void AddParameter(string name, string value)
		{
			if (parameters.ContainsKey(name))
			{
				parameters.Remove(name);
			}
			parameters.Add(name, value);
		}

		public string ConsumerKey
		{
			get
			{
				return parameters["oauth_consumer_key"];
			}
			set
			{
				AddParameter("oauth_consumer_key", value);
			}
		}

		public string Token
		{
			get { return parameters["oauth_token"]; }
			set { AddParameter("oauth_token", value); }
		}

		public string Verifier
		{
			get { return parameters["oauth_verifier"]; }
			set { AddParameter("oauth_verifier", value); }
		}

		public string Callback
		{
			get { return parameters["oauth_callback"]; }
			set { AddParameter("oauth_callback", value); }
		}

		public string ConsumerSecret { get; set; }
		public string TokenSecret { get; set; }
		public void CreateSignature(string baseUri, string method)
		{
			CreateSignature(baseUri, method, parameters);
		}
		
		public void CreateSignature(string baseUri,string method, IEnumerable<KeyValuePair<string,string>> parameters)
		{
			if (string.IsNullOrEmpty(ConsumerSecret))
			{
				throw new ArgumentException("Consumer Secret not set");
			}
			string consumerSecret = ConsumerSecret;
			string tokenSecret = TokenSecret ?? string.Empty;

			AddParameter("oauth_nonce", MakeNonce());
			AddParameter("oauth_signature_method", "HMAC-SHA1");
			AddParameter("oauth_timestamp", Timestamp());
			AddParameter("oauth_version", "1");
			this.parameters.Remove("oauth_signature");

			parameters = parameters.Union(this.parameters);

			StringBuilder sb = new StringBuilder();
			sb.Append(method+"&");
			sb.Append(Uri.EscapeDataString(baseUri) + "&");

			bool first = true;
			foreach (var param in parameters.OrderBy(kv => kv.Key))
			{
				if (!first)
				{
					sb.Append("%26");
				}
				first = false;
				sb.Append(Uri.EscapeDataString(param.Key));
				sb.Append("%3D");
				//sb.Append(Uri.EscapeDataString(Uri.EscapeDataString(param.Value)));
				sb.Append(EscapeStrict(param.Value));
			}

			string secret = consumerSecret + "&" + (tokenSecret ?? "");
			HMACSHA1 hasher = new HMACSHA1(Encoding.UTF8.GetBytes(secret));
			var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
			string sig = Convert.ToBase64String(hash);
			AddParameter("oauth_signature", sig);
		}

		private string EscapeStrict(string text)
		{
			string result = Uri.EscapeDataString(text);
			result = result.Replace("!", "%21");
			result = result.Replace("*", "%2A");
			result = result.Replace("'", "%27");
			result = result.Replace("(", "%28");
			result = result.Replace(")", "%29");
			return Uri.EscapeDataString(result);
		}

		private string Timestamp()
		{
			return ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
		}

		private static Random rnd = new Random();

		private string MakeNonce()
		{
			const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

			var nonce = new char[16];
			lock (rnd)
			{
				for (var i = 0; i < nonce.Length; i++)
				{
					nonce[i] = chars[rnd.Next(0, chars.Length)];
				}
			}
			return new string(nonce);

		}


		public string AuthenticationHeader
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("OAuth ");
				sb.Append(string.Join(",", from p in parameters select p.Key + "=\"" + Uri.EscapeDataString(p.Value)+"\""));
				return sb.ToString();
			}
		}
	}
	public static class OAuthDetails
	{
		public static string ConsumerKey { get { return "NDveqLnJ9lf6PsCf5MgDQ"; } }
		public static string ConsumerSecret { get { return "xro4UzRypElk3rMzQxYzzR0QiPt70pqad0UZwDyxc"; } }
		public static string Token { get; set; }
		public static string TokenSecret { get; set; }
		public static bool IsAuthenticated { get; set; }
	}
}
