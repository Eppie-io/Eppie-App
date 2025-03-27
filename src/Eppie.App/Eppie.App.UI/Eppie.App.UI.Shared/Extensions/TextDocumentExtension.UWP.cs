#if WINDOWS_UWP

using System;
using System.Linq;
using System.Text;
using Windows.UI.Text;

namespace Tuvi.App.Shared.Extensions
{
    public static partial class TextDocumentExtension
    {
        public static string ToHtml(this ITextDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.GetText(TextGetOptions.None, out string text);

            // it seems that we have a bug in rich edit and we always get extra '\r, cut it off
            int length = text.Last() == SpecialChar.NewLine ? text.Length - 1 : text.Length;
            ITextRange txtRange = document.GetRange(0, length);

            return GetHtml(txtRange, length);
        }
    }
}

#endif
