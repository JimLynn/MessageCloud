using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MessageCloud.Web
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITwitter" in both code and config file together.
	[ServiceContract]
	public interface ITwitter
	{
		[OperationContract]
		string GetTimeline();

		[OperationContract]
		string GetTimelineSince(Int64 id);

		[OperationContract]
		string Favorite(Int64 statusID);

		[OperationContract]
		string GetStatus(Int64 statusID);
	}
}
