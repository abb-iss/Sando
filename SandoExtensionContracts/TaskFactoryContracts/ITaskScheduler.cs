using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sando.ExtensionContracts.TaskFactoryContracts
{
    public interface ITaskScheduler
    {
        Task StartNew(Action a, CancellationTokenSource c);
    }
}
