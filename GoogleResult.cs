
using System;
namespace Escc.Search.Google
{
    /// <summary>
    /// A search result returned by Google Site Search
    /// </summary>
    public class GoogleResult : ISearchResult
    {
        /// <summary>
        /// Gets or sets the title, which can include HTML.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the excerpt, which can include HTML.
        /// </summary>
        /// <value>The excerpt.</value>
        public string Excerpt { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        public Uri Url { get; set; }
    }
}
