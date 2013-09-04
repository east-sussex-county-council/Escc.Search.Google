Escc.Search.Google
==================

Google Custom Search provider for a search results page which uses interfaces from the Escc.Search namespace. 

Example code:

```c#
var service = new GoogleSiteSearch("your search engine id");
service.CacheStrategy = new FileCacheStrategy(Server.MapPath("~/App_Data"), new TimeSpan(24, 0, 0));
var query = new GoogleQuery(Request.QueryString["q"]);
query.PageSize = 20;
query.Page = 1;

var response = service.Search(query);
var searchResults = response.Results();
this.results.DataSource = searchResults;
this.results.DataBind();
```