using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SetlistNet.Models;

namespace SaveConcert
{
    public interface ISetlistAPIManager
    {
        Setlists Search(Setlist s);
    }
}
