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
using System.ComponentModel;

namespace MessageCloud
{
	public class Message : INotifyPropertyChanged
	{
		public Message()
		{

		}

		public Message(string message)
		{
			Text = message;
		}

		private string _Text;
		public string Text
		{
			get
			{
				return _Text;
			}
			set
			{
				_Text = value;
				FirePropertyChanged("Text");
			}
		}

		private TwitterMessage _TwitterMessage;
		public TwitterMessage TwitterMessage
		{
			get
			{
				return _TwitterMessage;
			}
			set
			{
				_TwitterMessage = value;
				FirePropertyChanged("TwitterMessage");
			}
		}

		private double _X;
		public double X
		{
			get
			{
				return _X;
			}
			set
			{
				if (double.IsNaN(value))
				{
					throw new ArgumentOutOfRangeException("X is NaN");
				}
				else if (double.IsInfinity(value))
				{
					throw new ArgumentOutOfRangeException("X is Infinity");
				}
				_X = value;
				FirePropertyChanged("X");
			}
		}
		private double _Y;
		public double Y
		{
			get
			{
				return _Y;
			}
			set
			{
				_Y = value;
				FirePropertyChanged("Y");
			}
		}

		private double _Z;
		public double Z
		{
			get
			{
				return _Z;
			}
			set
			{
				_Z = value;
				FirePropertyChanged("Depth");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void FirePropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}
	}
}
