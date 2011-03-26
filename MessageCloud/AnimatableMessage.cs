using System;
using System.Net;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;

namespace MessageCloud
{
	public class AnimatableMessage : DependencyObject
	{

		public AnimatableMessage()
		{

		}



		private List<Timeline> _Timelines;

		private DoubleAnimation animateX;
		private DoubleAnimation animateY;
		private DoubleAnimation animateZ;
        public IEnumerable<Timeline> GetAnimations()
		{
			if (_Timelines == null)
			{
				_Timelines = new List<Timeline>();
				animateX = AddTimeline(_Timelines, "X");
				animateY = AddTimeline(_Timelines, "Y");
				animateZ = AddTimeline(_Timelines, "Z");
			}
			return _Timelines;
		}

		private DoubleAnimation AddTimeline(List<Timeline> timelines, string property)
		{
			var animate = new DoubleAnimation();
			Storyboard.SetTarget(animate, this);
			Storyboard.SetTargetProperty(animate, new PropertyPath(property));
			animate.Duration = new Duration(TimeSpan.FromSeconds(0.6));
			animate.EasingFunction = new QuinticEase{ EasingMode = EasingMode.EaseOut};
			timelines.Add(animate);
			return animate;
		}

		public Tuple<double,double,double> AnimateTo(double x, double y, double z)
		{
			var result = new Tuple<double, double, double>(x - (animateX.To ?? 0), y - (animateY.To ?? 0), z - (animateZ.To ?? 0));
			animateX.To = x;
			animateY.To = y;
			animateZ.To = z;
			return result;
		}

		public void AnimateBy(double diffX, double diffY, double diffZ)
		{
			animateX.To = (animateX.To ?? 0) + diffX;
			animateY.To = (animateY.To ?? 0) + diffY;
			animateZ.To = (animateZ.To ?? 0) + diffZ;
		}

		public Tuple<double, double, double> CurrentAnimationValues()
		{
			return new Tuple<double, double, double>(animateX.To ?? 0, animateY.To ?? 0, animateZ.To ?? 0);
		}

		public double CurrentDepth
		{
			get
			{
				return animateZ.To ?? 0;
			}
		}

		#region X

		/// <summary> 
		/// Gets or sets the X possible Value of the double object.
		/// </summary> 
		public double X
		{
			get { return (double)GetValue(XProperty); }
			set { SetValue(XProperty, value); }
		}

		/// <summary> 
		/// Identifies the X dependency property.
		/// </summary> 
		public static readonly DependencyProperty XProperty =
					DependencyProperty.Register(
						  "X",
						  typeof(double),
						  typeof(AnimatableMessage),
						  new PropertyMetadata(OnXPropertyChanged));

		/// <summary>
		/// XProperty property changed handler. 
		/// </summary>
		/// <param name="d">AnimatableMessage that changed its X.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AnimatableMessage _AnimatableMessage = d as AnimatableMessage;
			if (_AnimatableMessage != null)
			{
				//TODO: Handle new value. 
			}
		}
		#endregion X


		#region Y

		/// <summary> 
		/// Gets or sets the Y possible Value of the double object.
		/// </summary> 
		public double Y
		{
			get { return (double)GetValue(YProperty); }
			set { SetValue(YProperty, value); }
		}

		/// <summary> 
		/// Identifies the Y dependency property.
		/// </summary> 
		public static readonly DependencyProperty YProperty =
					DependencyProperty.Register(
						  "Y",
						  typeof(double),
						  typeof(AnimatableMessage),
						  new PropertyMetadata(OnYPropertyChanged));

		/// <summary>
		/// YProperty property changed handler. 
		/// </summary>
		/// <param name="d">AnimatableMessage that changed its Y.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AnimatableMessage _AnimatableMessage = d as AnimatableMessage;
			if (_AnimatableMessage != null)
			{
				//TODO: Handle new value. 
			}
		}
		#endregion Y


		#region Z

		/// <summary> 
		/// Gets or sets the Z possible Value of the double object.
		/// </summary> 
		public double Z
		{
			get { return (double)GetValue(ZProperty); }
			set { SetValue(ZProperty, value); }
		}

		/// <summary> 
		/// Identifies the Z dependency property.
		/// </summary> 
		public static readonly DependencyProperty ZProperty =
					DependencyProperty.Register(
						  "Z",
						  typeof(double),
						  typeof(AnimatableMessage),
						  new PropertyMetadata(OnZPropertyChanged));

		/// <summary>
		/// ZProperty property changed handler. 
		/// </summary>
		/// <param name="d">AnimatableMessage that changed its Z.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnZPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AnimatableMessage _AnimatableMessage = d as AnimatableMessage;
			if (_AnimatableMessage != null)
			{
				double v = (double)e.NewValue;
				if (v > 0 && v < 200)
				{
					_AnimatableMessage.Opacity = (200.0 - v) / 200.0;
				}
				else if (v >= 200)
				{
					_AnimatableMessage.Opacity = 0;
				}
				else
				{
					_AnimatableMessage.Opacity = 1;
				}
			}
		}
		#endregion Z


		#region Text

		/// <summary> 
		/// Gets or sets the Text possible Value of the string object.
		/// </summary> 
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <summary> 
		/// Identifies the Text dependency property.
		/// </summary> 
		public static readonly DependencyProperty TextProperty =
					DependencyProperty.Register(
						  "Text",
						  typeof(string),
						  typeof(AnimatableMessage),
						  new PropertyMetadata(OnTextPropertyChanged));

		/// <summary>
		/// TextProperty property changed handler. 
		/// </summary>
		/// <param name="d">AnimatableMessage that changed its Text.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AnimatableMessage _AnimatableMessage = d as AnimatableMessage;
			if (_AnimatableMessage != null)
			{
				//TODO: Handle new value. 
			}
		}
		#endregion Text


		#region TwitterMessage

		/// <summary> 
		/// Gets or sets the TwitterMessage possible Value of the TwitterMessage object.
		/// </summary> 
		public TwitterMessage TwitterMessage
		{
			get 
			{
				TwitterMessage m = (TwitterMessage)GetValue(TwitterMessageProperty);
				return m; 
			}
			set { SetValue(TwitterMessageProperty, value); }
		}

		/// <summary> 
		/// Identifies the TwitterMessage dependency property.
		/// </summary> 
		public static readonly DependencyProperty TwitterMessageProperty =
					DependencyProperty.Register(
						  "TwitterMessage",
						  typeof(TwitterMessage),
						  typeof(AnimatableMessage),
						  new PropertyMetadata(OnTwitterMessagePropertyChanged));

		/// <summary>
		/// TwitterMessageProperty property changed handler. 
		/// </summary>
		/// <param name="d">AnimatableMessage that changed its TwitterMessage.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnTwitterMessagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AnimatableMessage _AnimatableMessage = d as AnimatableMessage;
			if (_AnimatableMessage != null)
			{
				//TODO: Handle new value. 
			}
		}
		#endregion TwitterMessage



		#region Opacity

		/// <summary> 
		/// Gets or sets the Opacity possible Value of the double object.
		/// </summary> 
		public double Opacity
		{
			get { return (double)GetValue(OpacityProperty); }
			set { SetValue(OpacityProperty, value); }
		}

		/// <summary> 
		/// Identifies the Opacity dependency property.
		/// </summary> 
		public static readonly DependencyProperty OpacityProperty =
					DependencyProperty.Register(
						  "Opacity",
						  typeof(double),
						  typeof(AnimatableMessage),
						  new PropertyMetadata(0.0,OnOpacityPropertyChanged));

		/// <summary>
		/// OpacityProperty property changed handler. 
		/// </summary>
		/// <param name="d">AnimatableMessage that changed its Opacity.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnOpacityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AnimatableMessage _AnimatableMessage = d as AnimatableMessage;
			if (_AnimatableMessage != null)
			{
				//TODO: Handle new value. 
			}
		}
		#endregion Opacity


		#region ZOrder

		/// <summary> 
		/// Gets or sets the ZOrder possible Value of the int object.
		/// </summary> 
		public int ZOrder
		{
			get { return (int)GetValue(ZOrderProperty); }
			set { SetValue(ZOrderProperty, value); }
		}

		/// <summary> 
		/// Identifies the ZOrder dependency property.
		/// </summary> 
		public static readonly DependencyProperty ZOrderProperty =
					DependencyProperty.Register(
						  "ZOrder",
						  typeof(int),
						  typeof(AnimatableMessage),
						  new PropertyMetadata(OnZOrderPropertyChanged));

		/// <summary>
		/// ZOrderProperty property changed handler. 
		/// </summary>
		/// <param name="d">AnimatableMessage that changed its ZOrder.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnZOrderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			AnimatableMessage _AnimatableMessage = d as AnimatableMessage;
			if (_AnimatableMessage != null)
			{
				//TODO: Handle new value. 
			}
		}
		#endregion ZOrder
		
	}
}
