using System.Text;

#if WINDOWS_UWP
using Windows.UI.Text;
#else
using Microsoft.UI.Text;
#endif

namespace Tuvi.App.Shared.Extensions
{
    public static partial class TextDocumentExtension
    {
        private readonly struct SpecialChar
        {
            public static readonly char NewLine = '\r';
            public static readonly char VerticalTab = '\v';
            public static readonly char Space = ' ';
            public static readonly char LessThan = '<';
            public static readonly char GreaterThan = '>';
        }

        private struct ListProcessor
        {
            private StringBuilder _sb;
            private bool _listOpened;
            private bool _itemOpened;
            private string _openTag;
            private string _closeTag;
            public ListProcessor(StringBuilder sb, string openTag, string closeTag)
            {
                _sb = sb;
                _listOpened = false;
                _itemOpened = false;
                _openTag = openTag;
                _closeTag = closeTag;
            }

            public bool Process(bool condition, char character)
            {
                if (condition)
                {
                    if (!_listOpened)
                    {
                        _sb.Append(_openTag);
                        _listOpened = true;
                    }

                    if (!_itemOpened)
                    {
                        _sb.Append("<li>");
                        _itemOpened = true;
                    }

                    if (character == SpecialChar.NewLine)
                    {
                        _sb.Append("</li>");
                        _itemOpened = false;
                        return false; // DO not append this characted to the output
                    }
                }
                else
                {
                    if (_listOpened)
                    {
                        _sb.Append(_closeTag);
                        _listOpened = false;
                    }
                }
                return true; // append current character to the output
            }
        }

        private struct FormatProcessor
        {
            private StringBuilder _sb;
            private bool _opened;
            private string _tag;
            public FormatProcessor(StringBuilder sb, string tag)
            {
                _sb = sb;
                _opened = false;
                _tag = tag;
            }

            public void Process(bool condition)
            {
                if (condition)
                {
                    if (!_opened)
                    {
                        _sb.Append('<')
                           .Append(_tag)
                           .Append('>');
                        _opened = true;
                    }
                }
                else
                {
                    if (_opened)
                    {
                        _sb.Append("</")
                           .Append(_tag)
                           .Append('>');
                        _opened = false;
                    }
                }
            }
        }

#if WINDOWS_UWP || WINDOWS10_0_19041_0_OR_GREATER

        private static string GetHtml(ITextRange range, int length)
        {
            var strHTML = new StringBuilder("<html><head></head><body>");

            float fontSize = 11; // Default font size

            var boldProcessor = new FormatProcessor(strHTML, "b");
            var italicProcessor = new FormatProcessor(strHTML, "i");
            var underlineProcessor = new FormatProcessor(strHTML, "u");
            var bulletList = new ListProcessor(strHTML, "<ul>", "</ul>");
            var numberedList = new ListProcessor(strHTML, "<ol type=\"i\">", "</ol>");

            for (int i = 0; i < length; i++)
            {
                range.SetRange(i, i + 1);

                if (i == 0)
                {
                    fontSize = OpenNewFontSpan();
                }

                if (range.CharacterFormat.Size != fontSize)
                {
                    strHTML.Append("</span>");
                    fontSize = OpenNewFontSpan();
                }

                // bullet
                bool appendToOutput = bulletList.Process(range.ParagraphFormat.ListType == MarkerType.Bullet, range.Character);

                // numbering
                appendToOutput = numberedList.Process(range.ParagraphFormat.ListType == MarkerType.LowercaseRoman, range.Character);

                // new line should be placed here, right after list processing, to assure that we correctly processed list items
                if (appendToOutput && (range.Character == SpecialChar.NewLine || range.Character == SpecialChar.VerticalTab))
                {
                    strHTML.Append("<br/>");
                    appendToOutput = false;
                }

                // to insert multiple spaces into a document
                if (appendToOutput && range.Character == SpecialChar.Space)
                {
                    strHTML.Append("&nbsp;");
                    appendToOutput = false;
                }

                // bold
                boldProcessor.Process(range.CharacterFormat.Bold == FormatEffect.On);

                // italic
                italicProcessor.Process(range.CharacterFormat.Italic == FormatEffect.On);

                // underline
                underlineProcessor.Process(range.CharacterFormat.Underline == UnderlineType.Single);

                if (appendToOutput)
                {
                    // Escape angle brackets
                    if (range.Character == SpecialChar.LessThan)
                    {
                        strHTML.Append("&lt;");
                    }
                    else if (range.Character == SpecialChar.GreaterThan)
                    {
                        strHTML.Append("&gt;");
                    }
                    else
                    {
                        // Check for surrogate pairs
                        if (char.IsHighSurrogate(range.Character) && i + 1 < length)
                        {
                            range.SetRange(i, i + 2);
                            range.GetText(TextGetOptions.None, out string surrogateStr);

                            strHTML.Append(surrogateStr);
                            i = i + surrogateStr.Length - 1; // Skip the low surrogate character
                        }
                        else
                        {
                            strHTML.Append(range.Character);
                        }
                    }
                }
            }

            strHTML.Append("</span></body></html>");

            return strHTML.ToString();

            float OpenNewFontSpan()
            {
                float size = range.CharacterFormat.Size;
                string name = range.CharacterFormat.Name;
                strHTML.Append("<span style=\"font-family:")
                       .Append(name)
                       .Append("; font-size: ")
                       .Append(size)
                       .Append("pt;")
                       .Append("\">");
                return size;
            }
        }
#endif

    }
}
