using RaftImplementation.Repository;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RaftImplementation
{
    public class LeaderSelection : State
    {
        public IOrderEventRepository _orderEventRepository;
        public IOrderEventOutboxRepository _orderEventOutboxRepository;

        public LeaderSelection(OrderEventProcessor orderEventProcessor) : base(orderEventProcessor)
        {
            Console.WriteLine($"OrderEventProcessor Id: {orderEventProcessor.ProcessorId} become LeaderSelection.");
            _orderEventRepository = new OrderEventRepository();
            _orderEventOutboxRepository = new OrderEventOutboxRepository();
           // RegisterAsCandidate();
        }

        private async Task BecomeLeaderOrFollower()
        {
            Console.WriteLine($"OrderEventProcessor Id: {OrderEventProcessor.ProcessorId} -> BecomeLeaderOrFollower.");
            var leaderId = await _orderEventRepository.GetCandidateWithMaxVote();
            if (Equals(leaderId, OrderEventProcessor.ProcessorId))
            {
                Console.WriteLine($"OrderScheduler {OrderEventProcessor.ProcessorId} transition from Candidate to Leader.");
                this.OrderEventProcessor.State = new Leader(OrderEventProcessor);
               // await _orderEventRepository.WithdrawElection();
                //  await this.OrderEventProcessor.State.ExecuteTask();
            }
            else
            {
                Console.WriteLine($"OrderScheduler {OrderEventProcessor.ProcessorId} transition from Candidate to Follower.");
                this.OrderEventProcessor.State = new Follower(OrderEventProcessor);
            }

        }
        public override async Task ExecuteTask()
        {
            Console.WriteLine("LeaderSelection->ExecuteTask");
            await BecomeLeaderOrFollower();
        }
    }
}
