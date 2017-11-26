using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using BTDemo.Resources.callbacks;
using BTDemo.ui.adapters;
using BTDemo.utils;
using Java.Util;
using System;
using System.Collections.Generic;

namespace BTDemo.ui.activities
{
    //这是用来展示已连接蓝牙设备某服务中的特征值的View ;
    [Activity(Label = "CharacteristicsActivity")]
    public class CharacteristicsActivity : Activity
        , AdapterView.IOnItemClickListener, View.IOnClickListener
        , GattCallBack.ICharacteristicCallbackListener
    {
        private GridView characteristicsList;
        private Button back;
        private Button refreshCharacteristics;
        private Button stepintoCharacteristics;
        private GattCallBack callBack;
        private BluetoothGatt gatt;
        private BluetoothGattService service;
        private CharacteristicsAdapter adapter;
        private List<BluetoothGattCharacteristic> characteristics;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_characteristics);
            characteristics = new List<BluetoothGattCharacteristic>();
            gatt = SingleGattService.GetInstance().CurGatt;
            service = SingleGattService.GetInstance().CurService;
            callBack = SingleGattService.GetInstance().CurCallback;

            characteristicsList = FindViewById<GridView>(Resource.Id.characteristics_list);
            back = FindViewById<Button>(Resource.Id.characteristics_back);
            refreshCharacteristics = FindViewById<Button>(Resource.Id.characteristics_refresh);
            stepintoCharacteristics = FindViewById<Button>(Resource.Id.characteristics_stepinto);

            adapter = new CharacteristicsAdapter(this, characteristics);
            characteristicsList.Adapter = adapter;
            characteristicsList.OnItemClickListener = this;
            back.SetOnClickListener(this);
            refreshCharacteristics.SetOnClickListener(this);
            stepintoCharacteristics.SetOnClickListener(this);

            RefreshCharacteristics();
        }


        private void Back()
        {
            Finish();
        }

        private void StepintoCharacteristic()
        {
            BluetoothGattCharacteristic c = characteristics[adapter.GetChooseItem()];
            gatt.ReadCharacteristic(c);
        }

        private void RefreshCharacteristics()
        {
            characteristics.Clear();
            adapter.SetChooseItem(-1);
            if (service == null || service.Characteristics == null)
            {
                refreshCharacteristics.Text = "刷新服务列表 (null)";
                Toast.MakeText(this, "当前gatt为空或者服务为空", ToastLength.Short).Show();
            }

            foreach (BluetoothGattCharacteristic characteristic in service.Characteristics)
            {
                characteristics.Add(characteristic);
                adapter.NotifyDataSetChanged();
                Log.Error("dana.ye->xamarin->bt", "Cur Service is : " + service.Uuid);
            }
            refreshCharacteristics.Text = "刷新服务列表 (" + characteristics.Count + ")";
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.characteristics_back:
                    Back();
                    break;
                case Resource.Id.characteristics_refresh:
                    RefreshCharacteristics();
                    break;
                case Resource.Id.characteristics_stepinto:
                    StepintoCharacteristic();
                    break;
            }
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            adapter.SetChooseItem(position);
            adapter.NotifyDataSetChanged();
        }

        public void OnCharacteristicCallback(BluetoothGattCharacteristic characteristic)
        {
            /*byte[] values = characteristic.GetValue();
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
            
            Log.Error("dana.ye->xamarin->bt", str);*/

            BluetoothGattCharacteristic c = characteristics[adapter.GetChooseItem()];

            if (c.GetValue() == null) {
                Action action = () =>
                {
                    String str = "当前特征值的values为空 , 请换一个特征值查看!";
                    Toast.MakeText(this, str, ToastLength.Short).Show();
                };

                RunOnUiThread(action);
            }
            else { 
                SingleGattService.GetInstance().CurCharacteristic = c;
                Intent intent = new Intent();
                intent.SetClass(this, typeof(SetCharacteristicValueActivity));
                StartActivity(intent);
            }
        }


        protected override void OnPause()
        {
            base.OnPause();
            //注册特征值改变的监听
            callBack.RemoveCharacteristicCallbackListener(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            //SingleGattService.GetInstance().CurService = service;
            //取消特征值改变的监听
            callBack.AddCharacteristicCallbackListener(this);
        }
    }
}