using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidX.AppCompat.App;
using Com.Zebra.Rfid.Api3;
using System.Collections.Generic;
using System.Threading.Tasks;
using XamarinZebraRFIDSample.Model;
using XamarinZebraRFIDSample.ZebraReader;

namespace XamarinZebraRFIDSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button buttonConnection = null;
        ProgressBar progressBar = null;
        CheckBox checkBoxEPC;
        CheckBox checkBoxTID;
        CheckBox checkBoxMB;

        ZebraReaderInterface readerInterface = null;
        List<string> tagList = new List<string>();
        ArrayAdapter<string> tagReadListAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            buttonConnection = FindViewById<Button>(Resource.Id.buttonStartStop);
            buttonConnection.Click += OnClickButtonStartStop;
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            checkBoxEPC = FindViewById<CheckBox>(Resource.Id.checkBoxEPC);
            checkBoxEPC.Click += CheckBoxEPC_Click;
            checkBoxTID = FindViewById<CheckBox>(Resource.Id.checkBoxTID);
            checkBoxEPC.Click += CheckBoxTID_Click;
            checkBoxMB = FindViewById<CheckBox>(Resource.Id.checkBoxUserData);
            checkBoxEPC.Click += CheckBoxMB_Click;

            var resultListView = FindViewById<ListView>(Resource.Id.resultListView);
            tagReadListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, tagList);
            resultListView.SetAdapter(tagReadListAdapter);

            // Init reader interface
            readerInterface = new ZebraReaderInterface();
            readerInterface.ReaderOutputNotification = ReaderOutputMessage;
            readerInterface.ReaderTagDataEventOutput = OnTagRead;
        }

        private void CheckBoxEPC_Click(object sender, System.EventArgs e)
        {
            readerInterface.SetMemoryBankRead(MEMORY_BANK.MemoryBankEpc, checkBoxEPC.Checked);
        }
        private void CheckBoxTID_Click(object sender, System.EventArgs e)
        {
            readerInterface.SetMemoryBankRead(MEMORY_BANK.MemoryBankTid, checkBoxTID.Checked);
        }
        private void CheckBoxMB_Click(object sender, System.EventArgs e)
        {
            readerInterface.SetMemoryBankRead(MEMORY_BANK.MemoryBankUser, checkBoxMB.Checked);
        }

        private void OnClickButtonStartStop(object sender, System.EventArgs e)
        {
            Task.Run(() =>
            {
                var buttonText = readerInterface.IsConnected ? "Disconnecting..." : "Connecting...";
                this.RunOnUiThread(() =>
                {
                    buttonConnection.Text = buttonText;
                    buttonConnection.Enabled = false;
                    progressBar.Visibility = Android.Views.ViewStates.Visible;
                });

                if (!readerInterface.IsConnected)
                {
                    var result = readerInterface.ConnectReader();
                    buttonText = result ? "Disconnect Reader" : "Connect Reader";
                }
                else
                {
                    var result = readerInterface.DisconnectReader();
                    buttonText = result ? "Connect Reader" : "Disconnect Reader";
                }
                this.RunOnUiThread(() =>
                {
                    buttonConnection.Text = buttonText;
                    buttonConnection.Enabled = true;
                    progressBar.Visibility = Android.Views.ViewStates.Invisible;
                });

            });
        }

        private void OnTagRead(object sender, TagReadData tagReadData)
        {
            string item = "";
            if (!string.IsNullOrEmpty(tagReadData.EPC))
                item = item + "\nEPC: " + tagReadData.EPC;
            if (!string.IsNullOrEmpty(tagReadData.TID))
                item = item + "\nTID: " + tagReadData.TID;
            if (!string.IsNullOrEmpty(tagReadData.UMB))
                item = item + "\nUser: " + tagReadData.UMB;

            if (item.Length == 0)
                return;

            item.Remove(0, 1); // remove the first \n

            this.RunOnUiThread(() =>
            {
                tagReadListAdapter.Insert(item, 0);
            });
        }

        private void ReaderOutputMessage(object sender, string message)
        {
            Log.Debug("Reader Output", message);

            this.RunOnUiThread(() =>
            {
                Toast.MakeText(this, message, ToastLength.Short).Show();
            });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void Dispose(bool disposing)
        {
            if (readerInterface != null)
            {
                readerInterface.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}