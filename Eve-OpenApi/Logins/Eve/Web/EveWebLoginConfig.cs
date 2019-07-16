﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EveOpenApi.Eve
{
	public class EveWebLoginConfig : IEveWebLoginConfig
	{
		public string ClientID { get; set; }

		public string ClientSecret { get; set; }

		public string Callback { get; set; }

		public EveWebLoginConfig(string clientID, string clientSecret, string callback)
		{
			ClientID = clientID;
			ClientSecret = clientSecret;
			Callback = callback;
		}
	}
}
