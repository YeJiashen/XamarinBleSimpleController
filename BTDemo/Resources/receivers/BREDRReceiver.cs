using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using Android.Util;

namespace BTDemo.Resources.receivers
{
    class BREDRReceiver : BroadcastReceiver
    {

        private IDeviceFound listener;

        public BREDRReceiver(IDeviceFound listener)
        {
            this.listener = listener;
        }
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(BluetoothDevice.ActionFound))
            {
                BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                if (listener != null) {
                    listener.OnBREDRDeviceFound(device);
                }
            }
        }

        public interface IDeviceFound
        {
            void OnBREDRDeviceFound(BluetoothDevice device);
        }
    }
}