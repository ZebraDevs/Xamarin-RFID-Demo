using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XamarinZebraRFIDSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        ZebraReaderInterface readerInterface = null;
        ObservableCollection<string> readerOutputMessages = new ObservableCollection<string>();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            var resultListView = (ListView)FindViewById<ListView>(Resource.Id.resultListView);
            var buttonStartStop = (Button)FindViewById<Button>(Resource.Id.buttonStartStop);
            buttonStartStop.Click += OnClickButtonStartStop;

            var ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, readerOutputMessages);
            resultListView.Adapter = ListAdapter;

            // Init reader interface
            readerInterface = new ZebraReaderInterface(ReaderOutputHandler);
        }

        private void OnClickButtonStartStop(object sender, System.EventArgs e)
        {
            readerInterface.InitReader();
        }

        private void ReaderOutputHandler(object sender, string message)
        {
            readerOutputMessages.Add(message);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void Dispose(bool disposing)
        {
            if(readerInterface != null)
            {
                readerInterface.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}