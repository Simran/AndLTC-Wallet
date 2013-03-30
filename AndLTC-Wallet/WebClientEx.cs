using System;
using System.Net;

namespace AndLTCWallet
{
	public class WebClientEx : WebClient
	{
		private CookieContainer cookies;
	
		public WebClientEx()
		{
			ResetCookies();
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest r = base.GetWebRequest(address);
			if (r is HttpWebRequest)
			{
				(r as HttpWebRequest).AllowAutoRedirect = false;
				(r as HttpWebRequest).CookieContainer = cookies;
			}
			return r;
		}

		public void ResetCookies()
		{
			cookies = new CookieContainer();
		}
	}
}

