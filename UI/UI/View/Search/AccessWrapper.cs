using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.UI.View.Search
{
    public class AccessWrapper
    {
        public AccessWrapper(bool isChecked, AccessLevel accessLevel)
        {
            Checked = isChecked;
            Access = accessLevel;
        }

        public bool Checked { get; set; }
        public AccessLevel Access { get; set; }
    }
}