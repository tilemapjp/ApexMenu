using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using System.Collections.Generic;

namespace ApexMenuSample.Touch
{
	public class ApexMenu : UIView
	{
		const float kApexMenuDefaultNearRadius = 110.0f;
		const float kApexMenuDefaultEndRadius = 120.0f;
		const float kApexMenuDefaultFarRadius = 140.0f;
		const float kApexMenuDefaultStartPointX = 160.0f;
		const float kApexMenuDefaultStartPointY = 240.0f;
		const float kApexMenuDefaultTimeOffset = 0.036f;
		const float kApexMenuDefaultRotateAngle = 0.0f;
		const float kApexMenuDefaultMenuWholeAngle = (float)(Math.PI * 2);
		const float kApexMenuDefaultExpandRotation = (float)Math.PI;
		const float kApexMenuDefaultCloseRotation = (float)(Math.PI * 2);
		const float kApexMenuDefaultAnimationDuration = 0.5f;
		const float kApexMenuStartMenuDefaultAnimationDuration = 0.3f;

		public UIImage Image { 
			get{
				return StartButton.Image;
			}
			set{
				StartButton.Image = value;
			}
		}
		public UIImage HighlightedImage { 
			get{
				return StartButton.HighlightedImage;
			}
			set{
				StartButton.HighlightedImage = value;
			}
		}
		public UIImage ContentImage { 
			get{
				return StartButton.ContentImageView.Image;
			}
			set{
				StartButton.ContentImageView.Image = value;
			}
		}
		public UIImage HighlightedContentImage { 
			get{
				return StartButton.ContentImageView.HighlightedImage;
			}
			set{
				StartButton.ContentImageView.HighlightedImage = value;
			}
		}

		public float  NearRadius { get; set; }
		public float  EndRadius { get; set; }
		public float  FarRadius { get; set; }
		public PointF StartPoint { 
			get{
				return StartPoint;
			} 
			set{
				StartPoint = value;
				StartButton.Center = value;
			} }
		public float  TimeOffset { get; set; }
		public float  RotateAngle { get; set; }
		public float  MenuWholeAngle { get; set; }
		public float  ExpandRotation { get; set; }
		public float  CloseRotation { get; set; }
		public float  AnimationDuration { get; set; }

		private List<ApexMenuItem> MenusArray;
		private int Flag;
		private NSTimer Timer;
		private ApexMenuItem StartButton;

		//id<AwesomeMenuDelegate> __weak _delegate;
		private bool IsAnimating;

		public bool Expanding {
			get {
				return Expanding;
			}

			set {
				if (Expanding) {
					this.SetMenu();
				}
				Expanding = value;   

				// rotate add button
				float angle = this.Expanding ? -1.0f * Math.PI / 4.0f : 0.0f;
				Animate (
					kApexMenuStartMenuDefaultAnimationDuration,
					() => {
						StartButton.Transform = CGAffineTransform.MakeRotation(angle);
					}
				);

				// expand or close animation
				if (!Timer) 
				{
					Flag = this.Expanding ? 0 : (MenusArray.Count - 1);

					Action Selector = (this.Expanding) ? 
						() => {
							this.Expand();
						} : 
						() => {
							this.Close();
						};

					Timer = NSTimer.CreateRepeatingTimer (TimeOffset, Selector);
					NSRunLoop.Current.AddTimer(Timer, NSRunLoopMode.Common);
					IsAnimating = true;
				}
			}
		}

		public ApexMenu(RectangleF frame, ApexMenuItem startItem, List<ApexMenuItem> aMenusArray) : base(frame)
		{
			this.BackgroundColor = UIColor.Clear;

			this.NearRadius = kApexMenuDefaultNearRadius;
			this.EndRadius = kApexMenuDefaultEndRadius;
			this.FarRadius = kApexMenuDefaultFarRadius;
			this.TimeOffset = kApexMenuDefaultTimeOffset;
			this.RotateAngle = kApexMenuDefaultRotateAngle;
			this.MenuWholeAngle = kApexMenuDefaultMenuWholeAngle;
			this.StartPoint = new PointF(kApexMenuDefaultStartPointX, kApexMenuDefaultStartPointY);
			this.ExpandRotation = kApexMenuDefaultExpandRotation;
			this.CloseRotation = kApexMenuDefaultCloseRotation;
			this.AnimationDuration = kApexMenuDefaultAnimationDuration;

			this.MenusArray = aMenusArray;

			// assign startItem to "Add" Button.
			this.StartButton = startItem;
			//_startButton.delegate = self;
			StartButton.Center = this.StartPoint;
			this.AddSubview(StartButton);
		}

		//UIView's methods

		public bool PointInside(PointF point, UIEvent _event)
		{
			// if the menu is animating, prevent touches
			if (IsAnimating) 
			{
				return false;
			}
			// if the menu state is expanding, everywhere can be touch
			// otherwise, only the add button are can be touch
			if (Expanding == true) 
			{
				return true;
			}
			else
			{
				return StartButton.Frame.Contains(Point);
			}
		}

		public bool TouchesBegan(NSSet touches, UIEvent _event)
		{
			this.Expanding = !this.Expanding;
		}

		//AwesomeMenuItem delegates

		public void ApexMenuItemTouchesBegan(ApexMenuItem item)
		{
			if (item == StartButton) 
			{
				this.Expanding = !this.Expanding;
			}
		}

		public void ApexMenuItemTouchesEnd(ApexMenuItem item)
		{
			// exclude the "add" button
			if (item == StartButton) 
			{
				return;
			}
			// blowup the selected menu button
			CAAnimationGroup blowup = this.BlowupAnimationAtPoint(item.Center);
			item.Layer.AddAnimation(blowup, "blowup");
			item.Center = item.StartPoint;

			// shrink other menu buttons
			for (int i = 0; i < MenusArray.Count; i ++)
			{
				ApexMenuItem otherItem = MenusArray[i];
				CAAnimationGroup shrink = this.ShrinkAnimationAtPoint(otherItem.center);
				if (otherItem.Tag == item.Tag) {
					continue;
				}
				otherItem.Layer.AddAnimation(shrink, "shrink");

				otherItem.Center = otherItem.StartPoint;
			}
			Expanding = false;

			// rotate start button
			float angle = this.Expanding ? Math.PI / 4.0f : 0.0f;
			Animate (AnimationDuration, () => {
				StartButton.Transform = CGAffineTransform.MakeRotation(angle);
			});

			//if ([_delegate respondsToSelector:@selector(awesomeMenu:didSelectIndex:)])
			//{
			//	[_delegate awesomeMenu:self didSelectIndex:item.tag - 1000];
			//}
		}

		//Instant methods
		public void SetMenusArray(List<ApexMenuItem> aMenusArray)
		{	
			if (aMenusArray == MenusArray)
			{
				return;
			}
			MenusArray = new List<ApexMenuItem> (aMenusArray);

			// clean subviews
			foreach (UIView v in this.SubViews) 
			{
				if (v.Tag >= 1000) 
				{
					v.RemoveFromSuperview ();
				}
			}
		}

		private void SetMenu() {
			int count = MenusArray.Count;
			for (int i = 0; i < count; i ++)
			{
				ApexMenuItem item = MenusArray[i];
				item.Tag = 1000 + i;
				item.StartPoint = StartPoint;

				// avoid overlap
				if (MenuWholeAngle >= Math.PI * 2) {
					MenuWholeAngle = MenuWholeAngle - MenuWholeAngle / count;
				}
				PointF endPoint = new PointF(StartPoint.X + EndRadius * (float)Math.Sin(i * MenuWholeAngle / (count - 1)), StartPoint.Y - EndRadius * (float)Math.Cos(i * MenuWholeAngle / (count - 1)));
				item.EndPoint = RotateCGPointAroundCenter (endPoint, StartPoint, RotateAngle);
				PointF nearPoint = new PointF(StartPoint.X + NearRadius * (float)Math.Sin(i * MenuWholeAngle / (count - 1)), StartPoint.Y - NearRadius * (float)Math.Cos(i * MenuWholeAngle / (count - 1)));
				item.NearPoint = RotateCGPointAroundCenter(nearPoint, StartPoint, RotateAngle);
				PointF farPoint = new PointF(StartPoint.X + FarRadius * (float)Math.Sin(i * MenuWholeAngle / (count - 1)), StartPoint.Y - FarRadius * (float)Math.Cos(i * MenuWholeAngle / (count - 1)));
				item.FarPoint = RotateCGPointAroundCenter(farPoint, StartPoint, RotateAngle); 
				item.Center = item.StartPoint;
				//item.delegate = this;
				this.InsertSubviewBelow (item, StartButton);
			}
		}
	
		//Private methods
		private void Expand()
		{
			if (Flag == MenusArray.Count) 
			{
				IsAnimating = flase;
				Timer.Invalidate ();
				Timer = null;
				return;
			}

			var Tag = 1000 + Flag;
			ApexMenuItem item = this.ViewWithTag(Tag);
			var rotateAnimation = (CAKeyFrameAnimation)CAKeyFrameAnimation.FromKeyPath("transform.rotation.z");
			rotateAnimation.Values   = new NSNumber[] {this.ExpandRotation, 0.0f};
			rotateAnimation.Duration = AnimationDuration;
			rotateAnimation.KeyTimes = new NSNumber[] {0.3f, 0.4f}; 

			var positionAnimation = (CAKeyFrameAnimation)CAKeyFrameAnimation.FromKeyPath ("position");
			positionAnimation.Duration = AnimationDuration;

			var path = new CGPath();
			path.MoveToPoint(item.StartPoint.X, item.StartPoint.Y);
			path.AddLineToPoint(item.FarPoint.X, item.FarPoint.Y);
			path.AddLineToPoint(item.NearPoint.X, item.NearPoint.Y);
			path.AddLineToPoint(item.EndPoint.X, item.EndPoint.Y);
			positionAnimation.Path = path;

			var animationGroup = new CAAnimationGroup();
			animationGroup.Animations = new CAAnimation[] {positionAnimation, rotateAnimation};
			animationGroup.Duration   = AnimationDuration;
			animationGroup.FillMode   = CAFillMode.Forwards;
			animationGroup.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseIn);
			//animationgroup.delegate = self;

			if(Flag == MenusArray.Count - 1){
				animationGroup.SetValueForKey ("firstAnimation", "id");
			}
			item.Layer.AddAnimation (animationGroup, "Expand");
			item.Center = item.EndPoint;
			Flag++;
		}

		private void Close()
		{
			if (Flag == -1)
			{
				IsAnimating = false;
				Timer.Invalidate();
				Timer = null;
				return;
			}

			int tag = 1000 + _flag;
			AwesomeMenuItem *item = (AwesomeMenuItem *)[self viewWithTag:tag];

			CAKeyframeAnimation *rotateAnimation = [CAKeyframeAnimation animationWithKeyPath:@"transform.rotation.z"];
			rotateAnimation.values = [NSArray arrayWithObjects:[NSNumber numberWithFloat:0.0f],[NSNumber numberWithFloat:closeRotation],[NSNumber numberWithFloat:0.0f], nil];
			rotateAnimation.duration = animationDuration;
			rotateAnimation.keyTimes = [NSArray arrayWithObjects:
			                            [NSNumber numberWithFloat:.0], 
			                            [NSNumber numberWithFloat:.4],
			                            [NSNumber numberWithFloat:.5], nil]; 

			CAKeyframeAnimation *positionAnimation = [CAKeyframeAnimation animationWithKeyPath:@"position"];
			positionAnimation.duration = animationDuration;
			CGMutablePathRef path = CGPathCreateMutable();
			CGPathMoveToPoint(path, NULL, item.endPoint.x, item.endPoint.y);
			CGPathAddLineToPoint(path, NULL, item.farPoint.x, item.farPoint.y);
			CGPathAddLineToPoint(path, NULL, item.startPoint.x, item.startPoint.y); 
			positionAnimation.path = path;
			CGPathRelease(path);

			CAAnimationGroup *animationgroup = [CAAnimationGroup animation];
			animationgroup.animations = [NSArray arrayWithObjects:positionAnimation, rotateAnimation, nil];
			animationgroup.duration = animationDuration;
			animationgroup.fillMode = kCAFillModeForwards;
			animationgroup.timingFunction = [CAMediaTimingFunction functionWithName:kCAMediaTimingFunctionEaseIn];
			animationgroup.delegate = self;
			if(_flag == 0){
				[animationgroup setValue:@"lastAnimation" forKey:@"id"];
			}

			[item.layer addAnimation:animationgroup forKey:@"Close"];
			item.center = item.startPoint;

			_flag --;
		}

		private static PointF RotateCGPointAroundCenter(PointF point, PointF center, float angle)
		{
			CGAffineTransform translation = CGAffineTransform.MakeTranslation(center.X, center.Y);
			CGAffineTransform rotation = CGAffineTransform.MakeRotation(angle);

			var inverted = CGAffineTransform.CGAffineTransformInvert(translation);

			CGAffineTransform transformGroup = inverted * rotation * translation;

			return transformGroup.TransformPoint(point);
		}



	}
}

