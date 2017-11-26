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
using Java.Lang;
using Android.Bluetooth;
using Android.Util;

namespace BTDemo.ui.adapters
{
    class DevicesAdapter : BaseAdapter
    {
        List<BluetoothDevice> devices;
        Context ctx;
        DevicesHolder holder;
        int curPos = -1;


        public DevicesAdapter(Context ctx, List<BluetoothDevice> devices)
        {
            this.devices = devices;
            this.ctx = ctx;
            holder = new DevicesHolder();
        }


        public override int Count
        {
            get
            {
                return devices == null ? 0 : devices.Count;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null)
            {
                convertView = View.Inflate(ctx, Resource.Layout.item_device, null);
                holder.name = convertView.FindViewById<TextView>(Resource.Id.device_name);
                holder.addr = convertView.FindViewById<TextView>(Resource.Id.device_addr);
                holder.type = convertView.FindViewById<TextView>(Resource.Id.device_type);
            
                holder.bondState = convertView.FindViewById<TextView>(Resource.Id.device_bond_state);
                holder.chooseState = convertView.FindViewById<TextView>(Resource.Id.device_choose_state);
                convertView.SetTag(Resource.Id.device_name, holder.name);
                convertView.SetTag(Resource.Id.device_addr, holder.addr);
                convertView.SetTag(Resource.Id.device_type, holder.type);
                convertView.SetTag(Resource.Id.device_bond_state, holder.bondState);
                convertView.SetTag(Resource.Id.device_choose_state, holder.chooseState);
            }
            else
            {
                holder.name = (TextView)convertView.GetTag(Resource.Id.device_name);
                holder.addr = (TextView)convertView.GetTag(Resource.Id.device_addr);
                holder.type = (TextView)convertView.GetTag(Resource.Id.device_type);
                holder.bondState = (TextView)convertView.GetTag(Resource.Id.device_bond_state);
                holder.chooseState = (TextView)convertView.GetTag(Resource.Id.device_choose_state);
            }

            BluetoothDevice device = devices[position];

            holder.name.Text = device.Name;
            holder.addr.Text = device.Address;
            holder.type.Text = device.Type + "";
            holder.bondState.Text = device.BondState + "";

            if (position == curPos)
            {
                holder.chooseState.Text = "当前已被选中";
            }
            else
            {
                holder.chooseState.Text = "当前未被选中";
            }

            return convertView;
        }

        private class DevicesHolder
        {
            public TextView name;
            public TextView addr;
            public TextView type;
            public TextView bondState;
            public TextView chooseState;
        }

        public void SetChooseItem(int position)
        {
            curPos = position;
        }

        public int GetChooseItem()
        {
            return curPos;
        }
    }
}