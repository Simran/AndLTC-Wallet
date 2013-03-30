using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace AndLTCWallet
{
	public class UserItemAdapter : ArrayAdapter<TransListView> 
	{
		
		private List<TransListView> transInfo;
		private LayoutInflater vi;      
	
		public UserItemAdapter(Context context, int textViewResourceId, List<TransListView> transInfo)
			: base(context, textViewResourceId, transInfo)
		{
			this.transInfo = transInfo;
			vi = LayoutInflater.From(context);
		}

		public override View GetView(int position, View convertView, ViewGroup parent) {
			View v = convertView;
			if (v == null) {
				LayoutInflater vi = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
				v = vi.Inflate(Resource.Drawable.listitem, null);
			}
			
			TransListView TransView = transInfo[position];
			if (TransView != null) {
				TextView Confirms = v.FindViewById<TextView>(Resource.Id.Confirms);
				TextView Amount = v.FindViewById<TextView>(Resource.Id.Amount);
				TextView TransactionID = v.FindViewById<TextView>(Resource.Id.TransactionID);
				TextView Fee = v.FindViewById<TextView>(Resource.Id.Fee);
				
				if (Confirms != null) {
					Confirms.Text = TransView.Confirms;
				}

				if (Amount != null) {
					Amount.Text = string.Format("Amount: {0}", TransView.Amount);
				}
				
				if(TransactionID != null) {
					TransactionID.Text = TransView.TransactionID;
				}
				
				if(Fee != null) {
					Fee.Text = string.Format("Fee: {0}", TransView.Fee);
				}
			}
			return v;
		}
	}
}

