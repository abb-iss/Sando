using System;
using System.IO;
using System.Xml.Linq;
using Sando.Core.Extensions;
using Sando.Core.Logging;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer;
using Sando.Indexer.Documents;
using Sando.Indexer.IndexState;

namespace Sando.UI.Monitoring
{
    public class IndexUpdateManager
	{
        private readonly DocumentIndexer _currentIndexer;

        public IndexUpdateManager()
        {
            _currentIndexer = ServiceLocator.Resolve<DocumentIndexer>();
        }

        public void Update(string filePath, XElement xElement)
        {
            var fileInfo = new FileInfo(filePath);            
            try
            {
                var parsed = ExtensionPointsRepository.Instance.GetParserImplementation(fileInfo.Extension).Parse(filePath, xElement);

                var unresolvedElements = parsed.FindAll(pe => pe is CppUnresolvedMethodElement);
                if (unresolvedElements.Count > 0)
                {
                    //first generate program elements for all the included headers
                    var headerElements = CppHeaderElementResolver.GenerateCppHeaderElements(filePath, unresolvedElements);

                    //then try to resolve
                    foreach (CppUnresolvedMethodElement unresolvedElement in unresolvedElements)
                    {
                        var document = CppHeaderElementResolver.GetDocumentForUnresolvedCppMethod(unresolvedElement, headerElements);
                        if (document != null)
                        {
                            //writeLog( "- DI.AddDocument()");
                            _currentIndexer.AddDocument(document);
                        }
                    }
                }

                foreach (var programElement in parsed)
                {
                    if (!(programElement is CppUnresolvedMethodElement))
                    {
                        var document = DocumentFactory.Create(programElement);
                        if (document != null)
                        {
                            //writeLog( "- DI.AddDocument()");
                            _currentIndexer.AddDocument(document);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                FileLogger.DefaultLogger.Info("Exception in IndexUpdateManager.Update() " + e.Message + "\n" + e.StackTrace);
            }
        }
	}
}
