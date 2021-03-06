using System.Collections.Generic;
using CoCo.Analyser.Classifications;
using CoCo.Analyser.Classifications.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace CoCo.Analyser.Editor
{
    internal class VisualBasicTextBufferClassifier : RoslynTextBufferClassifier
    {
        private readonly VisualBasicClassifierService _service;

        internal VisualBasicTextBufferClassifier(Dictionary<string, ClassificationInfo> classifications) : base()
        {
            _service = VisualBasicClassifierService.GetClassifier(classifications);
        }

        internal VisualBasicTextBufferClassifier(
            Dictionary<string, ClassificationInfo> classifications,
            IClassificationChangingService classificationChangingService,
            bool isEnable,
            IEditorChangingService editorChangingService,
            ITextDocumentFactoryService textDocumentFactoryService,
            ITextBuffer buffer) : base(isEnable, editorChangingService, textDocumentFactoryService, buffer)
        {
            _service = VisualBasicClassifierService.GetClassifier(classifications, classificationChangingService);
        }

        protected override string Language => Languages.VisualBasic;

        internal override ICodeClassifier CodeClassifier => _service;

        internal override List<ClassificationSpan> GetClassificationSpans(
            Workspace workspace, SemanticModel semanticModel, SnapshotSpan span)
        {
            return _service.GetClassificationSpans(workspace, semanticModel, span);
        }
    }
}