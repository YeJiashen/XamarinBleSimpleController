using Android.Bluetooth;
using BTDemo.Resources.callbacks;

namespace BTDemo.utils
{
    class SingleGattService
    {
        private static SingleGattService instance;

        public string Address { get; set; }
        public BluetoothGatt CurGatt { get; set; }
        public BluetoothGattService CurService { get; set; }
        public BluetoothGattCharacteristic CurCharacteristic { get; set; }
        public GattCallBack CurCallback { get; set; }

        private SingleGattService() {}

        public static SingleGattService GetInstance() {
            if (instance == null) {
                instance = new SingleGattService();
            }
            return instance;
        }
    }
}