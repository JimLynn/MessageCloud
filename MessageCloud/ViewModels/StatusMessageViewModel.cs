using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

namespace MessageCloud
{
	public class StatusMessageViewModel : INotifyPropertyChanged
	{
		public StatusMessageViewModel()
		{
			if (DesignerProperties.IsInDesignTool)
			{
				Message = "This is where a message will be";
			}
		}

		private string _Message;
		public string Message
		{
			get
			{
				return _Message;
			}
			set
			{
				_Message = value;
				NotifyPropertyChanged("Message");
				State = "Completed";
			}
		}

		public string LoadingMessage
		{
			get
			{
				return Message;
			}
			set
			{
				_Message = value;
				NotifyPropertyChanged("Message");
				State = "InProgress";
			}
		}

		public string LoadedMessage
		{
			get
			{
				return Message;
			}
			set
			{
				_Message = value;
				NotifyPropertyChanged("Message");
				State = "Completed";
			}
		}

		public string ErrorMessage
		{
			get
			{
				return Message;
			}
			set
			{
				_Message = value;
				NotifyPropertyChanged("Message");
				State = "Error";
			}
		}


		private string _State = "Closed";
		public string State
		{
			get
			{
				return _State;
			}
			set
			{
				_State = value;
				NotifyPropertyChanged("State");
			}
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion
	}
}