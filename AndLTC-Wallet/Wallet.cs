using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace AndLTCWallet
{
	public class Wallet
	{

		WebClient walletClient = new WebClientEx();

		public string Key;
		public string Address;
		public string Balance;
		public MatchCollection LastTransactions;

		string walletPage;
		WalletSettings walletSettings = new WalletSettings();

		public Wallet ()
		{
			if (string.IsNullOrEmpty(walletSettings.VaultKey))
			{
				newWallet();
			}
			else
			{
				Key = walletSettings.VaultKey;
				walletSettings.setSettings("WALLET", "VAULTKEY", Key);
				refreshWallet();
			}
		}
		
		public void refreshWallet()
		{
			walletPage = getVault(Key);
			Address = getAddress(walletPage);
			Balance = getBalance(walletPage);
			LastTransactions = lastTransactions(walletPage);
		}

		public void newWallet()
		{
			walletClient.Dispose();
			walletClient = new WebClientEx();

			Key = getVaultKey();
			//Console.WriteLine("newWallet() called directly, new key {0}", getVaultKey());
			walletSettings.setSettings("WALLET", "VAULTKEY", Key);
			walletPage = getVault(Key);
			Address = getAddress(walletPage);
			Balance = getBalance(walletPage);
			LastTransactions = lastTransactions(walletPage);
		}

		private string getBalance(string vaultPage)
		{
			return Regex.Match (vaultPage, "is</h1><br><h1><font style='font-size: 100px';>(\\S+)</font><font style='font-size: 50px';>LTC").Groups[1].Value;
		}

		private string getVaultKey()
		{
			NameValueCollection newAddress = new NameValueCollection
			{
				{ "iwantaddress", "true" }
			};
			walletClient.UploadValues("http://wallet.coinpool.net/getaddress.php", newAddress);
			string vaultLocation = walletClient.ResponseHeaders["Location"].Split('=')[1];
			//walletClient.Dispose();

			return vaultLocation;
		}

		private string getAddress(string vaultPage)
		{
			return Regex.Match(vaultPage, "(L\\S+)<small>").Groups[1].Value;
		}

		private string getVault(string vaultKey)
		{
			NameValueCollection newAddress = new NameValueCollection
			{
				{ "password", "test" }, //dun need a password
				{ "submit", "Login"}
			};

			string vaultSource = Encoding.Default.GetString(walletClient.UploadValues(string.Format("http://wallet.coinpool.net/vault?key={0}", vaultKey), newAddress));
			//walletClient.Dispose();
			return vaultSource;
		}

		public string sendLTC(string ltcAddress, string ltcAmount)
		{
			NameValueCollection sendArgs = new NameValueCollection
			{
				{ "address", ltcAddress },
				{ "amount", ltcAmount }
			};

			string walletResponse = Encoding.Default.GetString(walletClient.UploadValues(string.Format("http://wallet.coinpool.net/vault?key={0}", Key), sendArgs));
			//walletClient.Dispose();

			if (walletResponse.Contains("Invalid litecoin address"))
			{
				walletResponse = "Invalid Litecoin Address!";
			}
			if (walletResponse.Contains ("Not enough funds"))
			{
				walletResponse = "Not Enough Funds!";
			}
			if (walletResponse.Contains ("Successfully"))
			{
				walletResponse = "LTC Successfully Sent!";
			}
			return walletResponse;

		}

		private MatchCollection lastTransactions(string vaultPage)
		{
			Regex transInfo = new Regex("<tr><td>(\\d+)</td><td><input type='text' value='(\\S+)' style='margin: 0px;'/></td><td>(-?\\d+\\.?\\d*)</td><td>(-?\\d+\\.?\\d*)</td></tr>");
			return transInfo.Matches(vaultPage);
		}
	}
}

