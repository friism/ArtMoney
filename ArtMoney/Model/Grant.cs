using System;

namespace ArtMoney.Model
{
	public class Grant
	{
		public int Id { get; set; }
		public int Year { get; set; }
		public string Date { get; set; }
		public string ArtistName { get; set; }
		public string ProjectName { get; set; }
		public string Type { get; set; }
		public string Giver { get; set; }
		public decimal? Amount { get; set; }
		public string Area { get; set; }
	}
}
