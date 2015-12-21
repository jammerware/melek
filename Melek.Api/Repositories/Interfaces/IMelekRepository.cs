using System.Collections.Generic;
using System.Threading.Tasks;
using Melek;

namespace Melek.Api.Repositories
{
    public interface IMelekRepository
    {
        string GetAllData();
        ICard GetCardByMultiverseId(string multiverseId);
        ICard GetCardByName(string name);
        string GetVersion();
        IReadOnlyList<Card> Search(string search);
        Task SetDataSource(string path);
    }
}