using System;
using System.Data.Entity;
using Melek.Db.Dtos;

namespace Melek.Db
{
    public class MelekDbInitializer : DropCreateDatabaseAlways<MelekDbContext>
    {
        protected override void Seed(MelekDbContext context)
        {
            context.Version.Add(new ApiVersionDto() { 
                ReleaseDate = DateTime.Now,
                Version = "1.0.0.0"
            });

            context.SaveChanges();
        }
    }
}