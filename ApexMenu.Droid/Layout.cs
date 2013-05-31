using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.Graphics;

/*package com.capricorn;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Rect;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewGroup;
import android.view.animation.AccelerateInterpolator;
import android.view.animation.Animation;
import android.view.animation.AnimationSet;
import android.view.animation.Interpolator;
import android.view.animation.LinearInterpolator;
import android.view.animation.OvershootInterpolator;
import android.view.animation.RotateAnimation;
import android.view.animation.Animation.AnimationListener;*/


namespace ApexMenu {
	public class Layout : ViewGroup {
		/**
	     * children will be set the same size.
    	 */
		private int mChildSize;

		private int mChildPadding = 5;

		private int mLayoutPadding = 10;

		public static float DEFAULT_FROM_DEGREES = 270.0f;

		public static float DEFAULT_TO_DEGREES = 360.0f;

		private float mFromDegrees = DEFAULT_FROM_DEGREES;

		private float mToDegrees = DEFAULT_TO_DEGREES;

		private static int MIN_RADIUS = 100;

		/* the distance between the layout's center and any child's center */
		private int mRadius;

		private bool mExpanded = false;

		public Layout(Context context) : base(context) 
		{
		}
	
		public Layout(Context context, IAttributeSet attrs) : base (context,attrs)
		{
			if (attrs != null) {
				TypedArray a = Context.ObtainStyledAttributes (attrs, Resource.Styleable.Layout, 0, 0);
				mFromDegrees = a.GetFloat(Resource.Styleable.Layout_fromDegrees, DEFAULT_FROM_DEGREES);

				mToDegrees   = a.GetFloat(Resource.Styleable.Layout_toDegrees,   DEFAULT_TO_DEGREES);
				mChildSize   = Math.Max(a.GetDimensionPixelSize(Resource.Styleable.Layout_childSize, 0), 0);

				a.Recycle ();
			}
		}
	
		private static int ComputeRadius(float arcDegrees, int childCount, int childSize,
		                                 int childPadding, int minRadius) {
			if (childCount < 2) {
				return minRadius;
			}

			float perDegrees = arcDegrees / (childCount - 1);
			float perHalfDegrees = perDegrees / 2;
			int   perSize = childSize + childPadding;

			int radius = (int) ((perSize / 2) / Math.Sin(perHalfDegrees/180*Math.PI));

			return Math.Max(radius, minRadius);
		}

		private static Rect ComputeChildFrame(int centerX, int centerY, int radius, float degrees,
		                                      int size) {

			double childCenterX = centerX + radius * Math.Cos(degrees/180*Math.PI);
			double childCenterY = centerY + radius * Math.Sin(degrees/180*Math.PI);

			return new Rect((int) (childCenterX - size / 2), (int) (childCenterY - size / 2),
			                (int) (childCenterX + size / 2), (int) (childCenterY + size / 2));
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
			int radius = mRadius = ComputeRadius(Math.Abs(mToDegrees - mFromDegrees), ChildCount, mChildSize,
			                                           mChildPadding, MIN_RADIUS);
			int size = radius * 2 + mChildSize + mChildPadding + mLayoutPadding * 2;

			SetMeasuredDimension(size, size);

			int count = ChildCount;
			for (int i = 0; i < count; i++) {
				GetChildAt(i).Measure(MeasureSpec.MakeMeasureSpec(mChildSize, MeasureSpecMode.Exactly),
				                      MeasureSpec.MakeMeasureSpec(mChildSize, MeasureSpecMode.Exactly));
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b) {
			int centerX = Width  / 2;
			int centerY = Height / 2;
			int radius  = mExpanded ? mRadius : 0;

			int childCount   = ChildCount;
			float perDegrees = (mToDegrees - mFromDegrees) / (childCount - 1);

			float degrees = mFromDegrees;
			for (int i = 0; i < childCount; i++) {
				Rect frame = ComputeChildFrame(centerX, centerY, radius, degrees, mChildSize);
				degrees += perDegrees;
				GetChildAt(i).Layout(frame.Left, frame.Top, frame.Right, frame.Bottom);
			}
		}

		/**
         * refers to {@link LayoutAnimationController#getDelayForView(View view)}
     	 */
		private static long ComputeStartOffset(int childCount, bool expanded, int index,
            float delayPercent, long duration, IInterpolator interpolator) {
        	float delay = delayPercent * duration;
        	long viewDelay = (long) (GetTransformedIndex(expanded, childCount, index) * delay);
        	float totalDelay = delay * childCount;

        	float normalizedDelay = viewDelay / totalDelay;
        	normalizedDelay = interpolator.GetInterpolation(normalizedDelay);

        	return (long) (normalizedDelay * totalDelay);
    	}

		private static int GetTransformedIndex(bool expanded, int count, int index) {
			if (expanded) {
				return count - 1 - index;
			}

			return index;
		}

		private static Animation CreateExpandAnimation(float fromXDelta, float toXDelta, float fromYDelta, float toYDelta,
		                                               long startOffset, long duration, IInterpolator interpolator) {
			Animation animation = new RotateAndTranslateAnimation(0, toXDelta, 0, toYDelta, 0, 720);
			animation.StartOffset  = startOffset;
			animation.Duration     = duration;
			animation.Interpolator = interpolator;
			animation.FillAfter    = true;

			return animation;
		}

		private static Animation CreateShrinkAnimation(float fromXDelta, float toXDelta, float fromYDelta, float toYDelta,
		                                               long startOffset, long duration, IInterpolator interpolator) {
			AnimationSet animationSet = new AnimationSet(false);
			animationSet.FillAfter = true;

			long preDuration = duration / 2;
			Animation rotateAnimation = new RotateAnimation(0, 360, Dimension.RelativeToSelf, 0.5f,
			                                                Dimension.RelativeToSelf, 0.5f);
			rotateAnimation.StartOffset  = startOffset;
			rotateAnimation.Duration     = preDuration;
			rotateAnimation.Interpolator = new LinearInterpolator ();
			rotateAnimation.FillAfter    = true;

			animationSet.AddAnimation (rotateAnimation);

			Animation translateAnimation    = new RotateAndTranslateAnimation(0, toXDelta, 0, toYDelta, 360, 720);
			translateAnimation.StartOffset  = startOffset + preDuration;
			translateAnimation.Duration     = duration - preDuration;
			translateAnimation.Interpolator = interpolator;
			translateAnimation.FillAfter    = true;

			animationSet.AddAnimation(translateAnimation);

			return animationSet;
		}

		private void BindChildAnimation(View child, int index, long duration) {
			bool expanded = mExpanded;
			int centerX = Width / 2;
			int centerY = Height / 2;
			int radius = expanded ? 0 : mRadius;

			int childCount = ChildCount;
			float perDegrees = (mToDegrees - mFromDegrees) / (childCount - 1);
			Rect frame = ComputeChildFrame(centerX, centerY, radius, mFromDegrees + index * perDegrees, mChildSize);

			int toXDelta = frame.Left - child.Left;
			int toYDelta = frame.Top  - child.Top;

			IInterpolator interpolator = mExpanded ? (IInterpolator)(new AccelerateInterpolator()) : (IInterpolator)(new OvershootInterpolator(1.5f));
			long startOffset = ComputeStartOffset(childCount, mExpanded, index, 0.1f, duration, interpolator);

			Animation animation = mExpanded ? CreateShrinkAnimation(0, toXDelta, 0, toYDelta, startOffset, duration,
			                                                        interpolator) : CreateExpandAnimation(0, toXDelta, 0, toYDelta, startOffset, duration, interpolator);

			bool isLast = GetTransformedIndex(expanded, childCount, index) == childCount - 1;

			animation.AnimationStart += (sender, e) => {};
			animation.AnimationRepeat += (sender, e) => {};
			animation.AnimationEnd += (sender, e) => {
				if (isLast) {
					PostDelayed(OnAllAnimationsEnd,0);
				}
			};

			child.Animation = animation;
		}

		public bool IsExpanded() {
			return mExpanded;
		}

		public void SetArc(float fromDegrees, float toDegrees) {
			if (mFromDegrees == fromDegrees && mToDegrees == toDegrees) {
				return;
			}

			mFromDegrees = fromDegrees;
			mToDegrees = toDegrees;

			RequestLayout();
		}

		public void SetChildSize(int size) {
			if (mChildSize == size || size < 0) {
				return;
			}

			mChildSize = size;

			RequestLayout();
		}

		/**
    	 * switch between expansion and shrinkage
    	 * 
    	 * @param showAnimation
	     */
		public void SwitchState(bool showAnimation) {
	        if (showAnimation) {
    	        int childCount = ChildCount;
        	    for (int i = 0; i < childCount; i++) {
					BindChildAnimation (GetChildAt(i), i, 300);
	            }
    	    }

 	       	mExpanded = !mExpanded;

        	if (!showAnimation) {
            	RequestLayout();
	        }
        
        	Invalidate();
	    }

		private void OnAllAnimationsEnd() {
			int childCount = ChildCount;
			for (int i = 0; i < childCount; i++) {
				GetChildAt(i).ClearAnimation();
			}

			RequestLayout();
		}
	}
}
