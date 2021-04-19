using RaftImplementation.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RaftImplementation
{
    public class Follower : State
    {

        private IOrderEventRepository _orderEventRepository;
        private IOrderEventOutboxRepository _orderEventOutboxRepository;
        private TimeSpan _electionTimeOut =TimeSpan.FromSeconds(7);
        private string SchedulerStatus = string.Empty;
        private IList<Guid> candidates = new List<Guid>();

        public Follower(OrderEventProcessor orderEventProcessor): base(orderEventProcessor)
        {
            Console.WriteLine($"OrderEventProcessor :{orderEventProcessor.ProcessorId} state become Follower.");
            _orderEventRepository = new OrderEventRepository();
            _orderEventOutboxRepository = new OrderEventOutboxRepository();
            RegisterAsFollower();
        }

        private void RegisterAsFollower()
        {
            Task.Run(() => { _orderEventRepository.UpdateProcessorState(OrderEventProcessor.ProcessorId, OrderEventProcessorState.FOLLOWER); _orderEventRepository.WithdrawElection(OrderEventProcessor.ProcessorId); }).Wait();

        }
      

        private async Task<bool> CheckforElectionTimeOut()
        {
            var isElectionTimeout = false;
            var leaderHeartBeatOn = await _orderEventRepository.GetLeaderHeartBeat();
            if (leaderHeartBeatOn.HasValue)
            {
                Console.WriteLine($"LeaderHeartbeatOn:{leaderHeartBeatOn}");
                var _leaderHeartBeatDelay = DateTime.Now - leaderHeartBeatOn.Value;
                if (_leaderHeartBeatDelay > _electionTimeOut)
                {
                    Console.WriteLine($"ElectionTimeout has happened.");
                    OrderEventProcessor.State = new Candidate(OrderEventProcessor);
                  //  await OrderEventProcessor.State.ExecuteTask();
                    isElectionTimeout = true;
                }
            }
            else //no leader
            {
                Console.WriteLine($"No Leader.");
                OrderEventProcessor.State = new Candidate(OrderEventProcessor);
              //  await OrderEventProcessor.State.ExecuteTask();
                isElectionTimeout = true;
            }
            return isElectionTimeout;
        }

        private async Task ProcessEvent()
        {
            Console.WriteLine("Follower->ProcessEvent");
              var eventId = await _orderEventOutboxRepository.GetOrderEvent(OrderEventProcessor.ProcessorId);
            if (this.SchedulerStatus != "Election")
            {
                await Task.Run(() =>
                {
                   
                     Console.WriteLine($"processing events {eventId}");
                    _orderEventOutboxRepository.MarkOrderEventPublished(eventId);
                });
            }
            else
            {
                var candidates = await _orderEventRepository.GetActiveCandidates();
                var random = new Random(candidates.Count);
                var candidate = candidates[random.Next()];
                await _orderEventRepository.CastVote(candidate);
            }
        }


        public override async Task ExecuteTask()
        {
            Console.WriteLine($"Follower Id: {OrderEventProcessor.ProcessorId}-> ExecuteTask");            
            var isElectionTimeout = await CheckforElectionTimeOut();
            if(!isElectionTimeout)  await ProcessEvent();

        }
    }
}
