using System.Collections.Generic;
using System.ComponentModel.Composition;
using CoCo.Analyser;
using CoCo.Analyser.Classifications;
using CoCo.Analyser.Classifications.VisualBasic;
using CoCo.Analyser.Editor;
using CoCo.Editor;
using CoCo.Settings;
using CoCo.Utils;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace CoCo.Providers
{
    /// <summary>
    /// Classifier provider which adds <see cref="VisualBasicTextBufferClassifier"/> to the set of classifiers.
    /// </summary>
    [Export(typeof(IClassifierProvider))]
    [ContentType("Basic")]
    public class VisualBasicClassifierProvider : IClassifierProvider
    {
        private readonly Dictionary<string, ClassificationInfo> _classificationsInfo;

        /// <summary>
        /// Determines that settings was set to avoid a many sets settings from the classifier
        /// </summary>
        private bool _wereSettingsSet;

        /// <summary>
        /// Determines that classifications in editor is enable or not
        /// </summary>
        private bool _isEnable;

        public VisualBasicClassifierProvider()
        {
            _classificationsInfo = new Dictionary<string, ClassificationInfo>(VisualBasicNames.All.Length);
            foreach (var item in VisualBasicNames.All)
            {
                _classificationsInfo[item] = default;
            }
            ClassificationChangingService.Instance.ClassificationChanged += OnAnalyzeOptionChanged;
            GeneralChangingService.Instance.EditorOptionsChanged += OnEditorOptionsChanged;
        }

#pragma warning disable 649

        /// <summary>
        /// Text document factory to be used for getting a event of text document disposed.
        /// </summary>
        [Import]
        private ITextDocumentFactoryService _textDocumentFactoryService;

#pragma warning restore 649

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            MigrationService.MigrateSettingsTo_2_0_0();
            MigrationService.MigrateSettingsTo_3_1_0();
            if (!_wereSettingsSet)
            {
                var editorSettings = SettingsManager.LoadEditorSettings(Paths.CoCoClassificationSettingsFile, MigrationService.Instance);
                var editorOption = OptionService.ToOption(editorSettings);
                FormattingService.SetFormattingOptions(editorOption);
                ClassificationChangingService.SetAnalyzingOptions(editorOption);

                var generalSettings = SettingsManager.LoadGeneralSettings(Paths.CoCoGeneralSettingsFile, MigrationService.Instance);
                var generalOption = OptionService.ToOption(generalSettings);
                GeneralChangingService.SetGeneralOptions(generalOption);

                _wereSettingsSet = true;
            }

            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new VisualBasicTextBufferClassifier(
                _classificationsInfo, ClassificationChangingService.Instance,
                _isEnable, GeneralChangingService.Instance,
                _textDocumentFactoryService, textBuffer));
        }

        private void OnAnalyzeOptionChanged(ClassificationsChangedEventArgs args)
        {
            foreach (var (classificationType, info) in args.ChangedClassifications)
            {
                if (_classificationsInfo.ContainsKey(classificationType.Classification))
                {
                    _classificationsInfo[classificationType.Classification] = new ClassificationInfo(classificationType, info);
                }
            }
        }

        private void OnEditorOptionsChanged(EditorChangedEventArgs args)
        {
            if (args.Changes.TryGetValue(Languages.VisualBasic, out var isEnable))
            {
                _isEnable = isEnable;
            }
        }
    }
}