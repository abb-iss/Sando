// -----------------------------------------------------------------------
// <copyright file="MockProgress.cs" company="ABB">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Sando.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class MockProgress : IProgressCallback
    {
        private IInvoker invoker;

        public MockProgress(IInvoker invoker)
        {
            this.invoker = invoker;
        }

        public MockProgress()
        {
            // TODO: Complete member initialization
        }
        public void Begin(int minimum, int maximum)
        {
            //throw new NotImplementedException();
        }

        public void Begin()
        {
            // throw new NotImplementedException();
        }

        public void SetRange(int minimum, int maximum)
        {
            // throw new NotImplementedException();
        }

        public void SetText(string text)
        {
            // throw new NotImplementedException();
        }

        public void StepTo(int val)
        {
            // throw new NotImplementedException();
        }

        public void Increment(int val)
        {
            //throw new NotImplementedException();
        }

        public bool IsAborting
        {
            get { throw new NotImplementedException(); }
        }

        public void End()
        {
            // throw new NotImplementedException();
        }



        public void Invoke(System.Windows.Forms.MethodInvoker globalSystemWindowsFormsMethodInvoker)
        {
            invoker.Invoke(globalSystemWindowsFormsMethodInvoker);
        }

        public void ShowDialog()
        {
            // throw new NotImplementedException();
        }

        public void Close()
        {
            //throw new NotImplementedException();
        }

        public static MockProgress getProgress(IInvoker invoker)
        {
            return new MockProgress(invoker);
        }


        public void Invoke(System.Windows.Forms.MethodInvoker methodInvoker, bool forceUi)
        {
            invoker.Invoke(methodInvoker);
        }
    }
}
