
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Escc.Net;

namespace Escc.Search.Google
{
    /// <summary>
    /// The Google Customer Search service, accessed using the JSON API
    /// </summary>
    public class GoogleCustomSearch : ISearchService, ICacheableService
    {
        private readonly IProxyProvider _proxyProvider;
        private static HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleCustomSearch" /> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="searchEngineId">The Google search engine id.</param>
        /// <param name="proxyProvider">The provider for proxy details for making the request to the API.</param>
        public GoogleCustomSearch(string apiKey, string searchEngineId, IProxyProvider proxyProvider)
        {
            _proxyProvider = proxyProvider;
            ApiKey = apiKey;
            SearchEngineId = searchEngineId;
        }

        /// <summary>
        /// Gets or sets the Google search engine id.
        /// </summary>
        /// <value>The search engine id.</value>
        public string SearchEngineId { get; set; }

        /// <summary>
        /// Gets or sets the API key for the Google Custom Search JSON API.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Runs a search query against Google Site Search
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public async Task<ISearchResponse> SearchAsync(ISearchQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            // Try to get the response from cache first. Saves re-querying Google, which saves money.
            var cachedResponse = this.CacheStrategy?.FetchCachedResponse(query);
            if (cachedResponse != null) return new GoogleResponse(cachedResponse);

            // Don't use gl=uk parameter as it affects the order of results, and not necessarily in a good way. 
            // Its purpose is to boost UK results above those from other countries, but since all our results are UK results
            // we shouldn't need that. Example: "age well" returns a 2006 consultation above the current page with this option on.
            //
            // Google says "Google will only return spelling suggestions for queries where the gl parameter value is in lowercase letters", 
            // but that turns out not to be true, as excluding the parameter still returns spelling suggestions.
            // http://code.google.com/intl/en/apis/customsearch/docs/xml_results.html#results_xml_tag_Spelling
            // 
            string url = "https://www.googleapis.com/customsearch/v1?cx=" + HttpUtility.UrlEncode(SearchEngineId) +
                "&key=" + HttpUtility.UrlEncode(ApiKey) +
                "&q=" + HttpUtility.UrlEncode(query.QueryTerms) +
                "&hq=" + HttpUtility.UrlEncode(query.QueryWithinResultsTerms) +
                "&start=" + (((query.Page - 1) * query.PageSize)+1) +
                "&num=" + query.PageSize +
                "&fields=queries(nextPage,previousPage),searchInformation,spelling(correctedQuery),items(title,link,htmlSnippet,htmlFormattedUrl)&hl=en";

            // Make a fresh request to Google for search results. 
            if (_httpClient == null)
            {
                _httpClient = new HttpClient(new HttpClientHandler()
                {
                    Proxy = _proxyProvider?.CreateProxy()
                });
            }
            var response = new GoogleResponse(await _httpClient.GetStringAsync(url).ConfigureAwait(false));

            // Cache the response if possible before returning it
            this.CacheStrategy?.CacheResponse(query, response);

            return response;
        }

        /// <summary>
        /// Gets or sets the cache strategy.
        /// </summary>
        /// <value>The cache strategy.</value>
        public ICacheStrategy CacheStrategy { get; set; }
    }
}
