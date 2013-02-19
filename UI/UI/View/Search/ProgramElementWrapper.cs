using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.UI.View.Search
{
    public class ProgramElementWrapper
    {
        public ProgramElementWrapper(bool isChecked, ProgramElementType programElementType)
        {
            Checked = isChecked;
            ProgramElement = programElementType;
        }

        public bool Checked { get; set; }
        public ProgramElementType ProgramElement { get; set; }
    }
}