﻿using EveOpenApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
[assembly: InternalsVisibleTo("Eve-OpenApi.Test")]

namespace EveOpenApi.Authentication.Managers
{
	internal class ResponseManager : IResponseManager
	{
		public string HtmlResponse { get; } = "" +
			"<html>" +
				"<body style=\"background-color: grey\">" +
					"You can close this page." +
				"</body>" +
			"</html>";

		ILoginCredentials credentials;
		IFactory<IAuthResponse> authResponseFactory;

		public ResponseManager(ILoginCredentials credentials, IFactory<IAuthResponse> authResponseFactory)
		{
			this.credentials = credentials;
			this.authResponseFactory = authResponseFactory;
		}

		public async Task<IAuthResponse> GetResponse(string authUrl, int timeout)
		{
			OpenUrl(authUrl);

			return await AwaitResponse(timeout);
		}

		public async Task<IAuthResponse> AwaitResponse(int timeout)
		{
			var listenerTask = ListenForResponse();
			await Task.WhenAny(listenerTask, Task.Delay(timeout));

			if (listenerTask.IsCompleted)
				return listenerTask.Result;
			else
				throw new TimeoutException();
		}

		async Task<IAuthResponse> ListenForResponse()
		{
			NameValueCollection parameters;
			using (HttpListener listener = new HttpListener())
			{
				listener.Prefixes.Add($"{credentials.Callback}/");
				listener.Start();

				HttpListenerContext context = await listener.GetContextAsync();
				using (Stream output = context.Response.OutputStream)
					await DisplayHtmlResponse(output);

				listener.Stop();
				parameters = HttpUtility.ParseQueryString(context.Request.Url.Query);
			}

			return authResponseFactory.Create(parameters.Get(0), parameters.Get(1));
		}

		async Task DisplayHtmlResponse(Stream outputStream)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(HtmlResponse);

			await outputStream.WriteAsync(buffer, 0, buffer.Length);
			await Task.Delay(buffer.Length); // Fix bug where page would not load on chrome :/
		}

		/// <summary>
		/// https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
		/// </summary>
		/// <param name="url"></param>
		static void OpenUrl(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				// hack because of this: https://github.com/dotnet/corefx/issues/10361
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					Process.Start("open", url);
				}
				else
				{
					throw;
				}
			}
		}
	}
}
