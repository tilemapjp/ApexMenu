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

/*
 * Copyright (C) 2012 Capricorn
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace ApexMenu {
	public class Menu : RelativeLayout {
		private Layout mLayout;

		private ImageView mHintView;

		public Menu(Context context) : base(context) {
			Init(context);
		}

		public Menu(Context context, IAttributeSet attrs) : base(context,attrs) {
			Init(context);
		}

		private void Init(Context context) {


			LayoutInflater li = (LayoutInflater)context.GetSystemService (Context.LayoutInflaterService);
			li.Inflate(Resource.Layout.menu, this);

			mLayout = (Layout)FindViewById (Resource.Id.item_layout);

			ViewGroup controlLayout = (ViewGroup) FindViewById(Resource.Id.control_layout);
			controlLayout.Clickable = true;

			controlLayout.Touch += (object sender, TouchEventArgs e) => {
				if (e.Event.Action == MotionEventActions.Down) {
					mHintView.StartAnimation(CreateHintSwitchAnimation(mLayout.IsExpanded()));
					mLayout.SwitchState(true);
				}
				e.Handled = false;
			};

			mHintView = (ImageView) FindViewById(Resource.Id.control_hint);
		}

		public void AddItem(int resId, Action<View> listener) {
			var frame = new FrameLayout (this.Context);
			LayoutInflater li = (LayoutInflater)this.Context.GetSystemService (Context.LayoutInflaterService);
			li.Inflate(Resource.Layout.item, frame);

			var bgframe  = (FrameLayout)frame.GetChildAt(0);
			var identity = (ImageView)bgframe.GetChildAt(0);

			identity.SetImageResource(resId);
			frame.RemoveView(bgframe);

			this.AddItem (bgframe, listener);
		}

		public void AddItem(View item, Action<View> listener) {
			mLayout.AddView(item);

			item.Click += (object sender, EventArgs e) => {
				var viewClicked = (View)sender;

				Animation animation = BindItemAnimation(viewClicked, true, 400);
				animation.AnimationStart  += (object sender_1, Animation.AnimationStartEventArgs e_1)  => {};
				animation.AnimationRepeat += (object sender_1, Animation.AnimationRepeatEventArgs e_1) => {};
				animation.AnimationEnd    += (object sender_1, Animation.AnimationEndEventArgs e_1)    => {
					PostDelayed(ItemDidDisappear,0);
				};

				int itemCount = mLayout.ChildCount;
				for (int i = 0; i < itemCount; i++) {
					View item_1 = mLayout.GetChildAt(i);
					if (viewClicked != item_1) {
						BindItemAnimation(item_1, false, 300);
					}
				}

				mLayout.Invalidate();
				mHintView.StartAnimation(CreateHintSwitchAnimation(true));

				if (listener != null) {
					listener(viewClicked);
				}
			};
		}

		private Animation BindItemAnimation(View child, bool isClicked, long duration) {
			Animation animation = CreateItemDisapperAnimation(duration, isClicked);
			child.Animation = animation;

			return animation;
		}

		private void ItemDidDisappear() {
			int itemCount = mLayout.ChildCount;
			for (int i = 0; i < itemCount; i++) {
				View item = mLayout.GetChildAt (i);
				item.ClearAnimation ();
			}

			mLayout.SwitchState(false);
		}

		private static Animation CreateItemDisapperAnimation(long duration, bool isClicked) {
			AnimationSet animationSet = new AnimationSet(true);
			animationSet.AddAnimation(new ScaleAnimation(1.0f, isClicked ? 2.0f : 0.0f, 1.0f, isClicked ? 2.0f : 0.0f,
			                                             Dimension.RelativeToSelf, 0.5f, Dimension.RelativeToSelf, 0.5f));
			animationSet.AddAnimation(new AlphaAnimation(1.0f, 0.0f));

			animationSet.Duration     = duration;
			animationSet.Interpolator = new DecelerateInterpolator();
			animationSet.FillAfter    = true;

			return animationSet;
		}

		private static Animation CreateHintSwitchAnimation(bool expanded) {
			Animation animation = new RotateAnimation(expanded ? 45 : 0, expanded ? 0 : 45, Dimension.RelativeToSelf,
			                                          0.5f, Dimension.RelativeToSelf, 0.5f);
			animation.StartOffset  = 0;
			animation.Duration     = 100;
			animation.Interpolator = new DecelerateInterpolator();
			animation.FillAfter    = true;

			return animation;
		}
	}
}
