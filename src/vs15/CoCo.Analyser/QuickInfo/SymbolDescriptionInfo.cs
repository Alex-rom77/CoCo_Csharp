﻿using System.Collections.Generic;
using System.Collections.Immutable;

namespace CoCo.Analyser.QuickInfo
{
    public struct SymbolDescriptionInfo
    {
        public SymbolDescriptionInfo(
            IReadOnlyDictionary<SymbolDescriptionKind, ImmutableArray<TaggedText>> descriptions, ImageKind image)
        {
            Image = image;
            Descriptions = descriptions;
        }

        public IReadOnlyDictionary<SymbolDescriptionKind, ImmutableArray<TaggedText>> Descriptions { get; }

        public ImageKind Image { get; }
    }
}