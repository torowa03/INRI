using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    class SiteMap
    {
        public string DisplayName { set; get; }
        public string Url { get; set; }
        public int Chapter { get; set; }

        public static List<SiteMap> Get()
        {
            List<SiteMap> lst = new List<SiteMap>();
            lst.Add(new SiteMap { DisplayName = "創世記", Url = "", Chapter = 10 });
            lst.Add(new SiteMap { DisplayName = "出エジプト記", Url = "", Chapter = 8 });



            return lst;
        }


    }

    class BibleBook
    {
        public string Testament { set; get; }
        public string BookName { get; set; }
        public string Url { get; set; }
        public int Chapter { get; set; }
        //public List<string> ChapterUrl { get; set; }

        //public BibleBook()
        //{
        //    Testament = string.Empty;
        //    BookName = string.Empty;
        //    Url = string.Empty;
        //    ChapterUrl = new List<string>();

        //}

        //public void AddChapterUrl(string url)
        //{
        //    ChapterUrl.Add(url);
        //}

    }

    class BibleChapter
    {
        public string Testament { set; get; }
        public string BookName { get; set; }
        public string Chapter { set; get; }
        public string Url { get; set; }

    }

    class BibleParagraph
    {
        public string Testament { set; get; }
        public string BookName { get; set; }
        public string Chapter { set; get; }
        public string Section { set; get; }
        public string Paragraph { set; get; }

    }


}
