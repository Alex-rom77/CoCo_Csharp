using System.Diagnostics;
using System.Windows.Media;

namespace CoCo.Settings
{
    // NOTE: use nullable to determine when value was presented or not
    [DebuggerDisplay("{Name}")]
    public struct ClassificationSettings
    {
        public string Name { get; set; }

        public string FontFamily { get; set; }

        public bool? IsBold { get; set; }

        public string FontStyle { get; set; }

        public int? FontStretch { get; set; }

        public bool? IsOverline { get; set; }

        public bool? IsUnderline { get; set; }

        public bool? IsBaseline { get; set; }

        public bool? IsStrikethrough { get; set; }

        public Color? Foreground { get; set; }

        public Color? Background { get; set; }

        public int? FontRenderingSize { get; set; }

        public bool? IsDisabled { get; set; }

        public bool? IsDisabledInXml { get; set; }

        public bool? IsDisabledInEditor { get; set; }

        public bool? IsDisabledInQuickInfo { get; set; }
    }
}