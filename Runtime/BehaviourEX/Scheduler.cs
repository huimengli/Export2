using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Export.BehaviourEX
{
    /// <summary>
    /// 提供静态携程队列
    /// </summary>
    public class Scheduler
    {
        /// <summary>
        /// 定义一个静态的、只读的锁对象，用于确保在多线程环境中对共享资源的访问是线程安全的。
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// 定义一个静态的、只读的动作队列，用于储存需要在主线程上执行的动作。
        /// </summary>
        private static readonly SortedList<long, Func<Task>> _taskQueue = new SortedList<long, Func<Task>>();

        /// <summary>
        /// 定义一个静态的、只读的计时器，用于定期检查和执行队列中的任务。
        /// </summary>
        private static Timer _timer;

        /// <summary>
        /// 检测下一个任务
        /// </summary>
        private static void CheckNextTask()
        {
            if (_taskQueue.Count == 0) return;

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var nextTask = _taskQueue.First();

            if (now >= nextTask.Key)
            {
                _ = ExecuteTaskAsync(nextTask.Value);
                _taskQueue.Remove(nextTask.Key);
                CheckNextTask();
            }
            else if (_timer == null)
            {
                _timer = new Timer(_ => Update(), null,
                    nextTask.Key - now, Timeout.Infinite);
            }
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private static async Task ExecuteTaskAsync(Func<Task> task)
        {
            try { await task(); }
            catch(Exception e) { 
                throw e;
            }
        }

        /// <summary>
        /// 添加一个任务到队列
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timestamp"></param>
        public static void AddTask(Func<Task> task, long timestamp)
        {
            lock (_lock)
            {
                _taskQueue.Add(timestamp, task);
                CheckNextTask();
            }
        }

        public static void AddRepeatTask(Func<Task> task, long interval)
        {
            lock (_lock)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var timestamp = now + interval;
                _taskQueue.Add(timestamp, async () =>
                {
                    await task();
                    AddTask(task, interval);
                });
                CheckNextTask();
            }
        }

        /// <summary>
        /// 更新,并检查下一个任务
        /// </summary>
        public static void Update()
        {
            lock (_lock)
            {
                CheckNextTask();
            }
        }
    }
}
