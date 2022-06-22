using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisLock1
{
    /// <summary>
    /// 实现一个redis锁,实现对特定资源的占用
    /// </summary>
    public class RedisLock
    {
        private IDatabase IDatabase;
        private string key;
        private string value;
        private TimeSpan TimeSpan;
        private TimeSpan TimeOut;
        public RedisLock(IDatabase IDatabase, string key, string value, TimeSpan timeSpan, TimeSpan? timeout = null)
        {
            this.key = key;
            this.value = value;
            this.TimeSpan = timeSpan;
            this.TimeOut = timeout.HasValue ? timeout.Value : timeSpan.Add(new TimeSpan(0, 0, 2));
            this.IDatabase = IDatabase;
        }
        /// <summary>
        /// 处理动作
        /// </summary>
        public bool Process(Action action)
        {
            bool state = false;
            try
            {
                state = Lock();
                if (state)
                {
                    action?.Invoke();
                }
            }
            finally
            {
                UnLock(state);
            }
            return state;
        }
        /// <summary>
        /// 申请锁
        /// </summary>
        /// <returns></returns>
        private bool Lock()
        {
            DateTime dateTime = DateTime.Now;
            var state = false;
            //申请标志位
            while (!state && (DateTime.Now - dateTime) < TimeOut)
            {
                state = IDatabase.StringSet(key, value, TimeSpan, When.NotExists);
                if (state)
                {
                    break;
                }
                SpinWait.SpinUntil(() => false, 100);
            }
            return state;
        }
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <returns></returns>
        private bool UnLock(bool lockState)
        {
            if (lockState)
            {
                IDatabase.KeyDelete(key);
            }
            else
            {
                var data = IDatabase.StringGet(key);
                if (data == value)
                {
                    IDatabase.KeyDelete(key);
                }
            }
            return true;
        }
    }
}
