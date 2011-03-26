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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Browser;
using System.Windows.Threading;
using System.Linq;
using MiniTwitter.Utils;

namespace MiniTwitter
{
	public class MyWebClient
	{
		public event EventHandler<DoPostCompletedEventArgs> DoPostCompleted;

		private Dictionary<string, string> parameters = new Dictionary<string, string>();
		private Dictionary<string, string> headers = new Dictionary<string, string>();

		public void AddParameter(string name, string value)
		{
			parameters.Add(name, value);
		}

		public void AddParameters(IEnumerable<KeyValuePair<string, string>> parameters)
		{
			foreach (var param in parameters)
			{
				AddParameter(param.Key, param.Value);
			}
		}


		public OAuthHelper OAuthHelper { get; set; }

		public void DoPostAsync(Uri uri)
		{
			WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);

			if (OAuthHelper != null)
			{
				OAuthHelper.CreateSignature(uri.ToString(), "POST", parameters);
				string auth = OAuthHelper.AuthenticationHeader;
				SetHeader("Authorization", auth);
			}

			var request = WebRequest.CreateHttp(uri);
			request.Method = "POST";

			foreach (var header in headers)
			{
				request.Headers[header.Key] = header.Value;
			}

			request.ContentType = "application/x-www-form-urlencoded";
			request.BeginGetRequestStream(new AsyncCallback(SendData), request);
		}

		public void DoGetAsync(Uri uri)
		{
			string encoded = string.Empty;
			if (parameters.Count > 0)
			{
				encoded = EncodeParameters();
				string originalUri = uri.ToString();
				if (originalUri.Contains("?"))
				{
					encoded = "&" + EncodeParameters();
				}
				else
				{
					encoded = "?" + EncodeParameters();
				}
			}
			WebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);
			if (OAuthHelper != null)
			{
				OAuthHelper.CreateSignature(uri.ToString(), "GET", parameters);
				string auth = OAuthHelper.AuthenticationHeader;
				SetHeader("Authorization", auth);
			}
			var request = WebRequest.CreateHttp(uri + encoded);
			request.Method = "GET";
			foreach (var header in headers)
			{
				request.Headers[header.Key] = header.Value;
			}
			request.BeginGetResponse(GetResponse, request);
		}


		public void SetHeader(string header, string value)
		{
			headers.Add(header, value);
		}

		private void SendData(IAsyncResult asyncResult)
		{
			var request = asyncResult.AsyncState as HttpWebRequest;
			using (Stream stream = request.EndGetRequestStream(asyncResult))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					writer.Write(EncodeParameters());
					writer.Flush();
					writer.Close();
				}
			}
			request.BeginGetResponse(GetResponse, request);
		}

		private void GetResponse(IAsyncResult asyncResult)
		{
			var request = asyncResult.AsyncState as HttpWebRequest;
			try
			{
				var response = request.EndGetResponse(asyncResult) as HttpWebResponse;
				var result = new DoPostCompletedEventArgs();
				using (Stream responseStream = response.GetResponseStream())
				{
					StreamReader reader = new StreamReader(responseStream);
					// get the result text  
					result.Response = reader.ReadToEnd();
				}
					FireCompleted(result);
			}
			catch (WebException ex)
			{
				HttpWebResponse response = ex.Response as HttpWebResponse;
				string result = null;
				if (response != null)
				{
					using (var stream = response.GetResponseStream())
					{
						using (var reader = new StreamReader(stream))
						{
							result = reader.ReadToEnd();
						}
					}
				}

					FireCompleted(new DoPostCompletedEventArgs { Error = ex, Response = result });
			}
		}

		private void FireCompleted(DoPostCompletedEventArgs e)
		{
			if (DoPostCompleted != null)
			{
				Deployment.Current.Dispatcher.BeginInvoke(() =>
					{
						DoPostCompleted(this, e);
					});
			}
		}

		private string EncodeParameters()
		{
			return string.Join("&",parameters.Select(kv => kv.Key.UriEncode() + "=" + kv.Value.UriEncode()).ToArray());
			//StringBuilder builder = new StringBuilder();
			//foreach (var item in parameters)
			//{
			//    if (builder.Length > 0)
			//    {
			//        builder.Append("&");
			//    }
			//    builder.Append(Uri.EscapeDataString(item.Key));
			//    builder.Append("=");
			//    builder.Append(Uri.EscapeDataString(item.Value));
			//}

			//return builder.ToString();
		}

		private void RemoveParameter(string paramname)
		{
			parameters.Remove(paramname);
		}

		/*				ConsumerKey = "NDveqLnJ9lf6PsCf5MgDQ",
				ConsumerSecret = "xro4UzRypElk3rMzQxYzzR0QiPt70pqad0UZwDyxc",
*/
	}

	public class DoPostCompletedEventArgs : EventArgs
	{
		public string Response { get; set; }
		public Exception Error { get; set; }
	}

}
