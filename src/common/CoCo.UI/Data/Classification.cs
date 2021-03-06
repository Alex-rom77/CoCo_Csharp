using System.Diagnostics;
using System.Windows.Media;

namespace CoCo.UI.Data
{
    [DebuggerDisplay("{Name}")]
    public sealed class Classification
    {
        public Classification(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public string Name { get; }

        public string DisplayName { get; }

        public string FontFamily { get; set; }

        public bool IsBold { get; set; }

        public string FontStyle { get; set; }

        public int FontStretch { get; set; }

        public bool IsOverline { get; set; }

        public bool IsUnderline { get; set; }

        public bool IsBaseline { get; set; }

        public bool IsStrikethrough { get; set; }

        public Color Foreground { get; set; }

        public bool ForegroundWasReset { get; set; }

        public Color Background { get; set; }

        public bool BackgroundWasReset { get; set; }

        public int FontRenderingSize { get; set; }

        public bool FontRenderingSizeWasReset { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsDisabledInEditor { get; set; }

        public bool IsDisabledInQuickInfo { get; set; }

        public bool IsDisabledInXml { get; set; }
    }
}