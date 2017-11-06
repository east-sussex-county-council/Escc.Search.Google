
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace Escc.Search.Google
{
    /// <summary>
    /// A response from Google Site Search containing results and associated metadata
    /// </summary>
    public class GoogleResponse : ISearchResponse, ICacheableResponse
    {
        private readonly string _responseJson;
        private readonly Rootobject _apiResponse;
        private IList<ISearchResult> _resultsList;
        private IList<string> _spellingSuggestions;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleResponse"/> class.
        /// </summary>
        /// <param name="responseJson">The response XML.</param>
        public GoogleResponse(string responseJson)
        {
            if (String.IsNullOrEmpty(responseJson)) throw new ArgumentNullException(nameof(responseJson));
            _responseJson = responseJson;
            this._apiResponse = JsonConvert.DeserializeObject<Rootobject>(responseJson);
            this.ResultsAvailable = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleResponse"/> class.
        /// </summary>
        /// <param name="apiResponse">The response XML.</param>
        public GoogleResponse(Rootobject apiResponse)
        {
            if (apiResponse == null) throw new ArgumentNullException(nameof(apiResponse));
            this._apiResponse = apiResponse;
            this.ResultsAvailable = true;
        }

        /// <summary>
        /// JSON response data returned by Google
        /// </summary>
        /// <returns></returns>
        public string RawData()
        {
            return _responseJson;
        }

        /// <summary>
        /// Gets or sets the total number of results.
        /// </summary>
        /// <value>The total results.</value>
        public int TotalResults => Int32.Parse(_apiResponse.searchInformation.totalResults, CultureInfo.InvariantCulture);

        /// <summary>
        /// Google only returns the first 100 results. Lower-ranked results are valid, but not available.
        /// </summary>
        /// <value><c>true</c> if results are available; <c>false</c> otherwise.</value>
        public bool ResultsAvailable { get; }

        /// <summary>
        /// Gets the results of the Google search
        /// </summary>
        /// <returns></returns>
        public IList<ISearchResult> Results()
        {
            // Only format into objects once
            if (this._resultsList == null)
            {
                this._resultsList = new List<ISearchResult>();

                if (_apiResponse.items != null)
                {
                    foreach (var item in _apiResponse.items)
                    {
                        var result = new GoogleResult();

                        // Get the title of the page
                        result.Title = item.title;

                        // Get the excerpt of the page
                        result.Excerpt = item.htmlSnippet;
                        result.Excerpt = result.Excerpt?.Replace("\n", String.Empty);
                        result.Excerpt = result.Excerpt?.Replace("<br>", String.Empty);

                        // Get the URL
                        result.Url = new Uri(item.link);

                        this._resultsList.Add(result);
                    }
                }
            }

            return this._resultsList;
        }

        /// <summary>
        /// Gets spelling suggestions for the supplied search term.
        /// </summary>
        /// <returns></returns>
        public IList<string> SpellingSuggestions()
        {
            // Only build the data object once
            if (this._spellingSuggestions == null)
            {
                this._spellingSuggestions = new List<string>();
                if (_apiResponse != null && _apiResponse.spelling != null && !String.IsNullOrEmpty(_apiResponse.spelling.correctedQuery))
                {
                    _spellingSuggestions.Add(_apiResponse.spelling.correctedQuery);
                }
            }

            return this._spellingSuggestions;
        }
    }
}
