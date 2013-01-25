using ArtMoney.Dataload;
using ArtMoney.Model;
using ArtMoney.Persistence;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ArtMoney
{
	public class Program
	{
		private const string _urlFormat =
			"kunststoette/tildelinger/?tx_lftilskudsbase_pi5%5Barea%5D={0}&tx_lftilskudsbase_pi5%5Bstype%5D=0&tx_lftilskudsbase_pi5%5Byear%5D={1}&tx_lftilskudsbase_pi5%5Bsword%5D=&tx_lftilskudsbase_pi5%5Border%5D=dato%20desc&tx_lftilskudsbase_pi5%5Bpage%5D={2}";

		private static readonly Dictionary<int, string> _areaMap = new Dictionary<int, string>
		{
			{ 1, "Arkitektur" },
			{ 2, "Billedkunst" },
			{ 4, "Film" },
			{ 9, "Internationalt" },
			{ 5, "Kunsthåndværk og design" },
			{ 6, "Litteratur" },
			{ 7, "Musik" },
			{ 8, "Scenekunst" },
		};

		static void Main(string[] args)
		{
			Enumerable.Range(2010, 3).AsParallel().WithDegreeOfParallelism(1).ForAll(year =>
				{
					foreach (var area in _areaMap.Keys)
					{
						GetYear(year, area);
					}
				}
			);
		}

		private static void GetYear(int year, int area)
		{
			var firstPage = GetContent(string.Format(_urlFormat, area, year, 1));
			var lastPageString = firstPage.DocumentNode.SelectNodes("//*[@id='c45806']/div/div/a")
					.Last().InnerText;
			if (!string.IsNullOrEmpty(lastPageString))
			{
				var pageCount = int.Parse(lastPageString);

				Enumerable.Range(1, pageCount - 1)
					.AsParallel().ForAll(pageNumber => FetchPage(year, area, pageNumber));
			}
		}

		private static void FetchPage(int year, int area, int pageNumber)
		{
			using (var context = new Context())
			{
				var page = GetContent(string.Format(_urlFormat, area, year, pageNumber));
				var rows = page.DocumentNode.SelectNodes("//div[@class='list']/table/tbody/tr");
				foreach (var row in rows)
				{
					var artistName = row.SelectSingleNode("td/h2").InnerText;
					var projectName = row.SelectSingleNode("td[1]/p").InnerText;
					var type = row.SelectSingleNode("td[2]").ChildNodes.First().InnerText.Trim();
					var giver = row.SelectSingleNode("td[2]/p").InnerText.Replace("Tildelt af: ", "");
					decimal amount;
					decimal.TryParse(row.SelectSingleNode("td[3]").InnerText, NumberStyles.Any, CultureInfo.GetCultureInfo("da"), out amount);
					var date = row.SelectSingleNode("td[4]").InnerText;

					context.Grants.Add(new Grant
					{
						Amount = amount,
						Area = _areaMap[area],
						ArtistName = artistName,
						Date = date,
						Giver = giver,
						ProjectName = projectName,
						Type = type,
						Year = year,
					});
				}
				context.SaveChanges();
				Console.WriteLine("Retrieved {2}, {0} page {1}", year, pageNumber, _areaMap[area]);
			}
		}

		private static HtmlDocument GetContent(string url)
		{
			using (var client = new HttpClientWrapper())
			{
				var contentString = client.HttpClient.GetAsync(url).Result.
					Content.ReadAsStringAsync().Result;

				var document = new HtmlDocument();
				document.LoadHtml(contentString);
				return document;
			}
		}
	}
}
