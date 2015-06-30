using System;
using System.Data.Entity;

namespace Melek.Db
{
    public class MelekDbInitializer : DropCreateDatabaseIfModelChanges<MelekDbContext>
    {
        protected override void Seed(MelekDbContext context)
        {
            context.Version.Add(new Version("1.0.0.0"));
        }
    }
}