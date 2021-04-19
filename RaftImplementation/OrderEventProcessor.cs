using RaftImplementation.Repository;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace RaftImplementation
{

    public class OrderEventProcessor : IOrderEventProcessor
    {
        private Timer _timer;
        private IOrderEventRepository _orderEventRepository;


        public Guid ProcessorId { get; private set; }       
        public State State { get; set; }

       

        public OrderEventProcessor()
        {
            ProcessorId = Guid.NewGuid();
            _orderEventRepository = new OrderEventRepository();
            _timer = new Timer();
            Initialize();
        }

       

        #region public method
        public void Start()
        {
            _timer.Interval =5000;
            _timer.Enabled = true;
            _timer.AutoReset=true;
            _timer.Elapsed += (sender, e) =>
            {
                
                Task.Run(()=>ExecuteTask()).Wait();
               
            };
            _timer.Start();
        }
        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }

       
        public async Task ExecuteTask()
        {
            
            Console.WriteLine("OrderEvenProcessor executeTask method started");
            await UpdateHeartBeat();
            if (State != null) await State.ExecuteTask();
            Console.WriteLine("OrderEvenProcessor executeTask method end");
            //}
        }
        #endregion

        #region private methods
        private async Task UpdateHeartBeat()
        {
           // Console.WriteLine("Updating heartBeat");
            await _orderEventRepository.UpdateHeartBeat(ProcessorId);
        }
        private void Initialize()
        {
            _orderEventRepository.RegisterOrderEventScheduler(ProcessorId);
            State = new Follower(this);
        }
        #endregion
    }
}
