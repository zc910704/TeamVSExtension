using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TeamDevTool.EditorExtension
{
    /// <summary>
    /// Set the display values for the classification
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "configParameter")]
    [Name("parameterName")]
    [UserVisible(true)]
    [Order(Before = Priority.High,After = Priority.High)]
    internal sealed class ParameterFormat : ClassificationFormatDefinition
    {
        public ParameterFormat()
        {
            DisplayName = "config file parameter name"; //human readable version of the name
            BackgroundOpacity = 1;
            ForegroundColor = Colors.Orange;
            /*BackgroundColor = Colors.Black;*/
        }
    }
}
