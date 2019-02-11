using System;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Escc.Search.Google.Tests
{
    [TestFixture]
    public class GoogleResponseTests
    {
        [Test]
        public void TotalResultsIsParsed()
        {
            var response = new GoogleResponse(SampleResponses.ExampleResponseSelectedFields);

            var totalResults = response.TotalResults;

            Assert.AreEqual(194, totalResults);
        }

        [Test]
        public void FirstResultIsParsed()
        {
            var response = new GoogleResponse(SampleResponses.ExampleResponseSelectedFields);

            var results = response.Results();

            Assert.AreEqual("Videos of council meetings – East Sussex County Council", results[0].Title);
            Assert.AreEqual("https://www.eastsussex.gov.uk/yourcouncil/webcasts/", results[0].Url.ToString());
            Assert.AreEqual("<b>Videos</b> of council meetings. Webcasts of meetings are available online for six months after the date of the meeting. They include index points that allow you to&nbsp;...", results[0].Excerpt);
        }

        [Test]
        public void SpellingSuggestionIsParsed()
        {
            var response = new GoogleResponse(SampleResponses.SpellingSuggestion);

            var results = response.SpellingSuggestions();

            Assert.AreEqual("education", results[0]);
        }
    }
}
