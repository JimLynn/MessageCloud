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
using System.Windows.Controls.Primitives;

namespace MessageCloud 
{
	public class NewPanel : ItemsControl
	{

		#region SelectedItem

		/// <summary> 
		/// Gets or sets the SelectedItem possible Value of the object object.
		/// </summary> 
		public object SelectedItem
		{
			get { return (object)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		/// <summary> 
		/// Identifies the SelectedItem dependency property.
		/// </summary>	
		public static readonly DependencyProperty SelectedItemProperty =
					DependencyProperty.Register(
						  "SelectedItem",
						  typeof(object),
						  typeof(NewPanel),
						  new PropertyMetadata(OnSelectedItemPropertyChanged));

		/// <summary>
		/// SelectedItemProperty property changed handler. 
		/// </summary>
		/// <param name="d">NewPanel that changed its SelectedItem.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			NewPanel _NewPanel = d as NewPanel;
			if (_NewPanel != null)
			{
				//TODO: Handle new value. 
			}
		}
		#endregion SelectedItem
	}

	public class APanel : VirtualizingPanel
	{
		public APanel()
		{

		}
	}
}
