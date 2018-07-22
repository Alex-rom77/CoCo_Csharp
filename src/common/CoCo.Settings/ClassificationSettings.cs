﻿using System.Diagnostics;
using System.Windows.Media;

namespace CoCo.Settings
{
    // NOTE: use nullable to determine when value was presented or not
    [DebuggerDisplay("{Name}")]
    public struct ClassificationSettings
    {
        public string Name { get; set; }

        public bool? IsBold { get; set; }

        public bool? IsItalic { get; set; }

        public Color? Foreground { get; set; }

        public Color? Background { get; set; }

        public int? FontRenderingSize { get; set; }

        public bool? IsEnabled { get; set; }
    }
}