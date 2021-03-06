﻿using EveOpenApi.Api;
using EveOpenApi.Authentication;
using EveOpenApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("Eve-OpenApi.Test")]

namespace EveOpenApi.Managers
{
	internal class TokenManager : BaseManager, ITokenManager
	{
		public TokenManager(IHttpHandler client, IApiConfig config, ILogin login) : base(client, login, config)
		{
		}


		/// <summary>
		/// Add auth token to request
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public async Task AddAuthTokens(IApiRequest request)
		{
			if (Login is null && !string.IsNullOrEmpty(request.Scope))
				throw new Exception("No login provided");

			if (Login != null || Config.AlwaysIncludeAuthHeader)
				await AddAuthToken(request);
		}

		/// <summary>
		/// Add auth token to request
		/// </summary>
		/// <param name="request"></param>
		/// <param name="index">Wich request to add to</param>
		/// <returns></returns>
		async Task AddAuthToken(IApiRequest request)
		{
			if (string.IsNullOrEmpty(request.Scope) && !Config.AlwaysIncludeAuthHeader)
				return;

			if (Login is null && Config.AlwaysIncludeAuthHeader)
				AddTokenLocation(request, "");

			if (string.IsNullOrEmpty(request.User))
				throw new Exception("User cannot be null or empty, please set a default user.");

			IToken token = await Login.GetToken(request.User, (Scope)request.Scope);

			if (token is null)
				throw new Exception($"No token with scope '{request.Scope}'");

			AddTokenLocation(request, token.GetToken());
		}

		/// <summary>
		/// Add auth token to the correct location accoridng to the login config
		/// </summary>
		/// <param name="request"></param>
		/// <param name="token"></param>
		void AddTokenLocation(IApiRequest request, string token)
		{
			switch (Config.TokenLocation)
			{
				case "header":
					request.SetHeader(Config.TokenName, token);
					break;
				case "query":
					request.SetParameter(Config.TokenName, token);
					break;
				default:
					throw new Exception("Invalid token location");
			}
		}
	}
}
