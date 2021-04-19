using RaftImplementation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LeaderFollowerPattern
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKey key = ConsoleKey.Enter;
            Console.WriteLine("please press Escape to exit");
            key = Console.ReadKey().Key;
            var processors = new List<OrderEventProcessor>();
            while(key != ConsoleKey.Escape)
            {
                if (key == ConsoleKey.S)
                {
                    if (processors.Count > 0)
                    {
                        var leader = processors.Find(p => p.State.GetType() == typeof(Leader));
                        leader.Stop();
                    }
                }
                else
                {
                    var orderEventProcessor1 = new OrderEventProcessor();
                    orderEventProcessor1.Start();
                    processors.Add(orderEventProcessor1);
                    Task.Delay(1000);
                    var orderEventProcessor2 = new OrderEventProcessor();
                    orderEventProcessor2.Start();
                    processors.Add(orderEventProcessor2);
                    Task.Delay(1000);
                    var orderEventProcessor3 = new OrderEventProcessor();
                    orderEventProcessor3.Start();
                    processors.Add(orderEventProcessor3);
                }
                Console.WriteLine("please press Escape to exit and [S]top Leader");
                
                key = Console.ReadKey().Key;

            }
        }
    }
}
