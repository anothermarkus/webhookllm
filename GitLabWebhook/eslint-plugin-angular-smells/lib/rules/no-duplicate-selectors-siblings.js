const htmlTags = new Set([
  'html', 'head', 'title', 'base', 'link', 'meta', 'style', 'script', 'noscript',
  'body', 'section', 'nav', 'article', 'aside', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
  'header', 'footer', 'address', 'main', 'p', 'hr', 'pre', 'blockquote', 'ol', 'ul',
  'li', 'dl', 'dt', 'dd', 'figure', 'figcaption', 'div', 'a', 'em', 'strong', 'small',
  's', 'cite', 'q', 'dfn', 'abbr', 'ruby', 'rt', 'rp', 'data', 'time', 'code', 'var',
  'samp', 'kbd', 'sub', 'sup', 'i', 'b', 'u', 'mark', 'bdi', 'bdo', 'span', 'br', 'wbr',
  'ins', 'del', 'picture', 'source', 'img', 'iframe', 'embed', 'object', 'param',
  'video', 'audio', 'track', 'map', 'area', 'table', 'caption', 'colgroup', 'col',
  'tbody', 'thead', 'tfoot', 'tr', 'td', 'th', 'form', 'fieldset', 'legend', 'label',
  'input', 'button', 'select', 'datalist', 'optgroup', 'option', 'textarea', 'output',
  'progress', 'meter', 'details', 'summary', 'dialog', 'script', 'template', 'canvas',
  'svg', 'math'
]);

module.exports = {
  create(context) {
    // Object to store tag counts at the current level
    const tagCounts = {};
    // Set to keep track of which components we've already reported for the current level
    const reportedTags = new Set();

    // This function identifies custom component selectors
    const isCustomComponent = (tagName) => {
      return !htmlTags.has(tagName.toLowerCase());
    };

    return {
      'Element$1': (node) => {
        const tagName = node.name;

        // Check if it's a custom component
        if (isCustomComponent(tagName)) {
          // Initialize the count for this tag name if it doesn't exist
          if (!tagCounts[tagName]) {
            tagCounts[tagName] = 0;
          }

          // Increment the count for this tag
          tagCounts[tagName]++;

          // Only report once for this tag if it's used more than once at the same level
          if (tagCounts[tagName] > 1 && !reportedTags.has(tagName)) {
            // Report the issue for this tag only once
            context.report({
              node,
              message: `The custom component "${tagName}" is used multiple times at the same level. Consider using "ngFor" to iterate over the component instances.`,
            });

            // Mark this tag as reported for the current level
            reportedTags.add(tagName);
          }
        }
      },

      // Handle other node types as needed (e.g., text nodes, etc.)
      Text$3(node) {
        // Optional: Adjust or suppress logging here
      }
    };
  },
};
