using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;

namespace BTDemo.ui.adapters
{
    class ServicesAdapter : BaseAdapter
    {

        List<BluetoothGattService> services;
        Context ctx;
        DevicesHolder holder;
        int curPos = -1;

        public ServicesAdapter(Context ctx, List<BluetoothGattService> services)
        {
            this.services = services;
            this.ctx = ctx;
            holder = new DevicesHolder();
        }

        public override int Count
        {
            get
            {
                return services == null ? 0 : services.Count;
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
                convertView = View.Inflate(ctx, Resource.Layout.item_service, null);
                holder.serviceUUID = convertView.FindViewById<TextView>(Resource.Id.service_uuid);
                holder.characteristicsSize = convertView.FindViewById<TextView>(Resource.Id.service_characteristics);
                holder.type = convertView.FindViewById<TextView>(Resource.Id.service_type);
                holder.chooseState = convertView.FindViewById<TextView>(Resource.Id.choose_state);
                convertView.SetTag(Resource.Id.service_uuid, holder.serviceUUID);
                convertView.SetTag(Resource.Id.service_characteristics, holder.characteristicsSize);
                convertView.SetTag(Resource.Id.service_type, holder.type);
                convertView.SetTag(Resource.Id.choose_state, holder.chooseState);
            }
            else
            {
                holder.serviceUUID = (TextView)convertView.GetTag(Resource.Id.service_uuid);
                holder.characteristicsSize = (TextView)convertView.GetTag(Resource.Id.service_characteristics);
                holder.type = (TextView)convertView.GetTag(Resource.Id.service_type);
                holder.chooseState = (TextView)convertView.GetTag(Resource.Id.choose_state);
            }

            BluetoothGattService service = services[position];

            holder.serviceUUID.Text = service.Uuid + "";
            int count = service.Characteristics == null ? -1 : service.Characteristics.Count;
            holder.characteristicsSize.Text = "当前特征值数量为 : " + count;
            holder.type.Text = "当前服务类型为 : " + service.Type;

            if (position == curPos)
            {
                holder.chooseState.Text = "当前选中状态 : 已选中";
            }
            else
            {
                holder.chooseState.Text = "当前选中状态 : 未选中";
            }

            return convertView;
        }

        private class DevicesHolder
        {
            public TextView serviceUUID;
            public TextView characteristicsSize;
            public TextView type;
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