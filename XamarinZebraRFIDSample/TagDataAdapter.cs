using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.Tabs;
using System;
using System.Collections.Generic;
using XamarinZebraRFIDSample.Model;
using static AndroidX.RecyclerView.Widget.RecyclerView;

namespace XamarinZebraRFIDSample
{
    internal class TagDataAdapter : ArrayAdapter<TagReadData>
    {
        private readonly Context context;
        private readonly List<TagReadData> items;

        public TagDataAdapter(Context context, List<TagReadData> items) : base(context, 1, items)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.items = items ?? throw new ArgumentNullException(nameof(items));
        }

        //public override TagReadData this[int position] => items[position];
        /*
        public override long GetItemId(int position)
        {
            return position;
        }
        */
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            TagDataAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as TagDataAdapterViewHolder;

            if (holder == null)
            {
                holder = new TagDataAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.tag_data_view, parent, false);
                holder.EPC = view.FindViewById<TextView>(Resource.Id.epcTextView);
                holder.TID = view.FindViewById<TextView>(Resource.Id.tidTextView);
                holder.USER = view.FindViewById<TextView>(Resource.Id.userTextView);
                holder.TitleEPC = view.FindViewById<TextView>(Resource.Id.epcTitle);
                holder.TitleTID = view.FindViewById<TextView>(Resource.Id.tidTitle);
                holder.TitleUSER = view.FindViewById<TextView>(Resource.Id.userTitle);
                view.Tag = holder;
            }

            var tagReadData = items[position];

            //fill in your items
            if (!string.IsNullOrEmpty(tagReadData.EPC))
            {
                holder.EPC.Text = tagReadData.EPC;
                holder.EPC.Visibility = ViewStates.Visible;
                holder.TitleEPC.Visibility = ViewStates.Visible;
            }
            else
            {
                holder.EPC.Visibility = ViewStates.Gone;
                holder.TitleEPC.Visibility = ViewStates.Gone;
            }

            if (!string.IsNullOrEmpty(tagReadData.TID))
            {
                holder.TID.Text = tagReadData.TID;
                holder.TID.Visibility = ViewStates.Visible;
                holder.TitleTID.Visibility = ViewStates.Visible;
            }
            else
            {
                holder.TID.Visibility = ViewStates.Gone;
                holder.TitleTID.Visibility = ViewStates.Gone;
            }


            if (!string.IsNullOrEmpty(tagReadData.UMB))
            {
                holder.USER.Text = tagReadData.UMB;
                holder.USER.Visibility = ViewStates.Visible;
                holder.TitleUSER.Visibility = ViewStates.Visible;
            }
            else
            {
                holder.USER.Visibility = ViewStates.Gone;
                holder.TitleUSER.Visibility = ViewStates.Gone;
            }


            return view;
        }

        /*

        internal void Insert(TagReadData tagReadData)
        {
            items.Insert(0, tagReadData);
        }
        
        public override int Count
        {
            get
            {
                return items.Count;
            }
        }*/

    }

    internal class TagDataAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView EPC { get; set; }
        public TextView TID { get; set; }
        public TextView USER { get; set; }
        public TextView TitleEPC { get; set; }
        public TextView TitleTID { get; set; }
        public TextView TitleUSER { get; set; }
    }
}