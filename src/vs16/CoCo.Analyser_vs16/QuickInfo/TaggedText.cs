using System.Diagnostics;

namespace CoCo.Analyser.QuickInfo
{
    /// <summary>
    /// Represents classification kind and the real part of text
    /// </summary>
    [DebuggerDisplay("{Text}")]
    public struct TaggedText
    {
        public string Tag { get; }

        public string Text { get; }

        public TaggedText(string tag, string text)
        {
            Tag = tag;
            Text = text;
        }

        public bool IsDefault => Tag is null || Text is null;
    }
}