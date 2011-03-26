using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Net;

namespace MessageCloud.Web
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Twitter" in code, svc and config file together.
	public class Twitter : ITwitter
	{
		public string GetTimeline()
		{
			WebClient client = new WebClient();
			client.Credentials = new NetworkCredential("jimlynn", "xxxxxxxxxxxxxxx");
			string result = client.DownloadString("http://api.twitter.com/1/statuses/home_timeline.xml?count=200");
			return result;
		}

		public string Favorite(long statusID)
		{
			WebClient client = new WebClient();
			client.Credentials = new NetworkCredential("jimlynn", "xxxxxxxxxxxxxxxx");
			string result = client.UploadString(string.Format("http://api.twitter.com/1/favorites/create/{0}.xml", statusID),"" );
			return result;
		}

		public string GetStatus(Int64 statusID)
		{
			WebClient client = new WebClient();
			client.Credentials = new NetworkCredential("jimlynn", "xxxxxxxxxxxxxxxxxxxxxxxx");
			string result = client.DownloadString(string.Format("http://api.twitter.com/1/statuses/show/{0}.xml", statusID));
			return result;
		}


		public string GetTimelineSince(long id)
		{
			WebClient client = new WebClient();
			client.Credentials = new NetworkCredential("jimlynn", "xxxxxxxxxxxxxxxxxxxxxxxx");
			string result = client.DownloadString("http://api.twitter.com/1/statuses/home_timeline.xml?since_id=" + id.ToString() + "&count=200");
			return result;
		}
	}
}
