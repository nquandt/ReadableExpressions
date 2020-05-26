﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if DEBUG && NET40
    using System.Diagnostics;
#endif
    using System.Text;
    using Formatting;
    using Interfaces;
    using static Formatting.TokenType;

    internal class TranslationWriter : ITranslationQuery
    {
        private readonly ITranslationFormatter _formatter;
        private readonly string _indent;
#if DEBUG && NET40
        private readonly int _estimatedSize;
#endif
        private readonly StringBuilder _content;
        private int _currentIndent;
        private bool _writeIndent;

        public TranslationWriter(ITranslationSettings settings, ITranslatable translatable)
            : this(
                settings.Formatter,
                settings.Indent,
                translatable.TranslationSize +
                translatable.FormattingSize +
                translatable.GetIndentSize())
        {
            translatable.WriteTo(this);
        }

        public TranslationWriter(ITranslationSettings settings, int estimatedSize)
            : this(settings.Formatter, settings.Indent, estimatedSize)
        {
        }

        public TranslationWriter(
            ITranslationFormatter formatter,
            string indent,
            int estimatedSize)
        {
            _formatter = formatter ?? NullTranslationFormatter.Instance;
            _indent = indent;
#if DEBUG && NET40
            _estimatedSize = estimatedSize;
#endif
            _content = new StringBuilder(estimatedSize);
        }


        #region ITranslationQuery

        bool ITranslationQuery.TranslationEndsWith(char character)
        {
            if (_content.Length == 0)
            {
                return false;
            }

            var newlineEncountered = false;

            for (var i = _content.Length; i > 0;)
            {
                --i;
                var contentCharacter = _content[i];

                if (contentCharacter == '\n')
                {
                    if (newlineEncountered)
                    {
                        return false;
                    }

                    newlineEncountered = true;
                    continue;
                }

                if (char.IsWhiteSpace(contentCharacter))
                {
                    if (contentCharacter != character)
                    {
                        continue;
                    }

                    return true;
                }

                return contentCharacter == character;
            }

            return false;
        }

        bool ITranslationQuery.TranslationEndsWith(string substring)
        {
            var substringLength = substring.Length;

            if (_content.Length < substringLength)
            {
                return false;
            }

            var newlineEncountered = false;
            var finalCharacter = substring[substringLength - 1];

            for (var i = _content.Length - 1; i > 0;)
            {
                var contentCharacter = _content[i];

                if (contentCharacter == '\n')
                {
                    if (newlineEncountered)
                    {
                        return false;
                    }

                    --i;
                    newlineEncountered = true;
                    continue;
                }

                if (char.IsWhiteSpace(contentCharacter))
                {
                    --i;
                    continue;
                }

                if (contentCharacter != finalCharacter)
                {
                    return false;
                }

                for (var j = substringLength - 2; j > -1;)
                {
                    --i;

                    if (_content[i] != substring[j])
                    {
                        return false;
                    }

                    --j;
                }

                return true;
            }

            return false;
        }

        bool ITranslationQuery.TranslationEndsWithBlankLine()
        {
            if (_content.Length < 2)
            {
                return false;
            }

            var newlineEncountered = false;

            for (var i = _content.Length - 1; i > 0;)
            {
                var contentCharacter = _content[i];

                if (contentCharacter == '\n')
                {
                    if (newlineEncountered)
                    {
                        return true;
                    }

                    --i;
                    newlineEncountered = true;
                    continue;
                }

                if (char.IsWhiteSpace(contentCharacter))
                {
                    --i;
                    continue;
                }

                return false;
            }

            return false;
        }

        #endregion

        public bool TranslationQuery(Func<ITranslationQuery, bool> predicate)
            => predicate.Invoke(this);

        public void Indent()
        {
            ++_currentIndent;

            if (_writeIndent == false)
            {
                _writeIndent = true;
            }
        }

        public void Unindent() => --_currentIndent;

        public void WriteNewLineToTranslation()
        {
            _content.Append(Environment.NewLine);

            if (_currentIndent != 0)
            {
                _writeIndent = true;
            }
        }

        public void WriteToTranslation(char character)
            => WriteToTranslation(character, Default);

        private void WriteToTranslation(char character, TokenType tokenType)
        {
            WriteIndentIfRequired();
            _formatter.WriteFormatted(character, Write, Write, tokenType);
        }

        private void Write(char character) => _content.Append(character);

        public void WriteToTranslation(string stringValue, TokenType tokenType = Default)
        {
            if (stringValue.Length == 1)
            {
                WriteToTranslation(stringValue[0], tokenType);
                return;
            }

            WriteIndentIfRequired();

            if (tokenType != Default)
            {
                _formatter.WriteFormatted(stringValue, Write, tokenType);
                return;
            }

            Write(stringValue);
        }

        private void Write(string stringValue) => _content.Append(stringValue);

        public void WriteToTranslation(int intValue)
        {
            WriteIndentIfRequired();
            _formatter.WriteFormatted(intValue, Write, Write, Numeric);
        }

        private void Write(int intValue) => _content.Append(intValue);

        public void WriteToTranslation(long longValue)
        {
            WriteIndentIfRequired();
            _formatter.WriteFormatted(longValue, Write, Write, Numeric);
        }

        private void Write(long longValue) => _content.Append(longValue);

        public void WriteToTranslation(object value)
        {
            WriteIndentIfRequired();
            _content.Append(value);
        }

        private void WriteIndentIfRequired()
        {
            if (_writeIndent)
            {
                for (var i = 0; i < _currentIndent; ++i)
                {
                    _content.Append(_indent);
                }

                _writeIndent = false;
            }
        }

        public string GetContent()
        {
#if DEBUG && NET40
            if (AppDomain.CurrentDomain.IsFullyTrusted)
            {
                Debug.WriteIf(
                    _estimatedSize >= _content.Length,
                    $"TranslationBuffer: estimated: {_estimatedSize}, actual " + _content.Length);
            }
#endif
            return (_content.Length > 0) ? _content.ToString() : null;
        }
    }
}