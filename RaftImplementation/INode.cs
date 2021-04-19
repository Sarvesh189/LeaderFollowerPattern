using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RaftImplementation
{
   public interface INode
    {
       // NodeState State { get; set; }
        Task Start();
        Task Stop();        
        Task Register();
     }
}
