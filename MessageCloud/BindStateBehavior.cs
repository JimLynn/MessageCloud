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
using System.Windows.Interactivity;

namespace MessageCloud
{
	public class BindStateBehavior : Behavior<Control>
	{
		protected override void OnAttached()
		{
			base.OnAttached();

		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
		}


		#region State

		/// <summary> 
		/// Gets or sets the State possible Value of the string object.
		/// </summary> 
		public string State
		{
			get { return (string)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}

		/// <summary> 
		/// Identifies the State dependency property.
		/// </summary> 
		public static readonly DependencyProperty StateProperty =
					DependencyProperty.Register(
						  "State",
						  typeof(string),
						  typeof(BindStateBehavior),
						  new PropertyMetadata(OnStatePropertyChanged));

		/// <summary>
		/// StateProperty property changed handler. 
		/// </summary>
		/// <param name="d">BindStateBehavior that changed its State.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			BindStateBehavior _BindStateBehavior = d as BindStateBehavior;
			if (_BindStateBehavior != null && e.OldValue != e.NewValue)
			{
				_BindStateBehavior.OnStateChanged((string)e.NewValue);
			}
		}

		private void OnStateChanged(string state)
		{
			if (AssociatedObject != null && state != null)
			{
				VisualStateManager.GoToState(AssociatedObject, state, true);
			}
		}
		#endregion State
	}
}
