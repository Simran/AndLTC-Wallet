using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ZXing;
using ZXing.Mobile;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace AndLTCWallet
{
	[Activity (Label = "AndLTC-Wallet", MainLauncher = true)]
	public class Activity1 : Activity
	{

		Wallet ltcWallet;
		ProgressDialog dialog;
		MobileBarcodeScanner scanner;

		string settingsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + Java.IO.File.Separator + "Settings.ini";
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			loadingWork();
			scanner = new MobileBarcodeScanner(this);
			new Thread(delegate() 
			{
				checkSettingsFile();
				ltcWallet = new Wallet();
				ThreadPool.QueueUserWorkItem(o => setUI());
			}).Start();
		}

		public void checkSettingsFile()
		{
			if (!File.Exists(settingsDir))
			{
				StreamReader settingsStream = new StreamReader(Assets.Open ("Settings.ini"));
				string settingsString = settingsStream.ReadToEnd();
				File.WriteAllText(settingsDir, settingsString);
			}
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.ActionItems, menu);
			return true;
		}

		public void loadingWork()
		{
			dialog = new ProgressDialog(this);
			dialog.SetMessage("Loading wallet..");
			dialog.SetCancelable(false);
			dialog.SetProgressStyle(ProgressDialogStyle.Spinner);
			dialog.Show();
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.menu_refresh:
				{
					loadingWork();
					new Thread(delegate() 
				    {
						ltcWallet.refreshWallet();
						setUI();
					}).Start();
					break;
				}
				case Resource.Id.menu_newwallet:
				{
				newWalletCheck("Are you sure you want a new wallet?", userOption =>
					{
						switch (userOption)
						{
							case userNewWallet.YES:
							{
								loadingWork();
								new Thread(delegate() {
									ltcWallet.newWallet();
									RunOnUiThread(delegate() {
									ListView transView = FindViewById<ListView>(Resource.Id.listView1);
									transView.SetAdapter(null);
									setUI(); });
									}).Start();
								break;
							}
							case userNewWallet.PREVIOUSKEY:
							{
								previousVaultKey();
								break;
							}
						}
					});
					break;
				}
			}
			return true;
		}

		public enum userNewWallet
		{
			YES = 0, 
			NO = 1,
			PREVIOUSKEY = 2
		}

		public void newWalletCheck(string alertMessage, Action<userNewWallet> callback)
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetTitle(Android.Resource.String.DialogAlertTitle);
			builder.SetIcon(Android.Resource.Drawable.IcDialogAlert);
			builder.SetMessage(alertMessage);
			builder.SetPositiveButton("Yes", (sender, e) =>
			                          {
				callback(userNewWallet.YES);
			});
			builder.SetNegativeButton("No", (sender, e) =>
			                          {
				callback(userNewWallet.NO);
			});
			builder.SetNeutralButton("Key", (sender, e) =>
			                          {
				callback(userNewWallet.PREVIOUSKEY);
			});
			builder.Show();
		}

		public void previousVaultKey()
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			
			builder.SetTitle(Android.Resource.String.DialogAlertTitle);
			builder.SetIcon(Android.Resource.Drawable.IcDialogAlert);
			builder.SetMessage("Please enter your vault key");
			
			// Set an EditText view to get user input 
			EditText input = new EditText(this);
			builder.SetView(input);
			
			builder.SetPositiveButton("Ok", (sender, e) =>
			                          {
					if (string.IsNullOrEmpty(input.Text))
					{
						RunOnUiThread(() => Toast.MakeText(this, "Adding vault key cancelled!", ToastLength.Short).Show());
					} 
					else
					{
					RunOnUiThread(delegate() {
						loadingWork();
						ltcWallet.usePreviousWallet(input.Text);
						ListView transView = FindViewById<ListView>(Resource.Id.listView1);
						transView.SetAdapter(null);
						setUI();
						});
					}
			});
			builder.SetNegativeButton("No", (sender, e) =>
			                          {
				Toast.MakeText(this, "Adding vault key cancelled!", ToastLength.Short).Show();
			});
			builder.Show();
		}

		public void sendLTC(string ltcSendAddress, string ltcAmount)
		{
			var amountCheck = Regex.Match(ltcAmount, "(\\d+\\.?\\d*)");
			if (!amountCheck.Success)
			{
				Toast.MakeText(this, "Not Valid LTC Amount!", ToastLength.Short).Show();
				return;
			}
			string sentResponse = ltcWallet.sendLTC(ltcSendAddress, ltcAmount);
			Toast.MakeText(this, sentResponse, ToastLength.Short).Show();
		}

		public void setUI()
		{
			RunOnUiThread(delegate {
				TextView ltcAddress = FindViewById<TextView>(Resource.Id.textView7);
				TextView ltcVaultKey = FindViewById<TextView>(Resource.Id.textView8);
				TextView ltcBalance = FindViewById<TextView>(Resource.Id.textView5);
				
				Button ltcSend = FindViewById<Button>(Resource.Id.button1);
				EditText ltcAmount = FindViewById<EditText>(Resource.Id.editText2);
				EditText ltcSendAddress = FindViewById<EditText>(Resource.Id.editText1);

				ltcSend.Click += delegate
				{
					sendLTC(ltcSendAddress.Text, ltcAmount.Text);
				};

				ImageButton qrScanner = FindViewById<ImageButton>(Resource.Id.qrCode);

				qrScanner.Click += delegate 
				{
					scanQR();
				};

				ltcAddress.Text = string.Format("{0}\n", ltcWallet.Address);
				ltcVaultKey.Text = ltcWallet.Key;
				ltcBalance.Text = string.Format("Balance: {0} LTC", ltcWallet.Balance);

				List<TransListView> transInfo = new List<TransListView>();
				foreach (Match transItem in ltcWallet.LastTransactions)
				{
					TransListView info = new TransListView(transItem.Groups[1].Value, transItem.Groups[3].Value, transItem.Groups[2].Value, transItem.Groups[4].Value);
					transInfo.Add (info);

					ListView transView = FindViewById<ListView>(Resource.Id.listView1);
					transView.SetAdapter(new UserItemAdapter(this, Android.Resource.Layout.SimpleListItem1, transInfo));
				}
			});
			dialog.Dismiss();
		}

		public void scanQR()
		{
			EditText ltcSendAddress = FindViewById<EditText>(Resource.Id.editText1);

			scanner.UseCustomOverlay = false;

			scanner.TopText = "Hold the camera up to the barcode\nAbout 6 inches away";
			scanner.BottomText = "Wait for the barcode to automatically scan!";

			scanner.Scan().ContinueWith(t => {   
				if (t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
					RunOnUiThread(() => ltcSendAddress.Text = t.Result.Text);
			});
		}
	}
}


