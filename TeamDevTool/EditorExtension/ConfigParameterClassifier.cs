using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamDevTool.EditorExtension
{
    /// <summary>
    /// 
    /// </summary>
    class ConfigParameterClassifier : IClassifier
    {
        private IClassificationType _classificationType;
        private ITagAggregator<ConfigParameterTag> _tagger;

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        internal ConfigParameterClassifier(ITagAggregator<ConfigParameterTag> tagger, IClassificationType type)
        {
            _tagger = tagger;
            _classificationType = type;
        }


        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            IList<ClassificationSpan> classifiedSpans = new List<ClassificationSpan>();

            var tags = _tagger.GetTags(span);

            foreach (IMappingTagSpan<ConfigParameterTag> tagSpan in tags)
            {
                SnapshotSpan todoSpan = tagSpan.Span.GetSpans(span.Snapshot).First();
                classifiedSpans.Add(new ClassificationSpan(todoSpan, _classificationType));
            }

            return classifiedSpans;
        }
    }
}
