using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlackHen.Threading;

namespace Sando.Core.Tools
{
    public delegate void WorkItemFinished(object sender, object output);

    public class WorkQueueBasedProcess
    {
        private readonly WorkQueue queue;

        public WorkQueueBasedProcess()
        {
            queue = new WorkQueue {ConcurrentLimit = 1};
        }

        public void Enqueue<T1, T2>(Func<T1, T2> function, T1 input, 
            WorkItemFinished method = null)
        {
            var item = new FuncWorkItem<T1, T2>(function, input);
            if(method != null)
                item.FinishedEvent += method;
            queue.Add(item);
        }

        public void Enqueue(Action action, WorkItemFinished method = null)
        {
            var item = new FuncWorkItem<int, int>(i => { 
                action.Invoke();
                return 1;
            }, 1);
            if (method != null)
                item.FinishedEvent += method;
            queue.Add(item);
        }

        private class FuncWorkItem<T1,T2> : WorkItem 
        {
            private readonly Func<T1, T2> func;
            private readonly T1 input;
            public event WorkItemFinished FinishedEvent;

            internal FuncWorkItem(Func<T1, T2> func, T1 input)
            {
                this.func = func;
                this.input = input;
            }

            public override void Perform()
            {
                var output = func.Invoke(input);
                if(FinishedEvent != null)
                    FinishedEvent(this, output);
            }
        }
    }
}
