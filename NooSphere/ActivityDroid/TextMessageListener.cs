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
using NooSphere.ActivitySystem.Base;


namespace ActivityDroid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" })] 
    public class TextMessageListener : BroadcastReceiver
    {
        public static readonly string IntentAction = "android.provider.Telephony.SMS_RECEIVED";

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == IntentAction)
            {
                Bundle bundle = intent.Extras;

                if (bundle != null)
                {
                    var pdus = (Java.Lang.Object[])bundle.Get("pdus");

                    var msgs = pdus.Select(pdu => SmsMessage.CreateFromPdu((byte[])pdu)).ToArray();
                    var number = msgs.Select(msg => msg.OriginatingAddress.ToString()).FirstOrDefault();
                    var content = msgs.Select(msg => msg.MessageBody.ToString()).FirstOrDefault();
                    
                    var message = new NooSphere.ActivitySystem.Base.Message { Type = MessageType.Communication, Header = "ReceivedTextMessage", From = number, Content = content };
                    Main.Client.SendMessage(message);
                }
            }
        } 
    }
}