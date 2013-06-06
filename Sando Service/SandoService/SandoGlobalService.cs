/******************************************************************************
 * Copyright (c) 2013 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Xiao Qu (ABB Group) - Initial implementation
 *****************************************************************************/
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using log4net;

namespace Sando.Service {
    /// <summary>
    /// Implement the global service class.
    /// This is the class that implements the global service. All it needs to do is to implement 
    /// the interfaces exposed by this service (in this case ISrcMLGlobalService).
    /// This class also needs to implement the SSandoGlobalService interface in order to notify the 
    /// package that it is actually implementing this service.
    /// </summary>
    public class SandoGlobalService : ISandoGlobalService, SSandoGlobalService {

        /// <summary>
        /// Store in this variable the service provider that will be used to query for other services.
        /// </summary>
        private IServiceProvider serviceProvider;
               

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="extensionDirectory"></param>
        public SandoGlobalService(IServiceProvider sp) {
            //Trace.WriteLine("Constructing a new instance of SandoGlobalService");
            serviceProvider = sp;            
        }

        // ISandoGlobalService Members
        #region ISandoGlobalService Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Microsoft.Samples.VisualStudio.Services.HelperFunctions.WriteOnOutputWindow(System.IServiceProvider,System.String)")]
        public void GlobalServiceFunction()
        {
            string outputText = " ======================================\n" +
                                "\tGlobalServiceFunction called.\n" +
                                " ======================================\n";
            HelperFunctions.WriteOnOutputWindow(serviceProvider, outputText);
        }

        /// <summary>
        /// Implementation of the function that will call a method of the local service.
        /// Notice that this class will access the local service using as service provider the one
        /// implemented by ServicesPackage.
        /// </summary>
        public int CallLocalService()
        {
            // Query the service provider for the local service.
            // This object is supposed to be build by ServicesPackage and it pass its service provider
            // to the constructor, so the local service should be found.
            ISandoLocalService localService = serviceProvider.GetService(typeof(SSandoLocalService)) as ISandoLocalService;
            if (null == localService)
            {
                // The local service was not found; write a message on the debug output and exit.
                Trace.WriteLine("Can not get the local service from the global one.");
                return -1;
            }

            // Now call the method of the local service. This will write a message on the output window.
            return localService.LocalServiceFunction();
        }

        #endregion        

    }
}
