using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamDevTool.EditorExtension
{
    /// <summary>
    /// Export a <see cref="IClassifierProvider"/>
    /// </summary>
    [Export(typeof(IClassifierProvider))]
    [ContentType("XML")]
    internal class ConfigParameterClassifierProvider : IClassifierProvider
    {
        /// <summary>
        /// 必须指定分类器对应的分类后的类型
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("configParameter")]
        internal ClassificationTypeDefinition ClassificationType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService _tagAggregatorFactory = null;

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            IClassificationType classificationType = ClassificationRegistry.GetClassificationType("configParameter");

            var tagAggregator = _tagAggregatorFactory.CreateTagAggregator<ConfigParameterTag>(buffer);
            return new ConfigParameterClassifier(tagAggregator, classificationType);
        }
    }
}
