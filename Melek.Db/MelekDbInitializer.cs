using System.Data.Entity;

namespace Melek.Db
{
    public class MelekDbInitializer : DropCreateDatabaseIfModelChanges<MelekDbContext>
    {
        protected override void Seed(MelekDbContext context)
        {
            // seed all the things, pls
        }
    }
}