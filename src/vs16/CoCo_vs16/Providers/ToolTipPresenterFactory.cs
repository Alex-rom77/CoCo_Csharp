using System.Collections.Generic;
using System.ComponentModel.Composition;
using CoCo.Analyser;
using CoCo.QuickInfo;
using CoCo.Utils;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace CoCo.Providers
{
    [Export(typeof(IToolTipPresenterFactory))]
    [Name("CoCo tooltip factory")]
    [Order(Before = "default")]
    internal class ToolTipPresenterFactory : IToolTipPresenterFactory
    {
        /// <summary>
        /// Determines that settings were set to avoid a many sets settings from the classifier
        /// </summary>
        private bool _wereSettingsSet;

        private readonly Dictionary<string, QuickInfoState> _quickInfoOptions;

        [Import]
        private IViewElementFactoryService _viewElementFactoryService;

        public ToolTipPresenterFactory()
        {
            _quickInfoOptions = new Dictionary<string, QuickInfoState>
            {
                [Languages.CSharp] = default,
                [Languages.VisualBasic] = default
            };

            GeneralChangingService.Instance.GeneralChanged += OnGeneralChanged;
        }

        public IToolTipPresenter Create(ITextView textView, ToolTipParameters parameters)
        {
            MigrationService.MigrateSettingsTo_3_1_0();
            if (!_wereSettingsSet)
            {
                var settings = Settings.SettingsManager.LoadGeneralSettings(Paths.CoCoGeneralSettingsFile, MigrationService.Instance);
                var options = OptionService.ToOption(settings);
                GeneralChangingService.SetGeneralOptions(options);
                _wereSettingsSet = true;
            }

            var language = textView.TextBuffer.GetLanguage();
            if (language is null || _quickInfoOptions.TryGetValue(language, out var state) && state != QuickInfoState.Override)
            {
                // NOTE: the next tooltip presenter would be invoked when an one from the exported returns null
                return null;
            }

            return parameters.TrackMouse
                ? new MouseTrackToolTipPresenter(_viewElementFactoryService, textView, parameters)
                : new ToolTipPresenter(_viewElementFactoryService, textView, parameters);
        }

        private void OnGeneralChanged(GeneralChangedEventArgs args)
        {
            foreach (var (language, generalInfo) in args.Changes)
            {
                if (_quickInfoOptions.ContainsKey(language))
                {
                    _quickInfoOptions[language] = generalInfo.QuickInfoState;
                }
            }
        }
    }
}