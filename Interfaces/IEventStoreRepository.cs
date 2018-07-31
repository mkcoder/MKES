using System.Collections.Generic;
using System.Threading.Tasks;
using MKES.Model;

namespace MKES.Interfaces
{
    public interface IEventStoreRepository
    {
        Task Commit(List<Event> uncommitedChanges);
    }
}