using System;

namespace Platform.Examples.Html
{
    /// <summary>
    /// Represents an HTML element as a link structure.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class HtmlElement<TLink>
    {
        /// <summary>
        /// Gets or sets the link identifier for this element.
        /// </summary>
        public TLink Id { get; set; }

        /// <summary>
        /// Gets or sets the tag name (e.g., "div", "p", "html").
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets whether this is an opening tag.
        /// </summary>
        public bool IsOpeningTag { get; set; }

        /// <summary>
        /// Gets or sets whether this is a self-closing tag.
        /// </summary>
        public bool IsSelfClosing { get; set; }

        /// <summary>
        /// Gets or sets the parent element link.
        /// </summary>
        public TLink Parent { get; set; }

        /// <summary>
        /// Gets or sets the text content (for text nodes).
        /// </summary>
        public string TextContent { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlElement{TLink}"/> class.
        /// </summary>
        public HtmlElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlElement{TLink}"/> class with the specified tag name.
        /// </summary>
        /// <param name="tagName">The tag name.</param>
        /// <param name="isOpeningTag">Whether this is an opening tag.</param>
        public HtmlElement(string tagName, bool isOpeningTag = true)
        {
            TagName = tagName;
            IsOpeningTag = isOpeningTag;
        }

        /// <summary>
        /// Returns a string representation of this HTML element.
        /// </summary>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(TextContent))
            {
                return $"Text: {TextContent}";
            }

            if (IsSelfClosing)
            {
                return $"<{TagName} />";
            }

            return IsOpeningTag ? $"<{TagName}>" : $"</{TagName}>";
        }
    }
}
