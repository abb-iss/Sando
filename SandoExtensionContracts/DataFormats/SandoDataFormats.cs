using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.ExtensionContracts.DataFormats
{
    /// <summary>
    /// Provides a set of data format names that can be used to identify data formats available in the clipboard or drag-and-drop operations.
    /// </summary>
    public class SandoDataFormats
    {
        /// <summary>
        /// Specifies the Sando Search Result data format. The type of this data should always be 
        /// Sando.ExtensionContracts.ResultsReordererContracts.SandoSearchResult.
        /// </summary>
        public static readonly string SandoSearchResult = "SandoSearchResult";
    }
}
