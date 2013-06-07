﻿/******************************************************************************
 * Copyright (c) 2013 ABB Group
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Jiang Zheng (ABB Group) - Initial implementation
 *****************************************************************************/
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;

namespace Sando.UI.Service {
    /// <summary>
    /// This class is used to expose some utility functions used in this project.
    /// </summary>
    internal static class HelperFunctions {
        /// <summary>
        /// This function is used to write a string on the Output window of Visual Studio.
        /// </summary>
        /// <param name="provider">The service provider to query for SVsOutputWindow</param>
        /// <param name="text">The text to write</param>
        internal static void WriteOnOutputWindow(IServiceProvider provider, string text) {
            // At first write the text on the debug output.
            //Trace.WriteLine(text);

            // Check if we have a provider
            if(null == provider) {
                // If there is no provider we can not do anything; exit now.
                //Trace.WriteLine("No service provider passed to WriteOnOutputWindow.");
                return;
            }

            // Now get the SVsOutputWindow service from the service provider.
            IVsOutputWindow outputWindow = provider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if(null == outputWindow) {
                // If the provider doesn't expose the service there is nothing we can do.
                // Write a message on the debug output and exit.
                //Trace.WriteLine("Can not get the SVsOutputWindow service.");
                return;
            }

            // We can not write on the Output window itself, but only on one of its panes.
            // Here we try to use the "General" pane.
            Guid guidGeneral = Microsoft.VisualStudio.VSConstants.GUID_OutWindowGeneralPane;
            IVsOutputWindowPane windowPane;
            if(Microsoft.VisualStudio.ErrorHandler.Failed(outputWindow.GetPane(ref guidGeneral, out windowPane)) ||
                (null == windowPane)) {
                Microsoft.VisualStudio.ErrorHandler.Failed(outputWindow.CreatePane(ref guidGeneral, "General", 1, 0));
                if(Microsoft.VisualStudio.ErrorHandler.Failed(outputWindow.GetPane(ref guidGeneral, out windowPane)) ||
                (null == windowPane)) {
                    // Again, there is nothing we can do to recover from this error, so write on the
                    // debug output and exit.
                    //Trace.WriteLine("Failed to get the Output window pane.");
                    return;
                }
                Microsoft.VisualStudio.ErrorHandler.Failed(windowPane.Activate());
            }

            // Finally we can write on the window pane.
            if(Microsoft.VisualStudio.ErrorHandler.Failed(windowPane.OutputString(text))) {
                //Trace.WriteLine("Failed to write on the Output window pane.");
            }
        }
    }
}
