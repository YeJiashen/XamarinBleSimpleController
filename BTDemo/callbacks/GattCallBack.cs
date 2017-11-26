using Android.Bluetooth;
using Android.Runtime;
using Android.Util;
using System.Collections.Generic;

namespace BTDemo.Resources.callbacks
{
    class GattCallBack : BluetoothGattCallback
    {
        private IGattCallbackListener listener;

        private List<ICharacteristicCallbackListener> charListeners;
        private List<ICharacteristicNotifyListener> charNotifyListeners;

        public GattCallBack(IGattCallbackListener listener) {
            this.listener = listener;
            charListeners = new List<ICharacteristicCallbackListener>();
            charNotifyListeners = new List<ICharacteristicNotifyListener>();
        }

        //ble连接状态改变的回调
        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {

            Log.Error("dana.ye->xamarin->bt", "cur status is " + status);
            Log.Error("dana.ye->xamarin->bt", "cur newState is " + newState);
            if (status == GattStatus.Success)
            {
                    listener.OnGattCallBack();
            }
            else if(newState == ProfileState.Disconnected)
            {
                Log.Error("dana.ye->xamarin->bt", "gett.Disconnect()");
                // 防止出现status 133
                Log.Error("dana.ye->xamarin->bt", "关闭GATT");
                gatt.Close();
                listener.OnStatusNotSuccess();
            }
        }

        //ble读取服务中的特征值的回调 , 只能在这里读到 , 如果直接用服务去搜索然后getvalue拿到的就是空的
        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            lock (charListeners)
            {
                base.OnCharacteristicRead(gatt, characteristic, status);
                byte[] values = characteristic.GetValue();
                string uuid = characteristic.Uuid + "";
                string str = "Read characteristic's uuid is : " + characteristic.Uuid + "\r\n";
                str += "Read characteristic's WriteType is : " + characteristic.WriteType + "\r\n";
                str += "Read characteristic's GetValue is ";
                //Log.Error("dana.ye->xamarin->bt", str);
                foreach (byte b in values)
                {
                    str += ": " + b;
                }
                //Log.Error("dana.ye->xamarin->bt", str);

                foreach (ICharacteristicCallbackListener cListener in charListeners)
                {
                    cListener.OnCharacteristicCallback(characteristic);
                }
            }
        }


        //ble设置服务中的特征值的回调
        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            lock (charListeners)
            { 
                base.OnCharacteristicWrite(gatt, characteristic, status);
                byte[] values = characteristic.GetValue();
                string uuid = characteristic.Uuid + "";
                string str = "write characteristic's uuid is : " + characteristic.Uuid + "\r\n";
                str += "write characteristic's WriteType is : " + characteristic.WriteType + "\r\n";
                str += "write characteristic's GetValue is ";
                //Log.Error("dana.ye->xamarin->bt", str);
                foreach (byte b in values)
                {
                    str += ": " + b;
                }
                Log.Error("dana.ye->xamarin->bt", str);

                foreach (ICharacteristicCallbackListener cListener in charListeners)
                {
                    cListener.OnCharacteristicCallback(characteristic);
                }
            }
        }

        //ble注册的服务中的特征值改变的回调
        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            lock (charNotifyListeners)
            {
                base.OnCharacteristicChanged(gatt, characteristic);
                byte[] values = characteristic.GetValue();
                string uuid = characteristic.Uuid + "";
                string str = "changed characteristic's uuid is : " + characteristic.Uuid + "\r\n";
                str += "changed characteristic's WriteType is : " + characteristic.WriteType + "\r\n";
                str += "changed characteristic's GetValue is ";
                //Log.Error("dana.ye->xamarin->bt", str);
                foreach (byte b in values)
                {
                    str += ": " + b;
                }
                //Log.Error("dana.ye->xamarin->bt", str);

                foreach (ICharacteristicNotifyListener cListener in charNotifyListeners)
                {
                    cListener.OnCharacteristicCallback(characteristic);
                }
            }
        }

        //gatt连接状态改变的监听 , 必须实现
        public interface IGattCallbackListener
        {
            void OnGattCallBack();

            void OnStatusNotSuccess();
        }

        //服务中的特征值发生改变的监听 , 选择实现 , 引用消失时必须手动移除
        public interface ICharacteristicCallbackListener
        {
            void OnCharacteristicCallback(BluetoothGattCharacteristic characteristic);
        }

        //服务中的特征值发生改变的监听 , 选择实现 , 引用消失时必须手动移除
        public interface ICharacteristicNotifyListener
        {
            void OnCharacteristicCallback(BluetoothGattCharacteristic characteristic);
        }

        //添加参数改变监听的方法
        public bool AddCharacteristicCallbackListener(ICharacteristicCallbackListener listener) {
            lock (charListeners)
            {
                if (charListeners.Contains(listener))
                {
                    return false;
                }
                else
                {
                    charListeners.Add(listener);
                    return true;
                }
            }
        }

        //移除参数改变监听的方法
        public bool RemoveCharacteristicCallbackListener(ICharacteristicCallbackListener listener)
        {
            lock (charListeners) {
                if (charListeners.Contains(listener))
                {
                    charListeners.Remove(listener);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //添加参数改变监听的方法
        public bool AddCharacteristicNotifyListener(ICharacteristicNotifyListener listener)
        {
            lock (charNotifyListeners)
            {
                if (charNotifyListeners.Contains(listener))
                {
                    return false;
                }
                else
                {
                    charNotifyListeners.Add(listener);
                    return true;
                }
            }
        }

        //移除参数改变监听的方法
        public bool RemoveCharacteristicNotifyListener(ICharacteristicNotifyListener listener)
        {
            lock (charNotifyListeners)
            {
                if (charNotifyListeners.Contains(listener))
                {
                    charNotifyListeners.Remove(listener);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}