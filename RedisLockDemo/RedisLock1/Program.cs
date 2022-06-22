using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RedisLock1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "redis lock Demo1 by 蓝创精英团队";
            var connMultiplexer = ConnectionMultiplexer.Connect("127.0.0.1");
            Console.WriteLine($"redis连接状态:{connMultiplexer.IsConnected}");
            var db = connMultiplexer.GetDatabase(1);//可以选择指定的db，0-15
            Test(db);
            Console.WriteLine("redis lock 1 案例!");
            Console.ReadLine();
        }
        public static void Test(IDatabase database)
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var ID = Guid.NewGuid().ToString("N");
                    var redislock = new RedisLock(database, "key", ID, new TimeSpan(0, 0, 15), new TimeSpan(0, 0, 15 * 10));
                    redislock.Process(() => {
                        Console.WriteLine($"{DateTime.Now} - {ID}:申请到标志位!");
                        Console.WriteLine($"{DateTime.Now} - {ID}:处理自己的 事情!");
                        Thread.Sleep(5 * 1000);
                        Console.WriteLine($"{DateTime.Now} - {ID}:处理完毕!");
                    });
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
