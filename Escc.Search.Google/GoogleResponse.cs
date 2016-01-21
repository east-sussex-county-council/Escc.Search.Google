
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Xml.XPath;
namespace Escc.Search.Google
{
    /// <summary>
    /// A response from Google Site Search containing results and associated metadata
    /// </summary>
    public class GoogleResponse : ISearchResponse, ICacheableResponse
    {
        private IXPathNavigable responseXml;
        private IList<ISearchResult> resultsList;
        private IList<string> spellingSuggestions;
        private int? totalResults;
        private bool resultsAvailable;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleResponse"/> class.
        /// </summary>
        /// <param name="responseXml">The response XML.</param>
        public GoogleResponse(string responseXml)
        {
            if (String.IsNullOrEmpty(responseXml)) throw new ArgumentNullException("responseXml");
            using (var reader = new StringReader(responseXml))
            {
                this.responseXml = new XPathDocument(reader);
            }
            this.resultsAvailable = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleResponse"/> class.
        /// </summary>
        /// <param name="responseXml">The response XML.</param>
        public GoogleResponse(IXPathNavigable responseXml)
        {
            if (responseXml == null) throw new ArgumentNullException("responseXml");
            this.responseXml = responseXml;
            this.resultsAvailable = true;
        }

        /// <summary>
        /// XML response data returned by Google
        /// </summary>
        /// <returns></returns>
        public string RawData()
        {
            var nav = this.responseXml.CreateNavigator();
            return nav.OuterXml;
        }

        /// <summary>
        /// Gets or sets the total number of results.
        /// </summary>
        /// <value>The total results.</value>
        public int TotalResults
        {
            get
            {
                // Only read the total from the XML once
                if (!this.totalResults.HasValue)
                {
                    var nav = this.responseXml.CreateNavigator();
                    nav.MoveToRoot();
                    var it = nav.Select("/GSP/RES/M");
                    if (it.MoveNext())
                    {
                        try
                        {
                            this.totalResults = Int32.Parse(it.Current.InnerXml, CultureInfo.CurrentCulture);
                        }
                        catch (FormatException)
                        {
                            this.totalResults = 0;
                        }
                        catch (OverflowException)
                        {
                            this.totalResults = 0;
                        }
                    }
                    else
                    {
                        // Google only returns the first 1000 results. If you try to view lower ones, this happens.
                        // The returned XML is exactly the same when no results are found, therefore the only way to
                        // distinguish the two scenarios is to see whether the request was for the first page of results.
                        this.totalResults = 0;

                        it = nav.Select("/GSP/PARAM[@name='start']");
                        if (it.MoveNext())
                        {
                            try
                            {
                                var start = Int32.Parse(it.Current.GetAttribute("value", String.Empty), CultureInfo.CurrentCulture);
                                if (start >= 1000)
                                {
                                    this.resultsAvailable = false;
                                }
                            }
                            catch (FormatException)
                            {
                                // unlikely that Google XML is malformed, but treat as if element not there: leave as 'no results found'
                            }
                            catch (OverflowException)
                            {
                                // the page number's very bit, so results not available
                                this.resultsAvailable = false;
                            }
                        }
                    }
                }
                return this.totalResults.Value;
            }
        }

        /// <summary>
        /// Google only returns the first 1000 results. Lower-ranked results are valid, but not available.
        /// </summary>
        /// <value><c>true</c> if results are available; <c>false</c> otherwise.</value>
        public bool ResultsAvailable { get { return this.resultsAvailable; } }

        /// <summary>
        /// Gets the results of the Google search
        /// </summary>
        /// <returns></returns>
        public IList<ISearchResult> Results()
        {
            // Only parse the XML once
            if (this.resultsList == null)
            {
                this.resultsList = new List<ISearchResult>();

                // Get the results from the XML
                var nav = this.responseXml.CreateNavigator();
                nav.MoveToRoot();
                var it = nav.Select("/GSP/RES/R");
                while (it.MoveNext())
                {
                    var result = new GoogleResult();

                    // Get the title of the page
                    var node = it.Current.SelectSingleNode("T");
                    if (node != null) result.Title = HttpUtility.HtmlDecode(node.InnerXml);

                    // Get the excerpt of the page
                    node = it.Current.SelectSingleNode("S");
                    if (node != null) result.Excerpt = HttpUtility.HtmlDecode(node.InnerXml);

                    // Get the URL
                    node = it.Current.SelectSingleNode("U");
                    if (node != null) result.Url = new Uri(node.InnerXml);

                    this.resultsList.Add(result);
                }
            }

            return this.resultsList;
        }

        /// <summary>
        /// Gets spelling suggestions for the supplied search term.
        /// </summary>
        /// <returns></returns>
        public IList<string> SpellingSuggestions()
        {
            // Only parse the XML once
            if (this.spellingSuggestions == null)
            {
                this.spellingSuggestions = new List<string>();

                // Get the results from the XML
                var nav = this.responseXml.CreateNavigator();
                nav.MoveToRoot();
                var it = nav.Select("/GSP/Spelling/Suggestion");
                while (it.MoveNext())
                {
                    // Get the URL encoded version, because that way it's easy to get just the search term without Google's formatting
                    this.spellingSuggestions.Add(HttpUtility.UrlDecode(it.Current.GetAttribute("q", String.Empty)));
                }
            }

            return this.spellingSuggestions;
        }
    }
}
