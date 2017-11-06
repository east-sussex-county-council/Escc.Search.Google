using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.Search.Google
{
    public class Rootobject
    {
        public Queries queries { get; set; }
        public Searchinformation searchInformation { get; set; }
        public Item[] items { get; set; }

        public Spelling spelling { get; set; }
    }

    public class Queries
    {
        public Nextpage[] nextPage { get; set; }
    }

    public class Nextpage
    {
        public string title { get; set; }
        public string totalResults { get; set; }
        public string searchTerms { get; set; }
        public int count { get; set; }
        public int startIndex { get; set; }
        public string inputEncoding { get; set; }
        public string outputEncoding { get; set; }
        public string safe { get; set; }
        public string cx { get; set; }
    }

    public class Searchinformation
    {
        public float searchTime { get; set; }
        public string formattedSearchTime { get; set; }
        public string totalResults { get; set; }
        public string formattedTotalResults { get; set; }
    }

    public class Spelling
    {
        public string correctedQuery { get; set; }
    }

    public class Item
    {
        public string title { get; set; }
        public string link { get; set; }
        public string htmlSnippet { get; set; }
        public string htmlFormattedUrl { get; set; }
    }
}
