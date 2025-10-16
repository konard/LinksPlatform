using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Platform.Examples.Triggers;

namespace Platform.Examples.Html
{
    /// <summary>
    /// Parser for HTML that uses the trigger system for validation.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class HtmlParser<TLink>
    {
        private readonly ITrigger<TLink> _validationTrigger;
        private TLink _nextId;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlParser{TLink}"/> class.
        /// </summary>
        public HtmlParser()
        {
            _validationTrigger = new HtmlValidationTrigger<TLink>();
            _nextId = default(TLink);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlParser{TLink}"/> class with a custom validation trigger.
        /// </summary>
        /// <param name="validationTrigger">The validation trigger to use.</param>
        public HtmlParser(ITrigger<TLink> validationTrigger)
        {
            _validationTrigger = validationTrigger ?? throw new ArgumentNullException(nameof(validationTrigger));
            _nextId = default(TLink);
        }

        /// <summary>
        /// Parses and validates HTML content.
        /// </summary>
        /// <param name="html">The HTML content to parse.</param>
        /// <returns>A tuple containing the validation result and list of parsed elements.</returns>
        public (bool IsValid, List<HtmlElement<TLink>> Elements, List<string> Errors) Parse(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return (false, new List<HtmlElement<TLink>>(), new List<string> { "HTML content is empty" });
            }

            // Tokenize HTML
            var elements = Tokenize(html);

            // Create trigger context
            var context = new TriggerContext<TLink>
            {
                Data = new Dictionary<string, object>
                {
                    ["Elements"] = elements
                }
            };

            // Process with validation trigger
            _validationTrigger.Process(context);

            // Extract errors if any
            var errors = new List<string>();
            if (context.Data.ContainsKey("Errors"))
            {
                errors = context.Data["Errors"] as List<string> ?? new List<string>();
            }

            return (context.IsValid, elements, errors);
        }

        private List<HtmlElement<TLink>> Tokenize(string html)
        {
            var elements = new List<HtmlElement<TLink>>();

            // Simple regex-based tokenizer
            // Matches: opening tags, closing tags, self-closing tags, and text content
            var pattern = @"<\s*(/?)(\w+)(\s+[^>]*)?(/?)\s*>|([^<]+)";
            var matches = Regex.Matches(html, pattern);

            foreach (Match match in matches)
            {
                if (match.Groups[5].Success)
                {
                    // Text content
                    var text = match.Groups[5].Value.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        elements.Add(new HtmlElement<TLink>
                        {
                            Id = GetNextId(),
                            TextContent = text
                        });
                    }
                }
                else
                {
                    var isClosingTag = match.Groups[1].Value == "/";
                    var tagName = match.Groups[2].Value;
                    var isSelfClosing = match.Groups[4].Value == "/" || IsSelfClosingTag(tagName);

                    if (isSelfClosing && !isClosingTag)
                    {
                        elements.Add(new HtmlElement<TLink>
                        {
                            Id = GetNextId(),
                            TagName = tagName.ToLower(),
                            IsSelfClosing = true,
                            IsOpeningTag = true
                        });
                    }
                    else
                    {
                        elements.Add(new HtmlElement<TLink>
                        {
                            Id = GetNextId(),
                            TagName = tagName.ToLower(),
                            IsOpeningTag = !isClosingTag,
                            IsSelfClosing = false
                        });
                    }
                }
            }

            return elements;
        }

        private bool IsSelfClosingTag(string tagName)
        {
            var selfClosingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "br", "hr", "img", "input", "meta", "link", "area", "base", "col", "embed", "param", "source", "track", "wbr"
            };
            return selfClosingTags.Contains(tagName);
        }

        private TLink GetNextId()
        {
            // This is a simplified ID generation
            // In a real implementation with ILinks, this would use the links storage
            if (typeof(TLink) == typeof(int))
            {
                var id = Convert.ToInt32(_nextId) + 1;
                _nextId = (TLink)(object)id;
                return (TLink)(object)id;
            }
            else if (typeof(TLink) == typeof(ulong))
            {
                var id = Convert.ToUInt64(_nextId) + 1;
                _nextId = (TLink)(object)id;
                return (TLink)(object)id;
            }
            else if (typeof(TLink) == typeof(long))
            {
                var id = Convert.ToInt64(_nextId) + 1;
                _nextId = (TLink)(object)id;
                return (TLink)(object)id;
            }

            return default(TLink);
        }
    }
}
