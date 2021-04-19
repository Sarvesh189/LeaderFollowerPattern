using System;
using System.Collections.Generic;
using System.Text;

namespace RaftImplementation
{
    public enum OrderEventProcessorState
    {
        FOLLOWER=0,
        CANDIDATE,
        LEADER
    }
}
