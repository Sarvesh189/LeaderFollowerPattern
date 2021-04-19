using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RaftImplementation.Repository
{
   public interface IOrderEventRepository
    {
        Task WithdrawElection(Guid instanceId);
        Task UpdateOrderEvent(int eventId, OrderEventStatus orderEventStatus);
        Task<IList<int>> GetOrderEvent(Guid instanceId);

        Task CastVote(Guid candidateId);

        Task<string> GetStatus(Guid instanceId);
        Task AssignOrderEventToFollower();
        Task UpdateHeartBeat(Guid Id);

        Task<IList<Guid>> GetActiveCandidates();
        Task<DateTime?> GetLeaderHeartBeat();
        Task UpdateProcessorState(Guid instanceId, OrderEventProcessorState state);

        Task DeclareElection();

        Task<IList<Guid>> GetProcessors();

        Task<Guid> GetCandidateWithMaxVote();
        Task RegisterOrderEventScheduler(Guid processorId);

    }
}
