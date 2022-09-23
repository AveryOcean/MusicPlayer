using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MusicViewer;
using MusicViewer.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(InfoCreatorEntry), typeof(InfoCreatorEntryRenderer))]
namespace MusicViewer.Droid
{
    class InfoCreatorEntryRenderer : EntryRenderer
    {
        public InfoCreatorEntryRenderer(Context context):base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
                return;

            Control.SetHighlightColor(Android.Graphics.Color.Beige);
        }
    }
}