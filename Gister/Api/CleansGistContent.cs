using System.Collections.Generic;
using System.Linq;

namespace EchelonTouchInc.Gister.Api
{
    public class CleansGistContent
    {
        public string Clean(string content)
        {
            Dictionary<string, string> itemsToBeCleaned = new Dictionary<string,string>
                                       {
                                           {"\t", "\\t"},
                                           {"\r", "\\r"},
                                           {"\n", "\\n"},
                                           {"\"", "\\\""},
                                       };
            return itemsToBeCleaned.Aggregate(content, (current, item) => current.Replace(item.Key, item.Value));
        }
    }
}