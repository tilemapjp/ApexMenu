using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace ApexMenuSample
{
	[Activity (Label = "ApexMenuSample", MainLauncher = true)]
	public class Activity1 : Activity
	{
		private static int[] ITEM_DRAWABLES = { 
			Resource.Drawable.composer_camera, 
			Resource.Drawable.composer_music,
			Resource.Drawable.composer_place, 
			Resource.Drawable.composer_sleep, 
			Resource.Drawable.composer_thought, 
			Resource.Drawable.composer_with 
		};

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			ApexMenu.Menu apexMenu = (ApexMenu.Menu) FindViewById(Resource.Id.apex_menu);

			int itemCount = ITEM_DRAWABLES.Length;
			for (int i = 0; i < itemCount; i++) {
				ImageView item = new ImageView(this);
				item.SetImageResource(ITEM_DRAWABLES[i]);

				int position = i;
				Action<View> listener = (View v) => {
					Toast.MakeText (this, "position:" + position, ToastLength.Short).Show ();
				};
				apexMenu.AddItem (item, listener);
			}



			// Get our button from the layout resource,
			// and attach an event to it
			//Button button = FindViewById<Button> (Resource.Id.myButton);
			//
			//button.Click += delegate {
			//	button.Text = string.Format ("{0} clicks!", count++);
			//};
		}
	}
}


