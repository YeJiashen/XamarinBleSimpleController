using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using BTDemo.Resources.receivers;
using BTDemo.ui.adapters;
using System;
using System.Collections.Generic;
using Android.Views;
using BTDemo.Resources.callbacks;
using BTDemo.utils;
using BTDemo.ui.activities;

namespace BTDemo
{
    //这是用来展示周围蓝牙设备并可以进行与特定设备连接操作的View ;
    [Activity(Label = "BTDemo", MainLauncher = true)]
    public class MainActivity : Activity , BREDRReceiver.IDeviceFound 
        , AdapterView.IOnItemClickListener , View.IOnClickListener
        , GattCallBack.IGattCallbackListener /*, BLEScanCallBack.IDeviceFound*/
    {

        private GridView deviceList;
        private Button refreshBluedevice;
        private Button startBLEConnection;
        private Button getServices;
        private DevicesAdapter adapter;
        private Handler handler = new Handler();
        private Action action;
        private BluetoothAdapter btAdapter;

        private BluetoothGatt gatt;

        public List<BluetoothDevice> bondedDevices = null;
        public List<BluetoothDevice> searchedDevices = null;
        private BREDRReceiver btReceiver = null;
        //经过测试 , 即使不使用StartLeScan()方法 
        //在DeviceFound的广播里面也能搜到Le和Classic的设备 ;
        //所以这里注释掉这个callback ;
        //private BLEScanCallBack bleScanCallback = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            //初始化View控件
            deviceList = FindViewById<GridView>(Resource.Id.device_list);
            refreshBluedevice = FindViewById<Button>(Resource.Id.ble_refresh);
            startBLEConnection = FindViewById<Button>(Resource.Id.ble_connect);
            getServices = FindViewById<Button>(Resource.Id.ble_getservices);
            searchedDevices = new List<BluetoothDevice>();
            adapter = new DevicesAdapter(this, searchedDevices);
            deviceList.Adapter = adapter;

            deviceList.OnItemClickListener = this;
            startBLEConnection.SetOnClickListener(this);
            getServices.SetOnClickListener(this);
            refreshBluedevice.SetOnClickListener(this);
            startBLEConnection.Enabled = false;
            

            //----------------------------------------------------------------------
            //-----------------------下面是开启业务逻辑的部分------------------------
            //----------------------------------------------------------------------

            BluetoothManager bm = (BluetoothManager)GetSystemService(Context.BluetoothService);

            //BluetoothAdapter bt = BluetoothAdapter.DefaultAdapter;
            btAdapter = bm.Adapter;
            bondedDevices = new List<BluetoothDevice>(btAdapter.BondedDevices);
            

            foreach (BluetoothDevice device in bondedDevices) {
                searchedDevices.Add(device);
                adapter.NotifyDataSetChanged();
            }

            if (!btAdapter.IsEnabled)
            {
                btAdapter.Enable();
            }

            //开启BR/EDR的蓝牙扫描(基于SPP协议的蓝牙设备发现) ;
            btAdapter.StartDiscovery();
            Log.Error("dana.ye->xamarin->bt", "开始扫描周围的BR/EDR蓝牙设备");
            //开启BLE的蓝牙扫描(基于BLE协议的蓝牙设备发现)
            //StartLeScan 是一个过时的方法 , 但是确实需要用到它
            /*
            bleScanCallback = new BLEScanCallBack(this);
            try
            {
                #pragma warning disable CS0618 // 类型或成员已过时
                btAdapter.StartLeScan(bleScanCallback);
                #pragma warning restore CS0618 // 类型或成员已过时
                Log.Error("dana.ye->xamarin->bt", "开始扫描周围的BLE蓝牙设备");
            }
            catch (Exception e)
            {
                Log.Error("dana.ye->xamarin->bt", "stopLeScan时出错 : " + e.Message);
            }
            
            */



            //20秒后关闭扫描 , 这里的语法和java有些不同 , 主要是因为C#不支持匿名内部对象的原因导致的
            //这样也有好处 , 就是对象方便管理 , 但是在写某些简单逻辑的时候就比较麻烦了
            action = () =>
            {
                //关闭BR/EDR的蓝牙扫描(基于SPP协议的蓝牙设备发现) ;
                btAdapter.CancelDiscovery();
                //关闭BLE设备的扫描
                /*
                try
                {
                    pragma warning disable CS0618 // 类型或成员已过时
                    btAdapter.StopLeScan(bleScanCallback);
                    #pragma warning restore CS0618 // 类型或成员已过时
                }
                catch (Exception e) {
                    Log.Error("dana.ye->xamarin->bt", "stopLeScan时出错 : " + e.Message);
                }
               
                */
                Log.Error("dana.ye->xamarin->bt", "自动停止扫描周围的蓝牙设备");
            };
            handler.PostDelayed(action , 10000);
            //到这里 , 定时关闭的任务就开启完成了 , 就等待10s后关闭扫描了 ;
        }

        //-------------------------------------------注册监听器相关-----------------------------------------------

        //BREDRReceiver.IDeviceFound
        public void OnBREDRDeviceFound(BluetoothDevice device)
        {
            if (device == null ||
                device.Name == null || device.Name.Equals("") ||
                device.Address == null || device.Address.Equals(""))
            {
                Log.Error("dana.ye->xamarin->bt", "cur Device Name or Address is null ");
                return;
            }

            searchedDevices.Add(device);
            adapter.NotifyDataSetChanged();
        }

        //BLEScanCallBack.IDeviceFound
        /*public void OnBleDeviceFound(BluetoothDevice device, int rssi)
        {
            if (device == null ||
                device.Name == null || device.Name.Equals("") ||
                device.Address == null || device.Address.Equals(""))
            {
                Log.Error("dana.ye->xamarin->bt", "cur Device Name or Address is null ");
                return;
            }

            if (searchedDevices.Contains(device)) {
                Log.Error("dana.ye->xamarin->bt", "cur device has Contained : " + device.Name);
            }
            searchedDevices.Add(device);
            adapter.NotifyDataSetChanged();
        }*/

        //GattCallBack.IGattCallbackListener
        public void OnGattCallBack()
        {
            Log.Error("dana.ye->xamarin->bt", "进入了成功连接的回调");
            Log.Error("dana.ye->xamarin->bt", "gatt.DiscoverServices()" + gatt.DiscoverServices());
            gatt.DiscoverServices();
            Action act = () =>
            {
                Toast.MakeText(this, "连接成功", ToastLength.Short).Show();
                SingleGattService.GetInstance().CurGatt = gatt;
                Intent intent = new Intent();
                intent.SetClass(this, typeof(ServiceListActivity));
                StartActivity(intent);
            };
            RunOnUiThread(act);
        }

        //GattCallBack.IGattCallbackListener 防止satats = 133
        public void OnStatusNotSuccess()
        {
            Action act = () =>
            {
                Toast.MakeText(this, "连接失败", ToastLength.Short).Show();
            };
            RunOnUiThread(act);
            action = () =>
            {
                OnDeviceReConnect();
            };
            handler.PostDelayed(action, 1000);
            //OnDeviceConnect();
        }

        //-------------------------------------------界面点击相关-----------------------------------------------
        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            adapter.SetChooseItem(position);
            adapter.NotifyDataSetChanged();

            BluetoothDevice device = searchedDevices[position];
            if (device.Type == BluetoothDeviceType.Le
                    || device.Type == BluetoothDeviceType.Classic
                    || device.Type == BluetoothDeviceType.Dual)
            {
                startBLEConnection.Enabled = true;
            }
            else
            {
                startBLEConnection.Enabled = false;
            }
        }

        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.ble_refresh:
                    RefreshBluetoothDeviceList();
                    break;
                case Resource.Id.ble_connect:
                    OnDeviceConnect();
                    break;
                case Resource.Id.ble_getservices:
                    GetCurDeviceServices();
                    break;
            }
        }

        private void RefreshBluetoothDeviceList() {
            Action act = () =>
            {
                btAdapter.CancelDiscovery();
                handler.RemoveCallbacks(action);
                searchedDevices.Clear();
                adapter.SetChooseItem(-1);
                adapter.NotifyDataSetChanged();
                btAdapter.StartDiscovery();
                foreach (BluetoothDevice device in bondedDevices)
                {
                    searchedDevices.Add(device);
                    adapter.NotifyDataSetChanged();
                }
                Log.Error("dana.ye->xamarin->bt", "开始重新扫描周围的蓝牙设备");
            };

            RunOnUiThread(act);
            
            action = () =>
            {
                //关闭BR/EDR的蓝牙扫描(基于SPP协议的蓝牙设备发现) ;
                btAdapter.CancelDiscovery();
                //关闭BLE设备的扫描
                /*
                try
                {
                    pragma warning disable CS0618 // 类型或成员已过时
                    btAdapter.StopLeScan(bleScanCallback);
                    #pragma warning restore CS0618 // 类型或成员已过时
                }
                catch (Exception e) {
                    Log.Error("dana.ye->xamarin->bt", "stopLeScan时出错 : " + e.Message);
                }
               
                */
                Log.Error("dana.ye->xamarin->bt", "自动停止扫描周围的蓝牙设备");
            };
            handler.PostDelayed(action, 10000);
            //到这里 , 定时关闭的任务就开启完成了 , 就等待10s后关闭扫描了 ;
        }

        private void OnDeviceReConnect()
        {
            Log.Error("dana.ye->xamarin->bt", "即将进行BLE的设备的重新连接");
        }


        private void OnDeviceConnect()
        {
            Log.Error("dana.ye->xamarin->bt", "手动点击按钮来停止扫描周围的蓝牙设备");
            btAdapter.CancelDiscovery();
            /*try
            {
                #pragma warning disable CS0618 // 类型或成员已过时
                btAdapter.StopLeScan(bleScanCallback);
                #pragma warning restore CS0618 // 类型或成员已过时
            }
            catch (Exception e)
            {
                Log.Error("dana.ye->xamarin->bt", "stopLeScan时出错 : " + e.Message);
            }*/
                handler.RemoveCallbacks(action);
            if (gatt != null)
            {
                gatt.Disconnect();
                gatt.Close();
            } 
            Log.Error("dana.ye->xamarin->bt", "即将进行BLE的设备连接");
            String addr = searchedDevices[adapter.GetChooseItem()].Address;
            SingleGattService.GetInstance().Address = addr;
            BluetoothDevice device = btAdapter.GetRemoteDevice(addr);
            GattCallBack mGattCallback = new GattCallBack(this);
            SingleGattService.GetInstance().CurCallback = mGattCallback;
            if (Android.OS.Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                Log.Error("dana.ye->xamarin->bt", "SdkInt >= M");
                gatt = device.ConnectGatt(this, false, mGattCallback, BluetoothTransports.Le);
            }
            else
            {
                Log.Error("dana.ye->xamarin->bt", "SdkInt < M");
                gatt = device.ConnectGatt(this, false, mGattCallback);
            }
           
        }

        private void GetCurDeviceServices()
        {
            Log.Error("dana.ye->xamarin->bt", "当前设备的Services数量为 : " + gatt.Services.Count);
            foreach (BluetoothGattService i in gatt.Services)
            {
                Log.Error("dana.ye->xamarin->bt", "Cur Service is : " + i.Uuid);
                Log.Error("dana.ye->xamarin->bt", "Cur Characteristics is : " + i.Characteristics.Count);
            }
        }

        //-------------------------------------------生命周期相关-----------------------------------------------
        protected override void OnPause()
        {
            base.OnPause();
            //反注册BR/EDR蓝牙发现的广播(SPP协议)
            UnregisterReceiver(btReceiver);
        }

        protected override void OnResume()
        {
            base.OnResume();
            //注册BR/EDR蓝牙发现的广播(SPP协议)
            btReceiver = new BREDRReceiver(this);
            //bleScanCallback = new BLEScanCallBack(this);
            IntentFilter intentFilter = new IntentFilter();
            intentFilter.AddAction(BluetoothDevice.ActionFound);
            RegisterReceiver(btReceiver, intentFilter);
        }
    }

}

