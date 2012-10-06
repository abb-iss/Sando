using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.UI.InterleavingExperiment.Multiplexing.MuxProgramElements
{
	public class MuxClassElement : ClassElement
	{
		public MuxClassElement(ClassElement copyFromElement) 
			: base(copyFromElement.Name, copyFromElement.DefinitionLineNumber, copyFromElement.FullFilePath, copyFromElement.Snippet, copyFromElement.AccessLevel,
			copyFromElement.Namespace, copyFromElement.ExtendedClasses, copyFromElement.ImplementedInterfaces, copyFromElement.Modifiers)
		{
			MuxedTag = 1;
		}

		[CustomIndexField] private int MuxedTag;
	}
}
