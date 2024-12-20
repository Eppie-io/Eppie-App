using System;
using System.Text;
using Windows.UI.Text;

namespace Tuvi.App.Shared.Extensions
{
    public static class TextDocumentExtension
    {
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

                    if (character == Convert.ToChar(13))
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
        public static string ToHtml(this ITextDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.GetText(TextGetOptions.None, out string text); // ToDo: warning Uno0001

            // it seems that we have a bug in rich edit and we always get extra '\r, cut it off
            int endPosition = text.Length == 0 ? text.Length : text.Length - 1;
            ITextRange txtRange = document.GetRange(0, endPosition); // ToDo: warning Uno0001

            var strHTML = new StringBuilder("<html><head></head><body>");

            float shtSize = 11; // Default font size

            var boldProcessor = new FormatProcessor(strHTML, "b");
            var italicProcessor = new FormatProcessor(strHTML, "i");
            var underlineProcessor = new FormatProcessor(strHTML, "u");
            var bulletList = new ListProcessor(strHTML, "<ul>", "</ul>");
            var numberedList = new ListProcessor(strHTML, "<ol type=\"i\">", "</ol>");

            for (int i = 0; i < endPosition; i++)
            {
                txtRange.SetRange(i, i + 1); // ToDo: warning Uno0001

                if (i == 0)
                {
                    shtSize = OpenNewFontSpan();
                }

                if (txtRange.CharacterFormat.Size != shtSize) // ToDo: warning Uno0001
                {
                    strHTML.Append("</span>");
                    shtSize = OpenNewFontSpan();
                }

                // bullet
                bool appendToOutput = bulletList.Process(txtRange.ParagraphFormat.ListType == MarkerType.Bullet, txtRange.Character); // ToDo: warning Uno0001

                // numbering
                appendToOutput = numberedList.Process(txtRange.ParagraphFormat.ListType == MarkerType.LowercaseRoman, txtRange.Character); // ToDo: warning Uno0001

                // new line should be placed here, right after list processing, to assure that we correctly processed list items
                if (appendToOutput && txtRange.Character == Convert.ToChar(13)) // ToDo: warning Uno0001
                {
                    strHTML.Append("<br/>");
                    appendToOutput = false;
                }

                // bold
                boldProcessor.Process(txtRange.CharacterFormat.Bold == FormatEffect.On); // ToDo: warning Uno0001

                // italic
                italicProcessor.Process(txtRange.CharacterFormat.Italic == FormatEffect.On); // ToDo: warning Uno0001

                // underline
                underlineProcessor.Process(txtRange.CharacterFormat.Underline == UnderlineType.Single); // ToDo: warning Uno0001

                if (appendToOutput)
                {
                    strHTML.Append(txtRange.Character); // ToDo: warning Uno0001
                }
            }

            strHTML.Append("</span></body></html>");

            return strHTML.ToString();

            float OpenNewFontSpan()
            {
                float fontSize = txtRange.CharacterFormat.Size; // ToDo: warning Uno0001
                string strFntName = txtRange.CharacterFormat.Name; // ToDo: warning Uno0001
                strHTML.Append("<span style=\"font-family:")
                       .Append(strFntName)
                       .Append("; font-size: ")
                       .Append(fontSize)
                       .Append("pt;")
                       .Append("\">");
                return fontSize;
            }
        }
    }
}
