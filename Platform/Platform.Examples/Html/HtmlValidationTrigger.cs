using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Examples.Triggers;

namespace Platform.Examples.Html
{
    /// <summary>
    /// Trigger for validating HTML structure using a stack-based approach.
    /// </summary>
    /// <typeparam name="TLink">The type of link identifier.</typeparam>
    public class HtmlValidationTrigger<TLink> : TriggerBase<TLink>
    {
        private const string ElementsKey = "Elements";
        private const string TagStackKey = "TagStack";
        private const string ErrorsKey = "Errors";

        /// <summary>
        /// Gets the name of this trigger.
        /// </summary>
        public override string Name => "HtmlValidationTrigger";

        /// <summary>
        /// Processes the context by validating HTML structure.
        /// </summary>
        /// <param name="context">The trigger context.</param>
        /// <returns>True if HTML structure is valid; otherwise, false.</returns>
        public override bool Process(TriggerContext<TLink> context)
        {
            if (!context.Data.ContainsKey(ElementsKey))
            {
                context.IsValid = false;
                AddError(context, "No HTML elements to validate");
                return false;
            }

            var elements = context.Data[ElementsKey] as List<HtmlElement<TLink>>;
            if (elements == null || elements.Count == 0)
            {
                context.IsValid = false;
                AddError(context, "Elements list is empty or invalid");
                return false;
            }

            var tagStack = new Stack<HtmlElement<TLink>>();
            context.Data[TagStackKey] = tagStack;

            foreach (var element in elements)
            {
                if (!string.IsNullOrEmpty(element.TextContent))
                {
                    // Text nodes don't affect validation
                    continue;
                }

                if (element.IsSelfClosing)
                {
                    // Self-closing tags are always valid
                    continue;
                }

                if (element.IsOpeningTag)
                {
                    // Push opening tag onto stack
                    tagStack.Push(element);
                }
                else
                {
                    // Closing tag - must match top of stack
                    if (tagStack.Count == 0)
                    {
                        context.IsValid = false;
                        AddError(context, $"Unexpected closing tag </{element.TagName}> with no matching opening tag");
                        return false;
                    }

                    var openingTag = tagStack.Pop();
                    if (!string.Equals(openingTag.TagName, element.TagName, StringComparison.OrdinalIgnoreCase))
                    {
                        context.IsValid = false;
                        AddError(context, $"Mismatched tags: expected </{openingTag.TagName}> but found </{element.TagName}>");
                        return false;
                    }
                }
            }

            // Check for unclosed tags
            if (tagStack.Count > 0)
            {
                context.IsValid = false;
                var unclosedTags = string.Join(", ", tagStack.Select(t => t.TagName));
                AddError(context, $"Unclosed tags: {unclosedTags}");
                return false;
            }

            context.IsValid = true;
            return TransferToNext(context);
        }

        private void AddError(TriggerContext<TLink> context, string error)
        {
            if (!context.Data.ContainsKey(ErrorsKey))
            {
                context.Data[ErrorsKey] = new List<string>();
            }

            var errors = context.Data[ErrorsKey] as List<string>;
            errors?.Add(error);
        }
    }
}
