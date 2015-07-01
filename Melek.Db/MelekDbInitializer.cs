using System.Data.Entity;

namespace Melek.Db
{
    public class MelekDbInitializer : DropCreateDatabaseAlways<MelekDbContext>
    {
        protected override void Seed(MelekDbContext context)
        {
            // seed here ok?
        }
    }
}