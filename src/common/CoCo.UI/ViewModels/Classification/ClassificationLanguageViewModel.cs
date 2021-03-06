using CoCo.UI.Data;
using System.Collections.Generic;

namespace CoCo.UI.ViewModels
{
    // TODO: at this moment it's a bad design to have one parent view model for two differents view models
    // to pass a changings from the one to another, instead of raising events and correct handled them.
    public class ClassificationLanguageViewModel : BaseViewModel, IClassificationProvider
    {
        public ClassificationLanguageViewModel(ClassificationLanguage language, IResetValuesProvider resetValuesProvider)
        {
            Name = language.Name;
            ClassificationsContainer = new ClassificationsViewModel(language.Classifications, resetValuesProvider);
            PresetsContainer = new PresetsViewModel(language.Presets, this, resetValuesProvider);

            ClassificationsContainer.IsActive = true;
            ActivateClassifications = new DelegateCommand(() =>
            {
                PresetsContainer.IsActive = false;
                ClassificationsContainer.IsActive = true;
            });
            ActivatePresets = new DelegateCommand(() =>
            {
                ClassificationsContainer.IsActive = false;
                PresetsContainer.IsActive = true;
            });
        }

        public string Name { get; }

        public DelegateCommand ActivateClassifications { get; }

        public DelegateCommand ActivatePresets { get; }

        public ClassificationsViewModel ClassificationsContainer { get; }

        public PresetsViewModel PresetsContainer { get; }

        public ClassificationLanguage ExtractData()
        {
            var language = new ClassificationLanguage(Name);
            foreach (var item in ClassificationsContainer.Classifications)
            {
                language.Classifications.Add(item.ExtractData());
            }

            foreach (var item in PresetsContainer.Presets)
            {
                language.Presets.Add(item.ExtractData());
            }

            return language;
        }

        ICollection<ClassificationViewModel> IClassificationProvider.GetCurrentClassificaions() =>
            ClassificationsContainer.Classifications;

        void IClassificationProvider.SetCurrentClassificaions(ICollection<ClassificationViewModel> classifications)
        {
            var currentClassifications = ClassificationsContainer.Classifications;
            /// TODO: again bulk operation under a <see cref="ObservableCollection{T}"/>
            while (currentClassifications.Count > 0)
            {
                currentClassifications.RemoveAt(currentClassifications.Count - 1);
            }

            foreach (var item in classifications)
            {
                currentClassifications.Add(item);
            }
            // NOTE: Reset selected classification from old items
            ClassificationsContainer.SelectedClassification = null;
        }
    }
}