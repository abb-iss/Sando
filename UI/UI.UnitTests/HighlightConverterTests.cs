using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using NUnit.Framework;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.UI.View.Search.Converters;

namespace Sando.UI.UnitTests
{
    [TestFixture]
    class HighlightConverterTests
    {
        private class InternalHighlightRawInfo : IHighlightRawInfo
        {
            public string Text { get; private set; }
            public int StartLineNumber { get; private set; }
            public int[] Offsets { get; private set; }
            public IndentionOption IndOption { get; private set; }

            internal InternalHighlightRawInfo(String Text, int StartLineNumber, int[] Offsets = null)
            {
                this.Text = Text;
                this.StartLineNumber = StartLineNumber;
                this.Offsets = Offsets;
            }
        }
        
        [Test]
        public void MakeSureEmptyLinesAreHandled()
        {
            var converter = new HighlightSearchKey();
            var sb = new StringBuilder();
            for (int i = 0; i < 10; i ++)
            {
                sb.AppendLine("abc");
                sb.AppendLine();
            }
            var info = new InternalHighlightRawInfo(sb.ToString(), 1);
            var lines = ((Span) converter.Convert(info, null, null, null)).Inlines.ToArray();
            var n = Environment.NewLine;
        }

        [Test]
        public void HandleTextWithHeadingAndTailingEmptyLines()
        {
            var converter = new HighlightSearchKey();
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine();
            for (int i = 0; i < 10; i++)
            {
                sb.AppendLine("abc");
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine();
            var info = new InternalHighlightRawInfo(sb.ToString(), 1);
            converter.Convert(info, null, null, null);
        }

        [Test]
        public void HighlightedWordAtBeginningTest()
        {
            var text = "|~S~|add|~E~|{\r\n}";
            var info = new InternalHighlightRawInfo(text, 1);
            var converter = new HighlightSearchKey();
            converter.Convert(info, null, null, null);
        }
    }
}
