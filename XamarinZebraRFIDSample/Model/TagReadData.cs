using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamarinZebraRFIDSample.Model
{
    public class TagReadData
    {
        public string EPC { get; internal set; }
        public string TID { get; internal set; }
        public string UMB { get; internal set; }
    }
}