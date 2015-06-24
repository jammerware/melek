using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Melek.Models;

namespace Melek.Db
{
    public class MelekDbContext : DbContext
    {
        public MelekDbContext() : base("MelekDbContext")
        {
            Database.SetInitializer<MelekDbContext>(new MelekDbInitializer());
        }

        public DbSet<ICard> Cards { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // have to call the base implementation here or stuff goes real wrong
            base.OnModelCreating(modelBuilder);

            // only froobs pluralize tables
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}