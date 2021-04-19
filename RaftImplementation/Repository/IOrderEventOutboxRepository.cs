using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RaftImplementation.Repository
{
   public interface IOrderEventOutboxRepository
    {
        Task<IList<int>> GetOrderEvents();
        Task SetProcessor(int orderEventId, Guid processorId);
        Task<int> GetOrderEvent(Guid instanceId);
        Task MarkOrderEventPublished(int orderEventId);

    }
}
