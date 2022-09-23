using Foundation;
using MusicViewer;
using MusicViewer.iOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(InfoCreatorEntry), typeof(InfoCreatorEntryRenderer))]
namespace MusicViewer.iOS
{
    class InfoCreatorEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
                return;

            Control.TintColor = UIColor.Brown;
            Control.BorderStyle = UITextBorderStyle.Line;
        }
    }
}