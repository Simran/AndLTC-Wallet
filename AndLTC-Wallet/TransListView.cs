using System;

namespace AndLTCWallet
{
	public class TransListView
	{
		public string Confirms;
		public string Amount;
		public string TransactionID;
		public string Fee;

		public TransListView (string Confirms, string Amount, string TransactionID, string Fee)
		{
			this.Confirms = Confirms;
			this.Amount = Amount;
			this.TransactionID = TransactionID;
			this.Fee = Fee;
		}
	}
}

