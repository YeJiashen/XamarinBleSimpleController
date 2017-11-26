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
using System;
using System.Collections.Generic;

namespace BTDemo.ui.activities
{
    //这是用来展示已连接的蓝牙设备中的服务并可以返回上一级和进入服务查看都有哪些特征值的View ;
    [Activity(Label = "ServiceListActivity")]
    public class ServiceListActivity : Activity
        , AdapterView.IOnItemClickListener, View.IOnClickListener
    {
        private GridView serviceList;
        private Button back;
        private Button refreshService;
        private Button stepintoService;
        private BluetoothGatt gatt;
        private ServicesAdapter adapter;
        private List<BluetoothGattService> services;

        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_service);
            services = new List<BluetoothGattService>();
            gatt = SingleGattService.GetInstance().CurGatt;

            serviceList = FindViewById<GridView>(Resource.Id.service_list);
            back = FindViewById<Button>(Resource.Id.service_back);
            refreshService = FindViewById<Button>(Resource.Id.service_refresh);
            stepintoService = FindViewById<Button>(Resource.Id.service_stepinto);

            adapter = new ServicesAdapter(this, services);
            serviceList.Adapter = adapter;
            serviceList.OnItemClickListener = this;
            back.SetOnClickListener(this);
            refreshService.SetOnClickListener(this);
            stepintoService.SetOnClickListener(this);

            RefreshBluetoothDeviceService();
        }


        private void Back()
        {
            Finish();
        }

        private void RefreshBluetoothDeviceService() {
            services.Clear();
            adapter.SetChooseItem(-1);
            if (gatt == null || gatt.Services == null) {
                refreshService.Text = "刷新服务列表 (null)";
                Toast.MakeText(this, "当前gatt为空或者服务为空", ToastLength.Short).Show();
            }
            
            foreach (BluetoothGattService service in gatt.Services)
            {
                services.Add(service);
                adapter.NotifyDataSetChanged();
                Log.Error("dana.ye->xamarin->bt", "Cur Service is : " + service.Uuid);
            }
            refreshService.Text = "刷新服务列表 (" + services.Count + ")";
        }

        private void StepintoService() {
            BluetoothGattService service = services[adapter.GetChooseItem()];

            SingleGattService.GetInstance().CurService = service;
            Intent intent = new Intent();
            intent.SetClass(this, typeof(CharacteristicsActivity));
            StartActivity(intent);
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.service_back:
                    Back();
                    break;
                case Resource.Id.service_refresh:
                    RefreshBluetoothDeviceService();
                    break;
                case Resource.Id.service_stepinto:
                    StepintoService();
                    break;
            }
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            adapter.SetChooseItem(position);
            adapter.NotifyDataSetChanged();
        }
    }
}