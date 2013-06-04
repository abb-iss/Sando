using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Sando.UI
{
    public delegate void HighlightedEntityChanged(IEnumerable<HighlightedEntity> entities);

    public sealed class HighlightedEntity : IEquatable<HighlightedEntity>
    {
        public string Path { private set;  get; }
        public int StartLineNumber { private set; get; }
        public int LineCount { private set; get; }
        public string Rawsource { private set;  get; }

        public HighlightedEntity(String Path, int StartLineNumber, string Rawsource)
        {
            this.Path = Path;
            this.StartLineNumber = StartLineNumber;
            this.Rawsource = Rawsource;
            this.LineCount = Rawsource.Split('\n').Length;
        }

        public bool IsLineInEntity(int number)
        {
            return number >= StartLineNumber && number < StartLineNumber + LineCount;
        }

        public bool Equals(HighlightedEntity other)
        {
            return this.Path.Equals(other.Path) && this.StartLineNumber == other.StartLineNumber
                   && this.Rawsource.Equals(other.Rawsource);
        }
    }

    public sealed class HighlightedEntitySet
    {
        private static HighlightedEntitySet instance;
        
        public static HighlightedEntitySet GetInstance()
        {
            return instance ?? (instance = new HighlightedEntitySet());
        }

        private readonly List<HighlightedEntity> entities = new List<HighlightedEntity>();
        public event HighlightedEntityChanged entityChanged;

        public void AddEntity(String path, int start, String rawSource)
        {
            var ent = new HighlightedEntity(path, start, rawSource);
            lock (entities)
            {
                if (!entities.Contains(ent))
                {
                    entities.Add(ent);
                    entityChanged(entities.ToList());
                }
            }
        }

        public void RemoveEntity(String path, int start, String rawSource)
        {
            var ent = new HighlightedEntity(path, start, rawSource);
            lock (entities)
            {
                entities.Remove(ent);
                entityChanged(entities.ToList());
            }
        }

        public void Clear()
        {
            lock (entities)
            {
                entities.Clear();
                entityChanged(entities.ToList());
            }
        }
    }

    internal class HighlightWordTag : TextMarkerTag
    {
        public HighlightWordTag() : base("MarkerFormatDefinition/SandoSearchResult") { }
    }

    [Export(typeof(EditorFormatDefinition))]
    [Name("MarkerFormatDefinition/SandoSearchResult")]
    [UserVisible(true)]
    internal class HighlightWordFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordFormatDefinition()
        {
            this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.DarkBlue;
            this.DisplayName = "Highlight Word";
            this.ZOrder = 5;
        }
    }

    internal class HighlightWordTagger : ITagger<HighlightWordTag>
    {
        ITextView View { get; set; }
        ITextBuffer SourceBuffer { get; set; }
        ITextSearchService TextSearchService { get; set; }
        ITextStructureNavigator TextStructureNavigator { get; set; }
        private readonly String fileFullPath;

        private IEnumerable<HighlightedEntity> highlightedEntities; 
        private NormalizedSnapshotSpanCollection highlightedSpans;
        private object updateLock = new object();

        public HighlightWordTagger(ITextView view, ITextBuffer sourceBuffer, ITextSearchService 
            textSearchService, ITextStructureNavigator textStructureNavigator)
        {
            this.View = view;
            this.fileFullPath = GetFileFullPath(view);
            this.SourceBuffer = sourceBuffer;
            this.TextSearchService = textSearchService;
            this.TextStructureNavigator = textStructureNavigator;
            this.highlightedEntities = Enumerable.Empty<HighlightedEntity>();
            this.highlightedSpans = new NormalizedSnapshotSpanCollection();
            HighlightedEntitySet.GetInstance().entityChanged += OnHighlightedEntityChanged;
            this.View.Caret.PositionChanged += CaretPositionChanged;
            this.View.LayoutChanged += ViewLayoutChanged;
        }

        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.OldSnapshot != e.NewSnapshot)
            {
                lock (updateLock)
                {
                    this.highlightedSpans = SearchHighlightedSpans(this.highlightedEntities,
                        SourceBuffer.CurrentSnapshot);
                }
            }
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            lock (updateLock)
            {
                this.highlightedSpans = SearchHighlightedSpans(this.highlightedEntities,
                    SourceBuffer.CurrentSnapshot);
            }
        }

        private void OnHighlightedEntityChanged(IEnumerable<HighlightedEntity> entities)
        {
            lock (updateLock)
            {
                this.highlightedEntities = entities.Where(e => e.Path.Equals(fileFullPath));
                this.highlightedSpans = SearchHighlightedSpans(this.highlightedEntities,
                      SourceBuffer.CurrentSnapshot);
            }
            TriggerTagChangedEvent();
        }

        private NormalizedSnapshotSpanCollection SearchHighlightedSpans (IEnumerable<HighlightedEntity> 
            entities, ITextSnapshot snapshot)
        {
            var highLightedSpans = new List<SnapshotSpan>();
            foreach (var entity in entities)
            {
                var findData = new FindData(".*\n", snapshot);
                findData.FindOptions = FindOptions.UseRegularExpressions;
                var allLines = TextSearchService.FindAll(findData);
                highLightedSpans.AddRange(allLines.Where(l => entity.IsLineInEntity(l.Start.
                    GetContainingLine().LineNumber + 1)));
            }
            return new NormalizedSnapshotSpanCollection(highLightedSpans);
        }


        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;


        private void TriggerTagChangedEvent()
        {
            var tempEvent = TagsChanged;
            if (tempEvent != null)
            {
                tempEvent(this, new SnapshotSpanEventArgs(new SnapshotSpan(SourceBuffer.CurrentSnapshot,
                    0, SourceBuffer.CurrentSnapshot.Length)));
            }
        }

        private String GetFileFullPath(ITextView textView)
        {
            ITextDocument doc;
            bool success = textView.TextBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out doc);
            return success ? doc.FilePath : "";
        }

        public IEnumerable<ITagSpan<HighlightWordTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if(spans.Count == 0 || !highlightedSpans.Any())
                yield break;

            var highSpans = this.highlightedSpans;
            if (highSpans[0].Snapshot != spans[0].Snapshot)
            {
                highSpans = new NormalizedSnapshotSpanCollection(highSpans.Select(s => 
                    s.TranslateTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive)));
            }

            foreach (SnapshotSpan span in NormalizedSnapshotSpanCollection.Overlap(spans, highSpans))
            {
                yield return new TagSpan<HighlightWordTag>(span, new HighlightWordTag());
            }
        }
    }

    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(TextMarkerTag))]
    internal class HighlightWordTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal ITextSearchService TextSearchService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            //provide highlighting only on the top buffer 
            if (textView.TextBuffer != buffer)
                return null;

            ITextStructureNavigator textStructureNavigator =
                TextStructureNavigatorSelector.GetTextStructureNavigator(buffer);

            return new HighlightWordTagger(textView, buffer, TextSearchService, textStructureNavigator) as ITagger<T>;
        }
    }
}
