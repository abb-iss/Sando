using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Sando.Core.Tools
{
    public class TimedProcessor
    {
        private static TimedProcessor instance;
        private readonly List<TimeWorkItem> workItems = new List<TimeWorkItem>(); 
        private readonly object locker = new object();

        public static TimedProcessor GetInstance()
        {
            return instance ?? (instance = new TimedProcessor());
        }

        private class TimeWorkItem : IDisposable
        {
            public Action Task { private set; get; }
            private int MilliSeconds { set; get; }
            private readonly Timer timer;

            public TimeWorkItem(Action Task, int MilliSeconds)
            {
                this.Task = Task;
                this.MilliSeconds = MilliSeconds;

                this.timer = new Timer(MilliSeconds);
                timer.Elapsed += (sender, args) => Task.Invoke();
                timer.Enabled = true;
                timer.AutoReset = true;
            }

            public void Dispose()
            {
                timer.Enabled = false;
            }
        }

        public void AddTimedTask(Action task, int time)
        {
            lock (locker)
            {
                workItems.Add(new TimeWorkItem(task, time));
            }
        }

        public void RemoveTimedTask(Action task)
        {
            lock (locker)
            {
                int index = workItems.FindIndex(item => item.Task == task);
                if (workItems.IsIndexInRange(index))
                {
                    workItems.ElementAt(index).Dispose();
                    workItems.RemoveAt(index);
                }
            }
        }

        public bool HasTasks()
        {
            lock (locker)
            {
                return workItems.Any();
            }
        }
    }
}
