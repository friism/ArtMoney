using ArtMoney.Model;
using System.Data.Entity;

namespace ArtMoney.Persistence
{
	public class Context : DbContext
	{
		public DbSet<Grant> Grants { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, Configuration>());
		}
	}
}
