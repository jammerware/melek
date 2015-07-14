using System;
using System.Data.Entity;
using Melek.Db.Dtos;

namespace Melek.Db
{
    public class MelekDbInitializer : DropCreateDatabaseAlways<MelekDbContext>
    {
        protected async override void Seed(MelekDbContext context)
        {
            int versionCount = await context.Version.CountAsync();
            if (versionCount == 0) {
                context.Version.Add(new ApiVersionDto() {
                    Notes = "First release lol",
                    ReleaseDate = DateTime.Now,
                    Version = "1.0.0.0"
                });
            }
        }
    }
}