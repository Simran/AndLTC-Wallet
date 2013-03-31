using System;
using System.IO;
using System.Net;
using System.Threading;

using IniParser;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace AndLTCWallet
{
	public class WalletSettings
	{

		public string VaultKey;

		FileIniDataParser settingsInfo = new FileIniDataParser();
		IniData settingsValues;
		
		string settingsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + Java.IO.File.Separator + "Settings.ini";

		public WalletSettings ()
		{
			settingsValues = settingsInfo.LoadFile(settingsDir);
			VaultKey = getVaultKey();
		}

		public void setSettings(string Section, string Key, string Value)
		{
			settingsValues[Section][Key] = Value;
			settingsInfo.SaveFile(settingsDir, settingsValues);
		}
		
		private string getVaultKey()
		{
			return settingsValues["WALLET"]["VAULTKEY"];
		}
	}
}

