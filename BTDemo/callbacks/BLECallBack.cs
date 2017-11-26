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

namespace BTDemo.Resources
{
    class BLEScanCallBack : BluetoothAdapter.ILeScanCallback
    {

        private IDeviceFound listener;

        public BLEScanCallBack(IDeviceFound listener)
        {
            this.listener = listener;
        }

        public IntPtr Handle
        {
            get
            {
                return new IntPtr();
            }
        }

        public void Dispose()
        {

        }

        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            if (listener != null)
            {
                listener.OnBleDeviceFound(device, rssi);
            }
        }

        public interface IDeviceFound
        {
            void OnBleDeviceFound(BluetoothDevice device, int rssi);
        }
    }
}