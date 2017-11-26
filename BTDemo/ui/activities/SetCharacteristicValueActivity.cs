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
using BTDemo.Resources.callbacks;
using BTDemo.utils;
using Java.Util;
using Android.Util;

namespace BTDemo.ui.activities
{
    [Activity(Label = "SetCharacteristicValueActivity")]
    public class SetCharacteristicValueActivity : Activity
        , View.IOnClickListener
        , GattCallBack.ICharacteristicNotifyListener
    {
        private TextView valueHex;
        private TextView valueStr;
        private EditText valueInput;
        private Button back;
        private Button setValue;
        private Button registNotify;
        private Button unregistNotify;

        private BluetoothGatt gatt;
        private BluetoothGattService service;
        private BluetoothGattCharacteristic characteristic;
        private GattCallBack callBack;

        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_set_characteristic_value);

            gatt = SingleGattService.GetInstance().CurGatt;
            service = SingleGattService.GetInstance().CurService;
            characteristic = SingleGattService.GetInstance().CurCharacteristic;
            callBack = SingleGattService.GetInstance().CurCallback;

            back = FindViewById<Button>(Resource.Id.settings_back);
            setValue = FindViewById<Button>(Resource.Id.settings_set_value);
            registNotify = FindViewById<Button>(Resource.Id.settings_regist_char_notify);
            unregistNotify = FindViewById<Button>(Resource.Id.settings_unregist_char_notify);
            valueHex = FindViewById<TextView>(Resource.Id.settings_value_hex);
            valueStr = FindViewById<TextView>(Resource.Id.settings_value_str);
            valueInput = FindViewById<EditText>(Resource.Id.settings_value_input);

            back.SetOnClickListener(this);
            setValue.SetOnClickListener(this);
            registNotify.SetOnClickListener(this);
            unregistNotify.SetOnClickListener(this);

            InitView();
        }

        private void InitView() {
            byte[] values = characteristic.GetValue();

            string strHex = "";
            string strStr = "";

            foreach (byte b in values)
            {
                strHex += b.ToString("X2") + ":";
            }
            strStr = Encoding.Default.GetString(values);

            valueHex.Text = strHex;
            valueStr.Text = strStr;

            Log.Error("dana.ye->xamarin->bt", strHex);
            Log.Error("dana.ye->xamarin->bt", strStr);
        }


        private Boolean SetNotifyEnable(bool enable) {
            if (gatt == null || characteristic == null)
                return false;

            bool isSuccess = gatt.SetCharacteristicNotification(characteristic, enable);

            if (!isSuccess)
                return false;

            BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
            byte[] value = new byte[BluetoothGattDescriptor.EnableNotificationValue.Count];

            for (int i = 0; i < BluetoothGattDescriptor.EnableNotificationValue.Count; i++) {
                value[i] = BluetoothGattDescriptor.EnableNotificationValue[i];
            }

            descriptor.SetValue(value);

            isSuccess = gatt.WriteDescriptor(descriptor);

            return isSuccess;
        }

        private void Back()
        {
            Finish();
        }

        private void SetValue()
        {
            String inputStr = valueInput.Text;
            String[] valStr = inputStr.Split(':');
            byte[] newValue = new byte[valStr.Length];

            for (int i = 0; i < valStr.Length; i++)
            {
                newValue[i] = Convert.ToByte(valStr[i], 16);
            }

            characteristic.SetValue(newValue);
            gatt.WriteCharacteristic(characteristic);
        }

        private void RegistNotify()
        {
            bool isSuccess = SetNotifyEnable(true);
            String result = isSuccess ? "成功" : "失败";

            Action action = () =>
            {
                String str = "注册通知 : " + result;
                Toast.MakeText(this, str, ToastLength.Short).Show();
            };

            RunOnUiThread(action);
        }

        private void UnregistNotify()
        {
            bool isSuccess = SetNotifyEnable(false);
            String result = isSuccess ? "成功" : "失败";

            Action action = () =>
            {
                String str = "注销通知 : " + result;
                Toast.MakeText(this, str, ToastLength.Short).Show();
            };

            RunOnUiThread(action);
        }


        public void OnCharacteristicCallback(BluetoothGattCharacteristic characteristic)
        {
            byte[] values = characteristic.GetValue();
            string uuid = characteristic.Uuid + "";
            string str = "changed characteristic's uuid is : " + characteristic.Uuid + "\r\n";
            str += "changed characteristic's WriteType is : " + characteristic.WriteType + "\r\n";
            str += "changed characteristic's GetValue is ";
            string value = "";
            foreach (byte b in values)
            {
                str += ": " + b.ToString("X2");
                value += b.ToString("X2") + ":";
            }

            Action act = () =>
            {
                Toast.MakeText(this, value, ToastLength.Short).Show();
            };
            RunOnUiThread(act);

            Log.Error("dana.ye->xamarin->bt", str);
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.settings_back:
                    Back();
                    break;
                case Resource.Id.settings_set_value:
                    SetValue();
                    break;
                case Resource.Id.settings_regist_char_notify:
                    RegistNotify();
                    break;
                case Resource.Id.settings_unregist_char_notify:
                    UnregistNotify();
                    break;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            //注册特征值改变的监听
            callBack.RemoveCharacteristicNotifyListener(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            //SingleGattService.GetInstance().CurService = service;
            //取消特征值改变的监听
            callBack.AddCharacteristicNotifyListener(this);
        }
    }
}