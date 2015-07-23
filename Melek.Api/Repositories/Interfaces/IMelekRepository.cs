using System.Collections.Generic;
using Melek.Domain;

namespace Melek.Api.Repositories.Interfaces
{
    public interface IMelekRepository
    {
        string GetAllData();
        ICard GetCardByMultiverseId(string multiverseId);
        ICard GetCardByName(string name);
        string GetVersion();
        IReadOnlyList<Card> Search(string search);
    }
}