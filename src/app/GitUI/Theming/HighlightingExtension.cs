﻿using GitExtUtils.GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Theming
{
    internal static class HighlightingExtension
    {
        public static HighlightColor Transform(this HighlightColor original)
        {
            Color backReplacement = Adapt(original.BackgroundColor, isForeground: false);
            Color replacement = Adapt(original.Color, isForeground: true);
            return new HighlightColor(original, replacement, backReplacement);

            Color Adapt(Color c, bool isForeground) =>
                !original.Adaptable || c.IsSystemColor
                    ? c
                    : ColorHelper.AdaptColor(c, isForeground);
        }
    }
}
