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
using System.Windows.Data;

namespace MessageCloud
{
	public class TextBoxDataChangedBehavior : Behavior<TextBox>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.TextChanged += TextChanged;
		}

		void TextChanged(object sender, TextChangedEventArgs e)
		{
			BindingExpression binding = AssociatedObject.GetBindingExpression(TextBox.TextProperty);
			binding.UpdateSource();
		}
		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.TextChanged -= TextChanged;
		}
	}
}
