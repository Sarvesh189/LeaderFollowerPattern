using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RaftImplementation
{
   public interface IOrderEventProcessor
    {
        Task ExecuteTask();
        void Stop();
    }
}
