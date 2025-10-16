// LinksPlatform Chrome Extension - Omnibox Integration
// Allows searching LinksPlatform resources using "lp" keyword in address bar

// Default search action when user types "lp <query>"
chrome.omnibox.onInputEntered.addListener((text, disposition) => {
  let url;

  // Parse search modifiers
  if (text.startsWith('repo ') || text.startsWith('r ')) {
    // Search repositories
    const query = text.replace(/^(repo|r)\s+/, '');
    url = `https://github.com/search?q=org%3Alinksplatform+${encodeURIComponent(query)}+in%3Aname&type=repositories`;
  } else if (text.startsWith('issue ') || text.startsWith('i ')) {
    // Search issues and PRs
    const query = text.replace(/^(issue|i)\s+/, '');
    url = `https://github.com/search?q=org%3Alinksplatform+org%3Akonard+${encodeURIComponent(query)}&type=issues`;
  } else if (text.startsWith('docs ') || text.startsWith('d ')) {
    // Search documentation
    const query = text.replace(/^(docs|d)\s+/, '');
    url = `https://www.google.com/search?q=site%3Alinksplatform.github.io+${encodeURIComponent(query)}`;
  } else if (text.startsWith('code ') || text.startsWith('c ')) {
    // Search code
    const query = text.replace(/^(code|c)\s+/, '');
    url = `https://github.com/search?q=org%3Alinksplatform+org%3Akonard+${encodeURIComponent(query)}&type=code`;
  } else {
    // Default: search code across all LinksPlatform repos
    url = `https://github.com/search?q=org%3Alinksplatform+org%3Akonard+${encodeURIComponent(text)}&type=code`;
  }

  // Open in current tab, new tab, or new window based on user action
  switch (disposition) {
    case "currentTab":
      chrome.tabs.update({url});
      break;
    case "newForegroundTab":
      chrome.tabs.create({url});
      break;
    case "newBackgroundTab":
      chrome.tabs.create({url, active: false});
      break;
  }
});

// Provide search suggestions as user types
chrome.omnibox.onInputChanged.addListener((text, suggest) => {
  const suggestions = [];

  // Add context-aware suggestions
  if (!text.match(/^(repo|r|issue|i|docs|d|code|c)\s/)) {
    suggestions.push({
      content: `code ${text}`,
      description: `Search <match>code</match> for: ${escapeXml(text)}`
    });
    suggestions.push({
      content: `repo ${text}`,
      description: `Search <match>repositories</match> for: ${escapeXml(text)}`
    });
    suggestions.push({
      content: `issue ${text}`,
      description: `Search <match>issues</match> for: ${escapeXml(text)}`
    });
    suggestions.push({
      content: `docs ${text}`,
      description: `Search <match>documentation</match> for: ${escapeXml(text)}`
    });
  }

  suggest(suggestions);
});

// Set default suggestion text
chrome.omnibox.setDefaultSuggestion({
  description: 'Search LinksPlatform (prefix: repo/r, issue/i, docs/d, code/c)'
});

// Helper function to escape XML for suggestion descriptions
function escapeXml(text) {
  return text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}
