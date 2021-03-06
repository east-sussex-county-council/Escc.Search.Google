﻿
using System.Runtime.InteropServices;

namespace Escc.Search.Google
{
    /// <summary>
    /// A query to pass to Google Site Search
    /// </summary>
    public class GoogleQuery : ISearchQuery
    {
        private int _pageSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleQuery"/> class.
        /// </summary>
        /// <param name="queryText">The query text.</param>
        public GoogleQuery(string queryText)
        {
            this.QueryTerms = queryText;
            this.PageSize = 10; // optimal value-for-money for Google because it's the maximum number you can ask for in one request
            this.Page = 1;
        }

        /// <summary>
        /// Gets or sets the query text.
        /// </summary>
        /// <value>The query text.</value>
        public string QueryTerms { get; set; }

        /// <summary>
        /// Gets or sets the query terms to search within a set of results.
        /// </summary>
        /// <value>The query within results terms.</value>
        public string QueryWithinResultsTerms { get; set; }

        /// <summary>
        /// Gets or sets how many results are on each page (up to a maximum of 10).
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize { get { return _pageSize; } set { _pageSize = (value > 10 ? 10 : value); } }

        /// <summary>
        /// Gets or sets the page of results to fetch.
        /// </summary>
        /// <value>The page.</value>
        public int Page { get; set; }
    }
}
