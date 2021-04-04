using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;

namespace TeamDevTool.EditorExtension
{
    internal class ConfigParameterTag : ITag
    {
    }
    internal class ConfigParameterTagger : ITagger<ConfigParameterTag>
    {
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private const string _searchText = "name";

        /// <summary>
        /// 创建ConfigParameterTag TagSpan
        /// </summary>
        /// <param name="spans"></param>
        /// <returns></returns>
        IEnumerable<ITagSpan<ConfigParameterTag>> ITagger<ConfigParameterTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            //todo: implement tagging
            foreach (SnapshotSpan curSpan in spans)
            {
                int loc = curSpan.GetText().ToLower().IndexOf(_searchText);
                if (loc > -1)
                {
                    SnapshotSpan todoSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curSpan.Start + loc, _searchText.Length));
                    yield return new TagSpan<ConfigParameterTag>(todoSpan, new ConfigParameterTag());
                }
            }
        }
    }
}