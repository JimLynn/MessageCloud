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
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using MessageCloud.ServiceReference1;
using System.Xml.Linq;
using System.Collections;
using System.Net.Browser;
using System.Windows.Controls.Primitives;
using System.Windows.Browser;

namespace MessageCloud
{
	public static class TreeHelper
	{
		public static DependencyObject VisualChild(this DependencyObject parent)
		{
			if (VisualTreeHelper.GetChildrenCount(parent) > 0)
			{
				return VisualTreeHelper.GetChild(parent, 0);
			}
			return null;
		}

		public static DependencyObject VisualChild(this DependencyObject parent, int index)
		{
			if (VisualTreeHelper.GetChildrenCount(parent) > index)
			{
				return VisualTreeHelper.GetChild(parent, index);
			}
			return null;
		}

		public static int CountVisualChildren(this DependencyObject parent)
		{
			return VisualTreeHelper.GetChildrenCount(parent);
		}
	}
}
