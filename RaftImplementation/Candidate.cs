using RaftImplementation.Repository;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RaftImplementation
{
    public class Candidate : State
    {
        public IOrderEventRepository _orderEventRepository;
        public IOrderEventOutboxRepository _orderEventOutboxRepository;
        public Candidate(OrderEventProcessor orderEventProcessor) : base(orderEventProcessor)
        {
            Console.WriteLine($"OrderEventProcessor Id: {orderEventProcessor.ProcessorId} become Candidate.");
            _orderEventRepository = new OrderEventRepository();
            _orderEventOutboxRepository = new OrderEventOutboxRepository();
            RegisterAsCandidate();
        }

        private void RegisterAsCandidate()
        {
            Task.Run(() => _orderEventRepository.UpdateProcessorState(OrderEventProcessor.ProcessorId, OrderEventProcessorState.CANDIDATE)).Wait();
        }
        private async Task SendElectionMessage()
        {      
                await _orderEventRepository.DeclareElection();
                await _orderEventRepository.CastVote(OrderEventProcessor.ProcessorId);
                OrderEventProcessor.State = new LeaderSelection(OrderEventProcessor);             
        }

       

        public override async Task ExecuteTask() 
        {
            Console.WriteLine($"OrderEventScheduler Id: {OrderEventProcessor.ProcessorId}: Candidate state-> ExecuteTask");
            await SendElectionMessage();            
          
        }

    }
}
