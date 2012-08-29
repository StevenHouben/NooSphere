using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;

namespace ActivityDroid
{
    [BroadcastReceiver]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" })] 
    public class TextMessageListener : BroadcastReceiver
    {
        public static readonly string INTENT_ACTION = "android.provider.Telephony.SMS_RECEIVED";

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == INTENT_ACTION)
            {
                Toast.MakeText(context, "Msg received!", ToastLength.Short).Show();
                var bundle = intent.Extras;

                if (bundle != null)
                {
                    var pdus = bundle.Get ("pdus");
                    var castedPdus = JNIEnv.GetArray<Java.Lang.Object>(pdus.Handle);

                    var bytes = new Byte[JNIEnv.GetArrayLength(castedPdus[0].Handle)];
                    JNIEnv.CopyArray(castedPdus[0].Handle, bytes);
                    var message = Encoding.UTF8.GetString(bytes);
                    ((Main) context).ShowMessage(message);
                }
            }
        } 
    }
}