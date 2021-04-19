using System;

namespace RaftImplementation
{
    public enum OrderEventStatus
    {
        Submitted=0,
        InProgress,
        OrderEventPublished,
    }
}
