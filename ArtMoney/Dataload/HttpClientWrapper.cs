using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;

namespace ArtMoney.Dataload
{
	public class HttpClientWrapper : IDisposable
	{
		private const string _userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11";

		private static ConcurrentStack<HttpClient> _httpClientStack = new ConcurrentStack<HttpClient>();

		private readonly Uri _baseAddress = new Uri("http://www.kunst.dk/");
		private HttpClient _httpClient;

		public HttpClientWrapper()
		{
			_httpClientStack.TryPop(out _httpClient);

			if (_httpClient == null)
			{
				var clientHandler = new HttpClientHandler
				{
					UseCookies = false,
					AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
				};
				_httpClient = new HttpClient(clientHandler)
				{
					BaseAddress = _baseAddress,
				};
				_httpClient.DefaultRequestHeaders.Add("user-agent", _userAgent);
			}
		}

		public HttpClient HttpClient
		{
			get { return _httpClient; }
		}

		void IDisposable.Dispose()
		{
			_httpClientStack.Push(_httpClient);
		}
	}
}
