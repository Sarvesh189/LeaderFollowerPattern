using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RaftImplementation
{
    public abstract class State 
    {
        public OrderEventProcessor OrderEventProcessor { get; set; }    
        
        protected State(OrderEventProcessor orderEventProcessor)
        {
            OrderEventProcessor = orderEventProcessor;
        }    
        

        public abstract Task ExecuteTask();

     
    }
}
