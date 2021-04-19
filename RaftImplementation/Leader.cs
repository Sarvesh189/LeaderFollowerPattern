using RaftImplementation.Repository;
using System;
using System.Threading.Tasks;

namespace RaftImplementation
{
    public class Leader: State
    {
        
        public IOrderEventRepository _orderEventRepository;
        public IOrderEventOutboxRepository _orderEventOutboxRepository;

        public Leader(OrderEventProcessor orderEventProcessor) : base(orderEventProcessor) {
            Console.WriteLine($"OrderEventScheduler: {OrderEventProcessor.ProcessorId} become Leader.");
            _orderEventRepository = new OrderEventRepository();
            _orderEventOutboxRepository = new OrderEventOutboxRepository();
            RegisterAsLeader();
        }

        private void RegisterAsLeader()
        {
            Task.Run(() => { _orderEventRepository.UpdateProcessorState(OrderEventProcessor.ProcessorId, OrderEventProcessorState.LEADER); _orderEventRepository.WithdrawElection(OrderEventProcessor.ProcessorId); }).Wait();
        }

        private async Task AssignOrderEventToSchedulers()
        {
            Console.WriteLine($"OrderEventScheduler Id: {OrderEventProcessor.ProcessorId} as Leader-> AssignOrderEventToSchedulers");
            var orderEvents = await _orderEventOutboxRepository.GetOrderEvents();
            var processors = await _orderEventRepository.GetProcessors();
            foreach (var orderEvent in orderEvents)
            { 
                var index = orderEvent % processors.Count;
                Console.WriteLine($"OrderEventId:{orderEvent} is assigned with processor:{processors[index]}");
                await _orderEventOutboxRepository.SetProcessor(orderEvent, processors[index]);

            }

        }

        private async Task ProcessEvent()
        {
            Console.WriteLine("Leader->ProcessEvent");
            var eventId = await _orderEventOutboxRepository.GetOrderEvent(OrderEventProcessor.ProcessorId);
            var status = await _orderEventRepository.GetStatus(OrderEventProcessor.ProcessorId);
            if (status != "Election")
            {
                await Task.Run(() =>
                {

                    Console.WriteLine($"Leader processing orderEventId:{eventId}");
                   
                    _orderEventOutboxRepository.MarkOrderEventPublished(eventId);
                });
            }
            else
            {
                Console.WriteLine("Scheduler is transitioned to Follower from Leader.");
                OrderEventProcessor.State = new Follower(OrderEventProcessor);
            }
        }

        public override async Task ExecuteTask()
        {
            Console.WriteLine($"OrderEventScheduler Id: {OrderEventProcessor.ProcessorId} as Leader-> ExecuteTask");
            await AssignOrderEventToSchedulers();
            await ProcessEvent();

        }
    }
}
