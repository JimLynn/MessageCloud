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
using System.Windows.Media.Imaging;

namespace MessageCloud
{
	public partial class ImagePopup : UserControl
	{
		public ImagePopup()
		{
			InitializeComponent();
			DataContext = this;
			image.ImageOpened += new EventHandler<RoutedEventArgs>(image_ImageOpened);
			image.ImageFailed += new EventHandler<ExceptionRoutedEventArgs>(image_ImageFailed);
		}

		void image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
		{
			message.Text = "Failed...";
		}

		void image_ImageOpened(object sender, RoutedEventArgs e)
		{
			loadingborder.Visibility = Visibility.Collapsed;
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			Visibility = Visibility.Collapsed;
		}


		#region Source

		/// <summary> 
		/// Gets or sets the Source possible Value of the Uri object.
		/// </summary> 
		public Uri Source
		{
			get { return (Uri)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		/// <summary> 
		/// Identifies the Source dependency property.
		/// </summary> 
		public static readonly DependencyProperty SourceProperty =
					DependencyProperty.Register(
						  "Source",
						  typeof(Uri),
						  typeof(ImagePopup),
						  new PropertyMetadata(OnSourcePropertyChanged));

		/// <summary>
		/// SourceProperty property changed handler. 
		/// </summary>
		/// <param name="d">ImagePopup that changed its Source.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ImagePopup _ImagePopup = d as ImagePopup;
			if (_ImagePopup != null)
			{
				if (e.NewValue != null)
				{
					_ImagePopup.SourceChanged((Uri)e.NewValue);
				}
			}
		}

		private void SourceChanged(Uri source)
		{
			Visibility = Visibility.Visible;
			loadingborder.Visibility = Visibility.Visible;
			message.Text = "Loading...";
		}

		#endregion Source

	}
}
