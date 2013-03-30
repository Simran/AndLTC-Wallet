using System;
using System.IO;
using System.Net;

using IniParser;

namespace AndLTCWallet
{
	public class WalletSettings
	{

		public string VaultKey;

		FileIniDataParser settingsInfo = new FileIniDataParser();
		IniData settingsValues;

		WebClient settingsClient = new WebClient();
		string settingsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + Java.IO.File.Separator + "Settings.ini";

		public WalletSettings ()
		{
			if (File.Exists(settingsDir))
			{
				settingsValues = settingsInfo.LoadFile(settingsDir);
				VaultKey = getVaultKey();
			}
			else
			{
				settingsClient.DownloadFile(new Uri("http://litecoinforums.org/android/andltc-wallet/Settings.ini"), settingsDir);
				settingsValues = settingsInfo.LoadFile(settingsDir);
			}
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

