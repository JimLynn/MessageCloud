using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace MessageCloud
{
	public class TwitterUser
	{
		public TwitterUser()
		{

		}
		public TwitterUser(XElement xElement)
		{
			UserID = Convert.ToInt32(xElement.Element("id").Value);
			Name = xElement.Element("name").Value;
			ScreenName = xElement.Element("screen_name").Value;
			Location = xElement.Element("location").Value;
			Description = xElement.Element("description").Value;
			ProfileImageUrl = new Uri(xElement.Element("profile_image_url").Value, UriKind.Absolute);
			FollowersCount = Convert.ToInt32(xElement.Element("followers_count").Value);
		}
		public int UserID { get; set; }
		public string Name { get; set; }
		public string ScreenName { get; set; }
		public string Location { get; set; }
		public string Description { get; set; }
		public Uri ProfileImageUrl { get; set; }
		public int FollowersCount { get; set; }
		public string UriToUser
		{
			get
			{
				return "http://twitter.com/" + this.ScreenName;
			}
		}

	}
}
