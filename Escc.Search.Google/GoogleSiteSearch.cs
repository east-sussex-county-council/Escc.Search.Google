
using System;
using System.Net;
using System.Web;
using Escc.Net;

namespace Escc.Search.Google
{
    /// <summary>
    /// The Google Site Search service, accessed using the XML API
    /// </summary>
    public class GoogleSiteSearch : ISearchService, ICacheableService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleSiteSearch"/> class.
        /// </summary>
        /// <param name="searchEngineId">The Google search engine id.</param>
        public GoogleSiteSearch(string searchEngineId)
        {
            this.SearchEngineId = searchEngineId;
        }

        /// <summary>
        /// Gets or sets the Google search engine id.
        /// </summary>
        /// <value>The search engine id.</value>
        public string SearchEngineId { get; set; }

        /// <summary>
        /// Runs a search query against Google Site Search
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public ISearchResponse Search(ISearchQuery query)
        {
            if (query == null) throw new ArgumentNullException("query");

            // Try to get the response from cache first. Saves re-querying Google, which saves money.
            if (this.CacheStrategy != null)
            {
                var cachedResponse = this.CacheStrategy.FetchCachedResponse(query);
                if (cachedResponse != null) return new GoogleResponse(cachedResponse);
            }

            // Don't use gl=uk parameter as it affects the order of results, and not necessarily in a good way. 
            // Its purpose is to boost UK results above those from other countries, but since all our results are UK results
            // we shouldn't need that. Example: "age well" returns a 2006 consultation above the current page with this option on.
            //
            // Google says "Google will only return spelling suggestions for queries where the gl parameter value is in lowercase letters", 
            // but that turns out not to be true, as excluding the parameter still returns spelling suggestions.
            // http://code.google.com/intl/en/apis/customsearch/docs/xml_results.html#results_xml_tag_Spelling
            // 
            string url = "http://www.google.com/search?cx=" + HttpUtility.UrlEncode(this.SearchEngineId) +
                "&q=" + HttpUtility.UrlEncode(query.QueryTerms) +
                "&as_q=" + HttpUtility.UrlEncode(query.QueryWithinResultsTerms) +
                "&start=" + ((query.Page - 1) * query.PageSize) +
                "&num=" + query.PageSize +
                "&client=google-csbe&output=xml_no_dtd&ie=utf8&oe=utf8&hl=en";

            // Make a fresh request to Google for search results. Use gzip for speed.
            var client = new HttpRequestClient(new ConfigurationProxyProvider());
            var request = client.CreateRequest(new Uri(url));
            request.AutomaticDecompression = DecompressionMethods.GZip;
            var response = new GoogleResponse(client.RequestXPath(request));

            // Cache the response if possible before returning it
            if (this.CacheStrategy != null)
            {
                this.CacheStrategy.CacheResponse(query, response);
            }

            return response;
        }

        /// <summary>
        /// Gets or sets the cache strategy.
        /// </summary>
        /// <value>The cache strategy.</value>
        public ICacheStrategy CacheStrategy { get; set; }
    }
}
