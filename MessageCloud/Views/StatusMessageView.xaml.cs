using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MessageCloud
{
	/// <summary>
	/// Interaction logic for StatusMessageView.xaml
	/// </summary>
	public partial class StatusMessageView : UserControl
	{
		public StatusMessageView()
		{
			this.InitializeComponent();
			
			// Insert code required on object creation below this point.
		}

		public StatusMessageViewModel ViewModel
		{
			get
			{
				return (StatusMessageViewModel)DataContext;
			}
		}
	}
}