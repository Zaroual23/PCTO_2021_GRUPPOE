using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetlistNet;
using SetlistNet.Models;

namespace SaveConcert
{
    public class SetlistAPIManager : ISetlistAPIManager
    {
        private readonly SCSetlistApi sa = default(SCSetlistApi);

        public SetlistAPIManager(string apiToken)
        {
            sa = new SCSetlistApi(apiToken);
        }

        public Setlists Search(Setlist s)
        {
            Setlists result = sa.SearchSetlists(s);
            Setlists newResult;

            for (int i = 1; i < result.TotalPages; i++) {
                newResult = sa.SearchSetlists(s, i);
                foreach (Setlist j in newResult)
                {
                    result.Add(j);
                }
            }

            return result;
        }
    }
}
