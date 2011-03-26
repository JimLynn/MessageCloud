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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Collections;

namespace MessageCloud
{
	public class Panel3D : Panel
	{

		public Panel3D()
		{
			MouseLeftButtonDown += new MouseButtonEventHandler(Panel3D_MouseLeftButtonDown);
			MouseMove += new MouseEventHandler(Panel3D_MouseMove);
			MouseLeftButtonUp += new MouseButtonEventHandler(Panel3D_MouseLeftButtonUp);
		}

		Point dragPosition;
		bool isMouseDown = false;
		bool isMouseDragging = false;

		void Panel3D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			isMouseDragging = false;
			isMouseDown = false;
			ReleaseMouseCapture();
		}

		void Panel3D_MouseMove(object sender, MouseEventArgs e)
		{
			if (isMouseDown)
			{
				isMouseDragging = true;
			}
			if (isMouseDragging)
			{
				Point cur = e.GetPosition(this);
				Point diff = new Point(dragPosition.X - cur.X, dragPosition.Y - cur.Y);
				ViewX -= diff.X;
				ViewY -= diff.Y;
			}
		}

		void Panel3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			isMouseDown = true;
			isMouseDragging = false;
			dragPosition = e.GetPosition(this);
			CaptureMouse();
		}
		private List<UIElement> elements = new List<UIElement>();

		protected override Size MeasureOverride(Size availableSize)
		{
			
			var newelements = (from element in Children
							   join existing in elements on element equals existing into joined
							   from item in joined.DefaultIfEmpty()
							   where item == null
							   select element).ToList();

			var oldelements = (from element in elements
							   join current in Children on element equals current into joined
							   from item in joined.DefaultIfEmpty()
							   where item == null
							   select element).ToList();

			newelements.ForEach((element) =>
				{
					PlaneProjection projection = element.Projection as PlaneProjection;
					if (projection == null)
					{
						projection = new PlaneProjection();
						element.Projection = projection;
					}
				});
			elements.AddRange(newelements);
			oldelements.ForEach((element) => { elements.Remove(element); });
			foreach (var item in Children)
			{
				item.Measure(availableSize);
			}
			if (double.IsPositiveInfinity(availableSize.Width) || double.IsPositiveInfinity(availableSize.Height))
			{
				return new Size(1000000, 1000000);
			}
			else
			{
				return availableSize;
			}
		}
		
		protected override Size ArrangeOverride(Size finalSize)
		{
			foreach (var item in Children)
			{
				item.Arrange(new Rect((finalSize.Width / 2) - (item.DesiredSize.Width / 2), (finalSize.Height / 2) - (item.DesiredSize.Height / 2), item.DesiredSize.Width, item.DesiredSize.Height));
				AdjustProjection(item);
			}
			return finalSize;
		}

		private void AdjustProjection(UIElement item)
		{
			double x = GetPropertyOnItemOrChild(item, XProperty);
			double y = GetPropertyOnItemOrChild(item, YProperty);
			double z = GetPropertyOnItemOrChild(item,ZProperty);

			//Debug.WriteLine(string.Format("AdjustProjection {0}", z));
			PlaneProjection projection = item.Projection as PlaneProjection;
			projection.GlobalOffsetX = x - ViewX;
			projection.GlobalOffsetY = y - ViewY;
			projection.GlobalOffsetZ = z - ViewZ;
		}

		private double GetPropertyOnItemOrChild(UIElement item, DependencyProperty property)
		{
			double value = (double)item.GetValue(property);
			if (value == 0.0 && (item is ContentPresenter || item is ContentControl))
			{
				if (VisualTreeHelper.GetChildrenCount(item) > 0)
				{
					var child = VisualTreeHelper.GetChild(item, 0);
					value = (double)child.GetValue(property);
				}
			}
			return value;
		}


		#region ViewX

		/// <summary> 
		/// Gets or sets the ViewX possible Value of the double object.
		/// </summary> 
		public double ViewX
		{
			get { return (double)GetValue(ViewXProperty); }
			set { SetValue(ViewXProperty, value); }
		}

		/// <summary> 
		/// Identifies the ViewX dependency property.
		/// </summary> 
		public static readonly DependencyProperty ViewXProperty =
					DependencyProperty.Register(
						  "ViewX",
						  typeof(double),
						  typeof(Panel3D),
						  new PropertyMetadata(0.0,OnViewXPropertyChanged));

		/// <summary>
		/// ViewXProperty property changed handler. 
		/// </summary>
		/// <param name="d">Panel3D that changed its ViewX.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnViewXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Panel3D _Panel3D = d as Panel3D;
			if (e.OldValue != e.NewValue && _Panel3D != null)
			{
				_Panel3D.AdjustX();
			}
		}

		private void AdjustX()
		{
			double newvalue = ViewX;
			foreach (var element in elements)
			{
				double value = GetPropertyOnItemOrChild(element, XProperty);
				((PlaneProjection)element.Projection).GlobalOffsetX = value - newvalue;
			}
		}
		#endregion ViewX

		#region ViewY

		/// <summary> 
		/// Gets or sets the ViewY possible Value of the double object.
		/// </summary> 
		public double ViewY
		{
			get { return (double)GetValue(ViewYProperty); }
			set { SetValue(ViewYProperty, value); }
		}

		/// <summary> 
		/// Identifies the ViewY dependency property.
		/// </summary> 
		public static readonly DependencyProperty ViewYProperty =
					DependencyProperty.Register(
						  "ViewY",
						  typeof(double),
						  typeof(Panel3D),
						  new PropertyMetadata(0.0,OnViewYPropertyChanged));

		/// <summary>
		/// ViewYProperty property changed handler. 
		/// </summary>
		/// <param name="d">Panel3D that changed its ViewY.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnViewYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Panel3D _Panel3D = d as Panel3D;
			if (e.OldValue != e.NewValue && _Panel3D != null)
			{
				_Panel3D.AdjustY(); 
			}
		}

		private void AdjustY()
		{
			double newvalue = ViewY;
			foreach (var element in elements)
			{
				double value = GetPropertyOnItemOrChild(element,YProperty);
				((PlaneProjection)element.Projection).GlobalOffsetY = value - newvalue;
			}
		}
		#endregion ViewY

		#region ViewZ

		/// <summary> 
		/// Gets or sets the ViewZ possible Value of the double object.
		/// </summary> 
		public double ViewZ
		{
			get { return (double)GetValue(ViewZProperty); }
			set { SetValue(ViewZProperty, value); }
		}

		/// <summary> 
		/// Identifies the ViewZ dependency property.
		/// </summary> 
		public static readonly DependencyProperty ViewZProperty =
					DependencyProperty.Register(
						  "ViewZ",
						  typeof(double),
						  typeof(Panel3D),
						  new PropertyMetadata(0.0,OnViewZPropertyChanged));

		/// <summary>
		/// ViewZProperty property changed handler. 
		/// </summary>
		/// <param name="d">Panel3D that changed its ViewZ.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnViewZPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Panel3D _Panel3D = d as Panel3D;
			if (e.OldValue != e.NewValue && _Panel3D != null)
			{
				_Panel3D.AdjustZ();
			}
		}

		private void AdjustZ()
		{
			double newvalue = ViewZ;
			foreach (var element in elements)
			{
				double value = GetPropertyOnItemOrChild(element,ZProperty);
				((PlaneProjection)element.Projection).GlobalOffsetZ = value - newvalue;
			}
		}
		#endregion ViewZ


		#region X

		/// <summary> 
		/// Gets or sets the X possible Value of the double object.
		/// </summary> 
		public double X
		{
			get { return (double)GetValue(XProperty); }
			set { SetValue(XProperty, value); }
		}

		public static double GetX(DependencyObject obj)
		{
			return (double)obj.GetValue(XProperty);
		}

		public static void SetX(DependencyObject obj, double value)
		{
			obj.SetValue(XProperty, value);
		}

		/// <summary> 
		/// Identifies the X dependency property.
		/// </summary> 
		public static readonly DependencyProperty XProperty =
					DependencyProperty.RegisterAttached(
						  "X",
						  typeof(double),
						  typeof(UIElement),
						  new PropertyMetadata(OnXPropertyChanged));

		/// <summary>
		/// XProperty property changed handler. 
		/// </summary>
		/// <param name="d">Panel3D that changed its X.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UIElement el = d as UIElement;
			if (el != null)
			{
				Panel3D parent = VisualTreeHelper.GetParent(el) as Panel3D;
				if (parent != null)
				{
					parent.AdjustX();
				}
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

		public static double GetY(DependencyObject obj)
		{
			return (double)obj.GetValue(YProperty);
		}

		public static void SetY(DependencyObject obj, double value)
		{
			obj.SetValue(YProperty, value);
		}

		/// <summary> 
		/// Identifies the Y dependency property.
		/// </summary> 
		public static readonly DependencyProperty YProperty =
					DependencyProperty.Register(
						  "Y",
						  typeof(double),
						  typeof(UIElement),
						  new PropertyMetadata(OnYPropertyChanged));

		/// <summary>
		/// YProperty property changed handler. 
		/// </summary>
		/// <param name="d">Panel3D that changed its Y.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UIElement el = d as UIElement;
			if (el != null)
			{
				Panel3D parent = VisualTreeHelper.GetParent(el) as Panel3D;
				if (parent != null)
				{
					parent.AdjustY();
				}
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

		public static double GetZ(DependencyObject obj)
		{
			return (double)obj.GetValue(ZProperty);
		}

		public static void SetZ(DependencyObject obj, double value)
		{
			obj.SetValue(ZProperty, value);
		}

		/// <summary> 
		/// Identifies the Z dependency property.
		/// </summary> 
		public static readonly DependencyProperty ZProperty =
					DependencyProperty.Register(
						  "Z",
						  typeof(double),
						  typeof(UIElement),
						  new PropertyMetadata(OnZPropertyChanged));

		/// <summary>
		/// ZProperty property changed handler. 
		/// </summary>
		/// <param name="d">Panel3D that changed its Z.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnZPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UIElement el = d as UIElement;
			if (el != null)
			{
				Panel3D parent = VisualTreeHelper.GetParent(el) as Panel3D;
				if (parent != null)
				{
					parent.AdjustZ();
				}
			}
		}
		#endregion Z

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
		}
		
	}

	public class New3DPanel : VirtualizingPanel
	{
		private IItemContainerGenerator _generator;

		public New3DPanel()
		{
			Background = new SolidColorBrush(Colors.Transparent);
			Loaded += new RoutedEventHandler(New3DPanel_Loaded);
			animateTo = new Storyboard();
			animateTo.Duration = new Duration(TimeSpan.FromSeconds(0.7));
			AnimateX = AnimateProperty(animateTo, ViewXProperty, null, TimeSpan.FromSeconds(0.7));
			AnimateY = AnimateProperty(animateTo, ViewYProperty, null, TimeSpan.FromSeconds(0.7));
			AnimateZ = AnimateProperty(animateTo, ViewZProperty, TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(0.5));
			animateTo.Completed += new EventHandler(animateTo_Completed);
			MouseLeftButtonDown += new MouseButtonEventHandler(Panel3D_MouseLeftButtonDown);
			MouseMove += new MouseEventHandler(Panel3D_MouseMove);
			MouseLeftButtonUp += new MouseButtonEventHandler(Panel3D_MouseLeftButtonUp);
			MouseWheel += new MouseWheelEventHandler(New3DPanel_MouseWheel);
		}

		void New3DPanel_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				AnimateX.To = ViewX;
				AnimateY.To = ViewY;
				AnimateZ.To = AnimateZ.To + 500;
				animateTo.Begin();
			}
			else if (e.Delta > 0)
			{
				AnimateX.To = ViewX;
				AnimateY.To = ViewY;
				AnimateZ.To = AnimateZ.To - 500;
				animateTo.Begin();
			}
		}

		Point dragPosition;
		bool isMouseDown = false;
		bool isMouseDragging = false;

		void Panel3D_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			isMouseDragging = false;
			isMouseDown = false;
			ReleaseMouseCapture();
		}

		void Panel3D_MouseMove(object sender, MouseEventArgs e)
		{
			if (isMouseDown)
			{
				isMouseDragging = true;
			}
			if (isMouseDragging)
			{
				Point cur = e.GetPosition(this);
				Point diff = new Point(dragPosition.X - cur.X, dragPosition.Y - cur.Y);
				ViewX += diff.X;
				ViewY += diff.Y;
				dragPosition = cur;
			}
		}

		void Panel3D_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (ReferenceEquals(this, e.OriginalSource))
			{
				isMouseDown = true;
				isMouseDragging = false;
				dragPosition = e.GetPosition(this);
				CaptureMouse();
			}
		}

		void animateTo_Completed(object sender, EventArgs e)
		{
		}

		private DoubleAnimation AnimateProperty(Storyboard animateTo, DependencyProperty property, TimeSpan? begin, TimeSpan duration)
		{
			DoubleAnimation animation = new DoubleAnimation();

			animation.Duration = new Duration(duration);
			animation.BeginTime = begin;
			Storyboard.SetTarget(animation, this);
			Storyboard.SetTargetProperty(animation, new PropertyPath(property));
			animation.EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut };
			animateTo.Children.Add(animation);
			return animation;
		}

		private Storyboard animateTo;
		private DoubleAnimation AnimateX;
		private DoubleAnimation AnimateY;
		private DoubleAnimation AnimateZ;

		private int CurrentIndex = -1;

		void New3DPanel_Loaded(object sender, RoutedEventArgs e)
		{
			Host = ItemsControl.GetItemsOwner(this) as TwitterPanel;
			Host.SelectionChanged += new SelectionChangedEventHandler(Host_SelectionChanged);
		}

		void Host_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			InvalidateMeasure();
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			// Make sure _realizedChildren exists and contains all children
			EnsureRealizedChildren();

			// Get the ItemContainerGenerator that generates UI items for this panel
			_generator = ItemContainerGenerator;

			// Get the panel
			TwitterPanel host = ItemsControl.GetItemsOwner(this) as TwitterPanel;

			// Keep track of all data items, assigning X,Y,Z coords to them
			UpdateItemsTracker(host);

			// Work out how many items we want to show. We show 21 - the previous item
			// from the selected item (so it doesn't suddenly disappear when we 
			// scroll past it) and 20 more. More than that and we can't really see them
			_startPosition = 0;
			if (host.SelectedIndex >= 0)
			{
				_startPosition = host.SelectedIndex - 1;
				if (_startPosition < 0)
				{
					_startPosition = 0;
				}
			}
			_endPosition = Math.Min(host.Items.Count - 1, _startPosition + 20);
			if (_endPosition < 0)
			{
				_endPosition = 0;
			}

			// Recycle any existing containers we don't need any more so the 
			// ItemsContainerGenerator can reuse them instead of creating more 
			RecycleContainers();
			if (_startPosition >= _endPosition && _endPosition > 0) return _previousMeasureSize;
			GeneratorPosition start = _generator.GeneratorPositionFromIndex(_startPosition);
			int childIndex = (start.Offset == 0) ? start.Index : start.Index + 1;

			using (_generator.StartAt(start, GeneratorDirection.Forward, true))
			{
				for (int i = _startPosition; i <= _endPosition; ++i)
				{
					bool isNewlyRealized;

					UIElement child = _generator.GenerateNext(out isNewlyRealized) as UIElement;
					if (child == null) continue;

					if (isNewlyRealized)
					{
						InsertContainer(childIndex, child, false);
					}
					else
					{
						if (childIndex >= _realizedChildren.Count || !(_realizedChildren[childIndex] == child))
						{
							// we have a recycled container (if it was realized container it would have been returned in the
							// propert location). Note also that recycled containers are NOT in the _realizedChildren list.
							InsertContainer(childIndex, child, true);
						}
						else
						{
							// previously realized child, so do nothing
						}
					}
					childIndex++;

					if (isNewlyRealized)
					{
						PlaneProjection proj = (PlaneProjection)child.Projection;
						ItemTracker tracked = trackedItems[i];
						proj.GlobalOffsetX = tracked.X;
						proj.GlobalOffsetY = tracked.Y;
						proj.GlobalOffsetZ = tracked.Z;
					}

					#region Measure Logic
					child.Measure(availableSize);

					#endregion Measure Logic
				}
			}

			DisconnectRecycledContainers();
			var oldrealized = realizedTrackers;
			if (oldrealized == null)
			{
				oldrealized = new List<ItemTracker>();
			}
			realizedTrackers = (from item in _realizedChildren.Cast<FrameworkElement>()
								join track in trackedItems on item.DataContext equals track.Item
								select track).ToList();

			var discarded = (from item in oldrealized 
							join j in realizedTrackers on item equals j into joined
							from j in joined.DefaultIfEmpty()
							where j == null
							select item).ToList();
			var added = (from item in realizedTrackers
						 join j in oldrealized on item equals j into joined
						 from j in joined.DefaultIfEmpty()
						 where j == null
						 select item).ToList();

			var real = from child in _realizedChildren.Cast<FrameworkElement>()
					   join j in realizedTrackers on child.DataContext equals j.Item
					   select new { Track = j, Element = child };
			foreach (var add in real)
			{
				PlaneProjection proj = add.Element.Projection as PlaneProjection;
				proj.GlobalOffsetX = add.Track.X - ViewX;
				proj.GlobalOffsetY = add.Track.Y - ViewY;
				proj.GlobalOffsetZ = add.Track.Z - ViewZ;
			}

			if (double.IsPositiveInfinity(availableSize.Width) || double.IsPositiveInfinity(availableSize.Height))
			{
				_previousMeasureSize = new Size(1000000, 1000000);
			}
			else
			{
				_previousMeasureSize = availableSize;
			}
			return _previousMeasureSize;

		}

		private List<ItemTracker> realizedTrackers;

		protected override Size ArrangeOverride(Size finalSize)
		{
			foreach (var item in _realizedChildren)
			{
				item.Arrange(new Rect((finalSize.Width / 2) - (item.DesiredSize.Width / 2), (finalSize.Height / 2) - (item.DesiredSize.Height / 2), item.DesiredSize.Width, item.DesiredSize.Height));
			}

			TwitterPanel host = ItemsControl.GetItemsOwner(this) as TwitterPanel;
			
			//if (CurrentIndex != host.SelectedIndex || (CurrentIndex >=0 && trackedItems[CurrentIndex].Item.Equals(host.SelectedItem) == false))
			//{
				CurrentIndex = host.SelectedIndex;
				if (CurrentIndex >= 0)
				{
					var tracked = trackedItems[CurrentIndex];
					var tm = tracked.Item;// as AnimatableMessage;
					//var x = from item in _realizedChildren
					//        select new
					//        {
					//            Element = item,
					//            Projection = item.Projection as PlaneProjection,
					//            Message = ((FrameworkElement)item).DataContext // as AnimatableMessage
					//        };
					//var realized = x.First(t => t.Message.Equals(tm));

					//if (tm != null)
					//{
					//    //tm.TwitterMessage.Text;
					//}
					AnimateX.To = tracked.X;
					AnimateY.To = tracked.Y;
					AnimateZ.To = tracked.Z;
					animateTo.Begin();
				}
			//}

			return finalSize;
		}

		private void InsertContainer(int childIndex, UIElement container, bool isRecycled)
		{
			// index in Children collection, whereas childIndex is the index into the _realizedChildren collection
			int visualTreeIndex = 0;
			UIElementCollection children = Children;

			if (childIndex > 0)
			{
				// find the item before where we want to insert the new item
				visualTreeIndex = ChildIndexFromRealizedIndex(childIndex - 1);
				visualTreeIndex++;
			}

			if (isRecycled && visualTreeIndex < children.Count && children[visualTreeIndex] == container)
			{
				// don't insert if a recycled container is in the proper place already
			}
			else
			{
				if (visualTreeIndex < children.Count)
				{
					int insertIndex = visualTreeIndex;
					if (isRecycled && VisualTreeHelper.GetParent(container) != null)
					{
						// If the container is recycled we have to remove it from its place in the visual tree and 
						// insert it in the proper location.   We cant use an internal Move api, so we are removing
						// and inserting the container
						int containerIndex = children.IndexOf(container);
						RemoveInternalChildRange(containerIndex, 1);
						if (containerIndex < insertIndex)
						{
							insertIndex--;
						}

						InsertInternalChild(insertIndex, container);
					}
					else
					{
						InsertInternalChild(insertIndex, container);
					}
				}
				else
				{
					if (isRecycled && VisualTreeHelper.GetParent(container) != null)
					{
						// Recycled container is still in the tree; move it to the end
						int originalIndex = children.IndexOf(container);
						RemoveInternalChildRange(originalIndex, 1);
						AddInternalChild(container);
					}
					else
					{
						AddInternalChild(container);
					}
				}
			}

			// Keep realizedChildren in sync w/ the visual tree.
			_realizedChildren.Insert(childIndex, container);
			_generator.PrepareItemContainer(container);
		}

		/// <summary>
		///     Takes an index from the realized list and returns the corresponding index in the Children collection
		/// </summary>
		private int ChildIndexFromRealizedIndex(int realizedChildIndex)
		{
			UIElementCollection children = Children;
			// If we're not recycling containers then we're not using a realizedChild index and no translation is necessary
			if (realizedChildIndex < _realizedChildren.Count)
			{
				UIElement child = _realizedChildren[realizedChildIndex];

				for (int i = realizedChildIndex; i < children.Count; i++)
				{
					if (children[i] == child)
					{
						return i;
					}
				}
			}

			return realizedChildIndex;
		}

		/// <summary>
		///     Recycled containers still in the InternalChildren collection at the end of Measure should be disconnected
		///     from the visual tree.  Otherwise they're still visible to things like Arrange, keyboard navigation, etc.
		/// </summary>
		private void DisconnectRecycledContainers()
		{
			int realizedIndex = 0;
			UIElement visualChild;
			UIElement realizedChild = _realizedChildren.Count > 0 ? _realizedChildren[0] : null;
			UIElementCollection children = Children;

			int removeStartRange = -1;
			int removalCount = 0;
			for (int i = 0; i < children.Count; i++)
			{
				visualChild = children[i];

				if (visualChild == realizedChild)
				{
					if (removalCount > 0)
					{
						RemoveInternalChildRange(removeStartRange, removalCount);
						i -= removalCount;
						removalCount = 0;
						removeStartRange = -1;
					}

					realizedIndex++;

					if (realizedIndex < _realizedChildren.Count)
					{
						realizedChild = _realizedChildren[realizedIndex];
					}
					else
					{
						realizedChild = null;
					}
				}
				else
				{
					if (removeStartRange == -1)
					{
						removeStartRange = i;
					}

					removalCount++;
				}
			}

			if (removalCount > 0)
			{
				RemoveInternalChildRange(removeStartRange, removalCount);
			}
		}


		private Size _previousMeasureSize = new Size(0, 0);
		private int _startPosition = 0;
		private int _endPosition = 0;

		/// <summary>
		/// Go through existing containers 
		/// </summary>
		private void RecycleContainers()
		{
			if (Children.Count == 0) return;

			ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
			int recycleRangeStart = -1;
			int recycleRangeCount = 0;
			int childCount = _realizedChildren.Count;
			for (int i = 0; i < childCount; i++)
			{
				bool recycleContainer = false;

				int itemIndex = itemsControl.Items.IndexOf((_realizedChildren[i] as ContentPresenter).Content);

				if (itemIndex >= 0 && (itemIndex < _startPosition || itemIndex > _endPosition))
				{
					recycleContainer = true;
				}

				if (!Children.Contains(_realizedChildren[i]))
				{
					recycleContainer = false;
					_realizedChildren.RemoveRange(i, 1);
					i--;
					childCount--;
				}

				if (recycleContainer)
				{
					if (recycleRangeStart == -1)
					{
						recycleRangeStart = i;
						recycleRangeCount = 1;
					}
					else
					{
						recycleRangeCount++;
					}
				}
				else
				{
					if (recycleRangeCount > 0)
					{
						GeneratorPosition position = new GeneratorPosition(recycleRangeStart, 0);
						((IRecyclingItemContainerGenerator)_generator).Recycle(position, recycleRangeCount);
						_realizedChildren.RemoveRange(recycleRangeStart, recycleRangeCount);

						childCount -= recycleRangeCount;
						i -= recycleRangeCount;
						recycleRangeCount = 0;
						recycleRangeStart = -1;
					}
				}
			}

			if (recycleRangeCount > 0)
			{
				GeneratorPosition position = new GeneratorPosition(recycleRangeStart, 0);
				((IRecyclingItemContainerGenerator)_generator).Recycle(position, recycleRangeCount);
				_realizedChildren.RemoveRange(recycleRangeStart, recycleRangeCount);
			}
		}
		private List<UIElement> _realizedChildren;

		private void EnsureRealizedChildren()
		{
			if (_realizedChildren == null)
			{
				_realizedChildren = new List<UIElement>(Children.Count);

				for (int i = 0; i < Children.Count; i++)
				{
					_realizedChildren.Add(Children[i]);
				}
			}
		}



		private List<ItemTracker> trackedItems = new List<ItemTracker>();

		private Random rnd = new Random();

		/// <summary>
		/// track every item in the host's list (these are data items)
		/// Generate an appropriate depth for them
		/// </summary>
		/// <param name="host"></param>
		private void UpdateItemsTracker(TwitterPanel host)
		{
			trackedItems = (from trackers in trackedItems
							   where host.Items.Contains(trackers.Item)
							   select trackers).ToList();

			var newtrackers = from item in host.Items
							  where trackedItems.Any(t => t.Item.Equals(item)) == false
							  select MakeTracker(item, null);
			trackedItems.AddRange(newtrackers);

			// order trackedItems to be the right order according to the host

			trackedItems = (from item in host.Items
							select trackedItems.First(t => t.Item.Equals(item))).ToList();

			// Adjust the depths for all the trackers, to account for new items
			int depth = -300;
			foreach (var track in trackedItems)
			{
				track.Z = depth;
				depth -= 1000;
			}

			// join every item in the hosts data list with all our known tracked items
			// this is a left join so we preserve existing tracker items
			
			
			
			//var newlist = (from child in host.Items
			//              join known in trackedItems on child equals known.Item into joined
			//              from item in joined.DefaultIfEmpty()
			//              select MakeTracker(child, item)).ToList();

			//int cnt = host.Items.Count;
			//var x = (from child in host.Items
			//         join known in trackedItems on child equals known.Item into joined
			//         from item in joined.DefaultIfEmpty()
			//         where item == null
			//         select child).ToList();

			//// Adjust the depths for all the trackers, to account for new items
			//int depth = -300;
			//foreach (var track in newlist)
			//{
			//    track.Z = depth;
			//    depth -= 1000;
			//}
			//trackedItems = newlist;
		}

		/// <summary>
		/// Track the object in an ItemTracker. If the existing tracker
		/// isn't null, use it, otherwise generate a new one and give it
		/// appropriate X,Y,Z coords 
		/// </summary>
		/// <param name="item">data item we're tracking</param>
		/// <param name="existing">either an existing tracker, or null</param>
		/// <returns>An ItemTracker tracking this data item</returns>
		private ItemTracker MakeTracker(object item, ItemTracker existing)
		{
			if (existing == null)
			{
				return new ItemTracker
				{
					Item = item,
					X = rnd.NextDouble() * 1200 - 600,
					Y = rnd.NextDouble() * 1200 - 600
				};
			}
			else
			{
				return existing;
			}
		}

		private TwitterPanel Host;


		#region ViewX

		/// <summary> 
		/// Gets or sets the ViewX possible Value of the double object.
		/// </summary> 
		public double ViewX
		{
			get { return (double)GetValue(ViewXProperty); }
			set { SetValue(ViewXProperty, value); }
		}

		/// <summary> 
		/// Identifies the ViewX dependency property.
		/// </summary> 
		public static readonly DependencyProperty ViewXProperty =
					DependencyProperty.Register(
						  "ViewX",
						  typeof(double),
						  typeof(New3DPanel),
						  new PropertyMetadata(OnViewXPropertyChanged));

		/// <summary>
		/// ViewXProperty property changed handler. 
		/// </summary>
		/// <param name="d">New3DPanel that changed its ViewX.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnViewXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			New3DPanel _New3DPanel = d as New3DPanel;
			if (_New3DPanel != null)
			{
				_New3DPanel.AdjustX(); 
			}
		}

		private void AdjustX()
		{
			double newvalue = ViewX;
			foreach (var element in _realizedChildren)
			{
				var fe = element as FrameworkElement;
				var tracked = realizedTrackers.FirstOrDefault(t => t.Item.Equals(fe.DataContext));
				if (tracked != null)
				{
					((PlaneProjection)element.Projection).GlobalOffsetX = tracked.X - newvalue;
				}
			}
		}

		#endregion ViewX


		#region ViewY

		/// <summary> 
		/// Gets or sets the ViewY possible Value of the double object.
		/// </summary> 
		public double ViewY
		{
			get { return (double)GetValue(ViewYProperty); }
			set { SetValue(ViewYProperty, value); }
		}

		/// <summary> 
		/// Identifies the ViewY dependency property.
		/// </summary> 
		public static readonly DependencyProperty ViewYProperty =
					DependencyProperty.Register(
						  "ViewY",
						  typeof(double),
						  typeof(New3DPanel),
						  new PropertyMetadata(OnViewYPropertyChanged));

		/// <summary>
		/// ViewYProperty property changed handler. 
		/// </summary>
		/// <param name="d">New3DPanel that changed its ViewY.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnViewYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			New3DPanel _New3DPanel = d as New3DPanel;
			if (_New3DPanel != null)
			{
				_New3DPanel.AdjustY();
			}
		}

		private void AdjustY()
		{
			double newvalue = ViewY;
			foreach (var element in _realizedChildren)
			{
				var fe = element as FrameworkElement;
				var tracked = realizedTrackers.FirstOrDefault(t => t.Item.Equals(fe.DataContext));
				if (tracked != null)
				{
					((PlaneProjection)element.Projection).GlobalOffsetY = tracked.Y - newvalue;
				}
			}
		}
		#endregion ViewY


		#region ViewZ

		/// <summary> 
		/// Gets or sets the ViewZ possible Value of the double object.
		/// </summary> 
		public double ViewZ
		{
			get { return (double)GetValue(ViewZProperty); }
			set { SetValue(ViewZProperty, value); }
		}

		/// <summary> 
		/// Identifies the ViewZ dependency property.
		/// </summary> 
		public static readonly DependencyProperty ViewZProperty =
					DependencyProperty.Register(
						  "ViewZ",
						  typeof(double),
						  typeof(New3DPanel),
						  new PropertyMetadata(OnViewZPropertyChanged));

		/// <summary>
		/// ViewZProperty property changed handler. 
		/// </summary>
		/// <param name="d">New3DPanel that changed its ViewZ.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnViewZPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			New3DPanel _New3DPanel = d as New3DPanel;
			if (_New3DPanel != null)
			{
				_New3DPanel.AdjustZ();

			}
		}

		private void AdjustZ()
		{
			double newvalue = ViewZ;
			foreach (var element in _realizedChildren)
			{
				var fe = element as FrameworkElement;
				var tracked = realizedTrackers.FirstOrDefault(t => t.Item.Equals(fe.DataContext));
				if (tracked != null)
				{
					((PlaneProjection)element.Projection).GlobalOffsetZ = tracked.Z - newvalue;
				}
			}
		}


		#endregion ViewZ

	
	}

	public class ItemTracker
	{
		public object Item { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
	}

	public class TwitterPanel : ItemsControl
	{
		public TwitterPanel()
		{
			
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);
		}

		//protected override void OnKeyUp(KeyEventArgs e)
		//{
		//    if (e.Handled)
		//        return;

		//    if (e.Key == Key.Up || e.Key == Key.Left)
		//    {
		//        if (SelectedItem == null )
		//        {
		//            SelectedIndex = Items.Count - 1;
		//        }
		//        else if (SelectedIndex > 0)
		//        {
		//            SelectedIndex--;
		//        }
		//        e.Handled = true;
		//    }
		//    else if (e.Key == Key.Down || e.Key == Key.Right)
		//    {
		//        if (SelectedItem == null)
		//        {
		//            SelectedIndex = 0;
		//        }
		//        else if (Items.Count > (SelectedIndex +1))
		//        {
		//            SelectedIndex++;
		//        }
		//        e.Handled = true;
		//    }
		//}

		public void BringIntoView(int index)
		{
			SelectedIndex = index;
			InvalidateMeasure();
			InvalidateArrange();
		}

		protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			base.ClearContainerForItemOverride(element, item);
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);
			UIElement ui = element as UIElement;
			if (ui != null)
			{
				if (ui.Projection == null)
				{
					ui.Projection = new PlaneProjection();
				}
			}
		}

		#region SelectedIndex

		/// <summary> 
		/// Gets or sets the SelectedIndex possible Value of the int object.
		/// </summary> 
		public int SelectedIndex
		{
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue(SelectedIndexProperty, value); }
		}

		/// <summary> 
		/// Identifies the SelectedIndex dependency property.
		/// </summary> 
		public static readonly DependencyProperty SelectedIndexProperty =
					DependencyProperty.Register(
						  "SelectedIndex",
						  typeof(int),
						  typeof(TwitterPanel),
						  new PropertyMetadata(-1,OnSelectedIndexPropertyChanged));

		/// <summary>
		/// SelectedIndexProperty property changed handler. 
		/// </summary>
		/// <param name="d">TwitterPanel that changed its SelectedIndex.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TwitterPanel _TwitterPanel = d as TwitterPanel;
			if (_TwitterPanel != null)
			{
				_TwitterPanel.OnSelectedIndexChanged((int)e.OldValue, (int)e.NewValue);
			}
		}

		private void OnSelectedIndexChanged(int oldValue, int newValue)
		{
			if (!isUpdatingSelection)
			{
				try
				{
					isUpdatingSelection = true;
					object oldItem = null;
					if (oldValue >= 0)
					{
						oldItem = Items[oldValue];
					}
					if (newValue < Items.Count && newValue >= 0)
					{
						SelectedItem = Items[newValue];
						var selectedContainer = ItemContainerGenerator.ContainerFromItem(SelectedItem);
						if (selectedContainer != null)
						{
							TryToSetFocus(selectedContainer);
						}
					}
					else
					{
						SelectedItem = null;
					}
					FireSelectionChanged(oldItem, SelectedItem);
				}
				finally
				{
					isUpdatingSelection = false;
				}
			}
		}

		private bool TryToSetFocus(DependencyObject element)
		{
			if (element is Control)
			{
				((Control)element).Focus();
				return true;
			}
			for (int i = 0; i < element.CountVisualChildren(); i++)
			{
				if (TryToSetFocus(element.VisualChild(i)))
				{
					return true;
				}
			}
			return false;
		}
		#endregion SelectedIndex

		private bool isUpdatingSelection = false;


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
						  typeof(TwitterPanel),
						  new PropertyMetadata(OnSelectedItemPropertyChanged));

		/// <summary>
		/// SelectedItemProperty property changed handler. 
		/// </summary>
		/// <param name="d">TwitterPanel that changed its SelectedItem.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TwitterPanel _TwitterPanel = d as TwitterPanel;
			if (_TwitterPanel != null)
			{
				_TwitterPanel.OnSelectedItemChanged(e.OldValue, e.NewValue); 
			}
		}

		private void OnSelectedItemChanged(object oldValue, object newValue)
		{
			if (!isUpdatingSelection)
			{
				try
				{
					isUpdatingSelection = true;
					SelectedIndex = Items.IndexOf(newValue);

					FireSelectionChanged(oldValue,newValue);
				}
				finally
				{
					isUpdatingSelection = false;
				}
			}
		}

		private void FireSelectionChanged(object itemRemoved, object itemAdded)
		{
			if (SelectionChanged != null)
			{
				List<object> removed = new List<object>();
				List<object> added = new List<object>();
				if (itemRemoved != null)
				{
					removed.Add(itemRemoved);
				}

				if (itemAdded != null)
				{
					added.Add(itemAdded);
				}
				SelectionChanged(this, new SelectionChangedEventArgs(removed, added));
			}
		}
		#endregion SelectedItem

		public event SelectionChangedEventHandler SelectionChanged;

	}

	public class Items3DControl : ItemsControl
	{
		public Items3DControl()
		{
			animateTo = new Storyboard();
			animateTo.Duration = new Duration(TimeSpan.FromSeconds(0.7));
			AnimateX = AnimateProperty(animateTo, ViewXProperty, null,TimeSpan.FromSeconds(0.7));
			AnimateY = AnimateProperty(animateTo, ViewYProperty, null,TimeSpan.FromSeconds(0.7));
			AnimateZ = AnimateProperty(animateTo, ViewZProperty, TimeSpan.FromSeconds(0.2),TimeSpan.FromSeconds(0.5));
			
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);
			UIElement ui = element as UIElement;
			if (ui != null)
			{
				if (ui.Projection == null)
				{
					ui.Projection = new PlaneProjection();
				}
			}
		}

		private DoubleAnimation AnimateProperty(Storyboard animateTo, DependencyProperty property, TimeSpan? begin, TimeSpan duration)
		{
			DoubleAnimation animation = new DoubleAnimation();
			
			animation.Duration = new Duration(duration);
			animation.BeginTime = begin;
			Storyboard.SetTarget(animation, this);
			Storyboard.SetTargetProperty(animation, new PropertyPath(property));
			animation.EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut };
			animateTo.Children.Add(animation);
			return animation;
		}

		private Storyboard animateTo;
		private DoubleAnimation AnimateX;
		private DoubleAnimation AnimateY;
		private DoubleAnimation AnimateZ;

		public void MoveToItem(int index)
		{
			DependencyObject containerFromIndex = ItemContainerGenerator.ContainerFromIndex(index);
			DependencyObject child = VisualTreeHelper.GetChild(containerFromIndex, 0);
			AnimateX.To = Panel3D.GetX(child);
			AnimateY.To = Panel3D.GetY(child);
			AnimateZ.To = Panel3D.GetZ(child);
			animateTo.Begin();
		}

		public void MoveToItem(object item)
		{
			DependencyObject container = ItemContainerGenerator.ContainerFromItem(item);
			DependencyObject child = VisualTreeHelper.GetChild(container, 0);
			AnimateX.To = Panel3D.GetX(child);
			AnimateY.To = Panel3D.GetY(child);
			AnimateZ.To = Panel3D.GetZ(child);
			animateTo.Begin();
		}
		
		private void AdjustPanel(double newValue, DependencyProperty property)
		{
			DependencyObject child = this as DependencyObject;
			while (child != null)
			{
				if (child is Panel3D)
				{
					break;
				}
				if (VisualTreeHelper.GetChildrenCount(child) > 0)
				{
					child = VisualTreeHelper.GetChild(child, 0);
				}
				else
				{
					child = null;
				}
			}
			if (child != null)
			{
				child.SetValue(property, newValue);
			}
		}


		#region ViewX

		/// <summary> 
		/// Gets or sets the ViewX possible Value of the double object.
		/// </summary> 
		public double ViewX
		{
			get { return (double)GetValue(ViewXProperty); }
			set { SetValue(ViewXProperty, value); }
		}

		/// <summary> 
		/// Identifies the ViewX dependency property.
		/// </summary> 
		public static readonly DependencyProperty ViewXProperty =
					DependencyProperty.Register(
						  "ViewX",
						  typeof(double),
						  typeof(Items3DControl),
						  new PropertyMetadata(OnViewXPropertyChanged));

		/// <summary>
		/// ViewXProperty property changed handler. 
		/// </summary>
		/// <param name="d">Items3DControl that changed its ViewX.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnViewXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Items3DControl _Items3DControl = d as Items3DControl;
			if (_Items3DControl != null)
			{
				_Items3DControl.AdjustPanel((double)e.NewValue, Panel3D.ViewXProperty);
			}
		}
		#endregion ViewX


		#region ViewY

		/// <summary> 
		/// Gets or sets the ViewY possible Value of the double object.
		/// </summary> 
		public double ViewY
		{
			get { return (double)GetValue(ViewYProperty); }
			set { SetValue(ViewYProperty, value); }
		}

		/// <summary> 
		/// Identifies the ViewY dependency property.
		/// </summary> 
		public static readonly DependencyProperty ViewYProperty =
					DependencyProperty.Register(
						  "ViewY",
						  typeof(double),
						  typeof(Items3DControl),
						  new PropertyMetadata(OnViewYPropertyChanged));

		/// <summary>
		/// ViewYProperty property changed handler. 
		/// </summary>
		/// <param name="d">Items3DControl that changed its ViewY.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnViewYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Items3DControl _Items3DControl = d as Items3DControl;
			if (_Items3DControl != null)
			{
				_Items3DControl.AdjustPanel((double)e.NewValue, Panel3D.ViewYProperty);
			}
		}
		#endregion ViewY


		#region ViewZ

		/// <summary> 
		/// Gets or sets the ViewZ possible Value of the double object.
		/// </summary> 
		public double ViewZ
		{
			get { return (double)GetValue(ViewZProperty); }
			set { SetValue(ViewZProperty, value); }
		}

		/// <summary> 
		/// Identifies the ViewZ dependency property.
		/// </summary> 
		public static readonly DependencyProperty ViewZProperty =
					DependencyProperty.Register(
						  "ViewZ",
						  typeof(double),
						  typeof(Items3DControl),
						  new PropertyMetadata(OnViewZPropertyChanged));

		/// <summary>
		/// ViewZProperty property changed handler. 
		/// </summary>
		/// <param name="d">Items3DControl that changed its ViewZ.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnViewZPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Items3DControl _Items3DControl = d as Items3DControl;
			if (_Items3DControl != null)
			{
				_Items3DControl.AdjustPanel((double)e.NewValue, Panel3D.ViewZProperty);
			}
		}
		#endregion ViewZ


	}
}
