using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalindromeCheckClient
{
    public class FileDataItem
    {
        //private string id, text, similarityTPalString = "";
        public string Text { get; set; }
        public bool Procd { get; set; }
    }

    public class SimilarityTPalItem
    {
        //public int Index { get; set; }
        public string SimilarityTPalString { get; set; }
    }
}
