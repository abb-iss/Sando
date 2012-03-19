using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace Sando.UI.Monitoring
{
    public class SolutionWrapper
    {
        public ProjectItem FindProjectItem(string name)
        {
            throw new NotImplementedException();
        }

        public Projects getProjects()
        {
            throw new NotImplementedException();
        }

        public static SolutionWrapper Create(Solution openSolution)
        {
            return new StandardSolutionWrapper(openSolution);
        }
    }

    public class StandardSolutionWrapper:SolutionWrapper
    {
        private Solution _mySolution;
        public StandardSolutionWrapper(Solution s)
        {
            _mySolution = s;
        }

        public ProjectItem FindProjectItem(string name)
        {
            return _mySolution.FindProjectItem(name);
        }

        public Projects getProjects()
        {
            return _mySolution.Projects;
        }
    }

}
