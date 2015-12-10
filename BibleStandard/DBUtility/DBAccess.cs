using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;

using System.Collections.ObjectModel;
using Inri.Common;
using Data;

namespace DBUtility
{
    public class DBConst
    {
        public static string DB_Path = "DB";
        public static string DB_File = "bible.db";
    }


    /// <summary>
    /// 汎用データアクセスオブジェクト
    /// </summary>
    class DBAccess
    {
        #region 定数

        //DBフォルダ
        private string DB_PATH;


        #endregion

        #region 変数


        /// <summary>
        /// SQLite接続クラス
        /// </summary>
        private SQLiteAdapter _SQLAdp;


        /// <summary>
        /// DBファイルパス
        /// </summary>
        //string _dbPath;

        #endregion

        public DBAccess(string dbPath)
        {
            if (System.IO.File.Exists(dbPath) == false) return;

            DB_PATH = dbPath;

        }

        public static void ClearDB()
        {
            string dbpath = System.IO.Path.Combine(StaticTools.GetAssembyPath(), DBConst.DB_Path, DBConst.DB_File);

            DBAccess da = new DBAccess(dbpath);
            da.AllDeleteTable();

        }

        public static string GetDBPath()
        {
            return System.IO.Path.Combine(StaticTools.GetAssembyPath(), DBConst.DB_Path, DBConst.DB_File);
            //Uri path = new Uri("/DB/bible.db", UriKind.Relative);
            //return path.AbsolutePath;
        }


        /// <summary>
        /// テーブルにレコードを追加する
        /// </summary>
        /// <param name="table"></param>
        /// <param name="data"></param>
        public static void Add(string table, string data)
        {
            string dbpath = System.IO.Path.Combine(StaticTools.GetAssembyPath(), DBConst.DB_Path, DBConst.DB_File);

            DBAccess da = new DBAccess(dbpath);
            string[] vals = data.Split(',');
            da.__insertRecord(table, vals);

        }

        /// <summary>
        /// 書巻リストを取得
        /// </summary>
        /// <returns></returns>
        public static List<BibleBook> ReadBook()
        {
            string dbpath =  DBAccess.GetDBPath(); // System.IO.Path.Combine(StaticTools.GetAssembyPath(), DBConst.DB_Path, DBConst.DB_File);
            DBAccess da = new DBAccess(dbpath);
            return da.ReadTestament();

        }

        /// <summary>
        /// 章リストを取得
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public static List<BibleChapter> ReadChapter(string book)
        {
            string dbpath = DBAccess.GetDBPath();
            DBAccess da = new DBAccess(dbpath);
            return da.__readChapter(book);

        }

        /// <summary>
        /// 節リストを取得
        /// </summary>
        /// <param name="book"></param>
        /// <param name="chapter"></param>
        /// <returns></returns>
        public static List<BibleParagraph> ReadParagraph(string book, string chapter)
        {
            string dbpath = DBAccess.GetDBPath();
            DBAccess da = new DBAccess(dbpath);
            return da.__readScripture(book, chapter);

        }

        /// <summary>
        /// 節をキーワード検索する
        /// </summary>
        /// <param name="word">キーワード</param>
        /// <returns></returns>
        public static List<BibleParagraph> SearchParagraph(string word)
        {
            string dbpath = DBAccess.GetDBPath();
            DBAccess da = new DBAccess(dbpath);
            return da.__searchScripture(word);

        }


        /// <summary>
        /// 節を登録
        /// </summary>
        /// <param name="book"></param>
        /// <param name="chapter"></param>
        /// <param name="datas"></param>
        public static void AddScripture(string book, string chapter, List<string> datas)
        {
            string dbpath = DBAccess.GetDBPath();
            DBAccess da = new DBAccess(dbpath);

            int cnt = 0;
            foreach(string s in datas)
            {
                cnt++;
                //string section = string.Format("{0}節", cnt);
                string[] buf = s.Split(' ');
                string section = buf[0].Trim();
                string para = buf[1];
                if(buf.Length > 2)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("節の分解 -- {0}  {1} {2}", buf.Length, book, chapter));
                    para = s.Substring(buf[0].Length + 1);
                    System.Diagnostics.Debug.WriteLine(s);
                    System.Diagnostics.Debug.WriteLine(para);
                }

                string data = string.Format("{0},{1},{2},{3},{4},{5}", "新改訳", book, chapter,section, para,string.Empty);
                string[] vals = data.Split(',');
                da.__insertRecord("Scripture", vals);
            }

            //章レコードの更新
            string where = string.Format("where BookId = '{0}'",book);
            da.__updateRecord("Chapter", "Local", "1", where);
        }

        /// <summary>
        /// ランダムに節を取得
        /// </summary>
        /// <returns></returns>
        public static BibleParagraph RandomScripture()
        {
            string dbpath = DBAccess.GetDBPath();
            DBAccess da = new DBAccess(dbpath);

            return da.__readRandomScripture();
        }



        /// <summary>
        /// レコード追加
        /// </summary>
        /// <param name="vals"></param>
        public void __insertRecord(string table, string[] vals)
        {
            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("insert into {0} values(",table));
                sb.AppendLine("'" + string.Join("','", vals) + "'");
                sb.AppendLine(")");

                try
                {
                    _SQLAdp.ExecuteNonQuery(sb.ToString());
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// レコードの更新
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="val"></param>
        public void __updateRecord(string table, string column,  string val, string where)
        {
            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("update {0} set {1} = '{2}'", table, column, val));
                if(where != string.Empty)
                {
                    sb.AppendLine(where);
                }

                try
                {
                    _SQLAdp.ExecuteNonQuery(sb.ToString());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }



        /// <summary>
        /// 書類一覧の読み込み
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public List<BibleBook> ReadTestament()
        {
            List<BibleBook> lst = new List<BibleBook>();

            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();
                //sb.AppendLine("select");
                //sb.AppendLine(" Name,Url");
                //sb.AppendLine(" from Book ");
                sb.AppendLine("select ");
                sb.AppendLine("b.Name,");
                sb.AppendLine("b.Url,");
                sb.AppendLine("coalesce(c.cnt,0) as Chapter");
                sb.AppendLine("from Book b");
                sb.AppendLine("left join ");
                sb.AppendLine("(");
                sb.AppendLine("select");
                sb.AppendLine("BookId, ");
                sb.AppendLine("count(*) as cnt");
                sb.AppendLine("from Chapter");
                sb.AppendLine("group by BookId");
                sb.AppendLine(") c");
                sb.AppendLine("on ");
                sb.AppendLine("b.Name = c.BookId");
                sb.AppendLine("order by b.sortorder");


                DataSet ds = new DataSet();
                ds = _SQLAdp.GetDataset(ds, "table", sb.ToString(), new object[] { });
                if (ds != null)
                {
                    foreach(DataRow r in ds.Tables[0].Rows)
                    {
                        BibleBook bb = new BibleBook();
                        bb.BookName = r["Name"].ToString();
                        bb.Url = r["Url"].ToString();
                        bb.Chapter = Convert.ToInt16(r["Chapter"].ToString());
                        lst.Add(bb);               
                    }
                }

            }

            return lst;

        }

        /// <summary>
        /// 章一覧の読み込み
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public List<BibleChapter> __readChapter(string book)
        {
            List<BibleChapter> lst = new List<BibleChapter>();

            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select");
                sb.AppendLine(" Chapter, Url");
                sb.AppendLine(" from Chapter ");
                sb.AppendLine(string.Format(" where bookid = '{0}'", book));

                DataSet ds = new DataSet();
                ds = _SQLAdp.GetDataset(ds, "table", sb.ToString(), new object[] { });
                if (ds != null)
                {
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        BibleChapter cp = new BibleChapter();
                        cp.BookName = book;
                        cp.Chapter = r["Chapter"].ToString();
                        cp.Url = r["Url"].ToString();
                        lst.Add(cp);
                    }
                }

            }

            return lst;

        }

        /// <summary>
        /// 節の読み込み
        /// </summary>
        /// <param name="book"></param>
        /// <param name="chapter"></param>
        /// <returns></returns>
        public List<BibleParagraph> __readScripture(string book, string chapter)
        {
            var lst = new List<BibleParagraph>();

            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select");
                sb.AppendLine(" Section, Paragraph");
                sb.AppendLine(" from Scripture ");
                sb.AppendLine(string.Format(" where BookId = '{0}'", book));
                sb.AppendLine(string.Format(" and Chapter = '{0}'", chapter));
                sb.AppendLine("order by cast(section as BIGINT) asc");

                DataSet ds = new DataSet();
                ds = _SQLAdp.GetDataset(ds, "table", sb.ToString(), new object[] { });
                if (ds != null)
                {
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        var pg = new BibleParagraph();
                        pg.BookName = book;
                        pg.Chapter = chapter;
                        pg.Section = r["Section"].ToString();
                        pg.Paragraph = r["Paragraph"].ToString();
                        lst.Add(pg);
                    }
                }

            }

            return lst;

        }

        /// <summary>
        /// 節の検索
        /// </summary>
        /// <param name="word">キーワード</param>
        /// <returns></returns>
        public List<BibleParagraph> __searchScripture(string word)
        {
            var lst = new List<BibleParagraph>();

            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select ans.*, b.SortOrder from");
                sb.AppendLine("(");
                sb.AppendLine("select ");
                sb.AppendLine("BookId, Chapter, Section, Paragraph");
                sb.AppendLine("from Scripture");
                sb.AppendLine("where");
                sb.AppendLine(string.Format("Paragraph like '%{0}%' ",word));
                sb.AppendLine(") ans");
                sb.AppendLine("left join Book as b");
                sb.AppendLine("on ans.BookId = b.Name");
                sb.AppendLine("order by cast(b.SortOrder as BIGINT), cast(Chapter as BIGINT), cast(Section as BIGINT)");

                DataSet ds = new DataSet();
                ds = _SQLAdp.GetDataset(ds, "table", sb.ToString(), new object[] { });
                if (ds != null)
                {
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        var pg = new BibleParagraph();
                        pg.BookName = r["BookId"].ToString();
                        pg.Chapter = r["Chapter"].ToString();
                        pg.Section = r["Section"].ToString();
                        pg.Paragraph = r["Paragraph"].ToString().Replace("\r","");
                        lst.Add(pg);
                    }
                }

            }

            return lst;

        }

        /// <summary>
        /// ランダムに１件、節を取得する
        /// </summary>
        /// <returns></returns>
        public BibleParagraph __readRandomScripture()
        {
            var pg = new BibleParagraph();

            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select");
                sb.AppendLine(" *");
                sb.AppendLine(" from Scripture ");
                sb.AppendLine("order by RANDOM() limit 1");

                DataSet ds = new DataSet();
                ds = _SQLAdp.GetDataset(ds, "table", sb.ToString(), new object[] { });
                if (ds != null)
                {
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        pg.BookName = r["BookId"].ToString();
                        pg.Chapter = r["Chapter"].ToString();
                        pg.Section = r["Section"].ToString();
                        pg.Paragraph = r["Paragraph"].ToString();

                    }
                }

            }
            return pg;
        }


        /// <summary>
        /// 取り込み時に詩篇が分割されているため元にもどす
        /// </summary>
        /// <param name="table"></param>
        public void __fixDB()
        {
            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("update Chapter");
                sb.AppendLine("set BookId = '詩篇', Chapter = Chapter + 41");
                sb.AppendLine("where BookId = '詩篇2';");

                sb.AppendLine("update Chapter");
                sb.AppendLine("set BookId = '詩篇', Chapter = Chapter + 41 + 31");
                sb.AppendLine("where BookId = '詩篇3';");

                sb.AppendLine("update Chapter");
                sb.AppendLine("set BookId = '詩篇', Chapter = Chapter + 41 + 31 + 17");
                sb.AppendLine("where BookId = '詩篇4';");

                sb.AppendLine("update Chapter");
                sb.AppendLine("set BookId = '詩篇', Chapter = Chapter + 41 + 31 + 17 + 17");
                sb.AppendLine("where BookId = '詩篇5';");

                sb.AppendLine("update Scripture");
                sb.AppendLine("set BookId = '詩篇', Chapter = Chapter + 41");
                sb.AppendLine("where BookId = '詩篇2';");

                sb.AppendLine("update Scripture");
                sb.AppendLine("set BookId = '詩篇', Chapter = Chapter + 41 + 31");
                sb.AppendLine("where BookId = '詩篇3';");

                sb.AppendLine("update Scripture");
                sb.AppendLine("set BookId = '詩篇', Chapter = Chapter + 41 + 31 + 17");
                sb.AppendLine("where BookId = '詩篇4';");

                sb.AppendLine("update Scripture");
                sb.AppendLine("set BookId = '詩篇', Chapter = Chapter + 41 + 31 + 17 + 17");
                sb.AppendLine("where BookId = '詩篇5';");

                sb.AppendLine("delete from Book");
                sb.AppendLine("where Name='詩篇2';");

                sb.AppendLine("delete from Book");
                sb.AppendLine("where Name='詩篇3';");

                sb.AppendLine("delete from Book");
                sb.AppendLine("where Name='詩篇4';");

                sb.AppendLine("delete from Book");
                sb.AppendLine("where Name='詩篇5';");


                try
                {
                    _SQLAdp.ExecuteNonQuery(sb.ToString());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

            }
        }




        /// <summary>
        /// クリア
        /// </summary>
        public void AllDeleteTable()
        {
            DeleteTable("Scripture");
            DeleteTable("Chapter");
            DeleteTable("Book");

        }

        /// <summary>
        /// テーブルのクリア
        /// </summary>
        /// <param name="table"></param>
        public void DeleteTable(string table)
        {
            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("delete from {0}",table));

                try
                {
                    _SQLAdp.ExecuteNonQuery(sb.ToString());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

            }
        }




        /// <summary>
        /// テーブルの件数チェック
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int TableRecordCount(string tbl)
        {
            using (_SQLAdp = new SQLiteAdapter(DB_PATH))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("select");
                sb.AppendLine(" count(*)");
                sb.AppendLine(" from " + tbl);

                DataSet ds = new DataSet();
                ds = _SQLAdp.GetDataset(ds, "table", sb.ToString(), new object[] {  });
                if (ds != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return int.Parse(ds.Tables[0].Rows[0][0].ToString());
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    //見つからない
                    return 0;
                }
            }
        }

        /// <summary>
        /// マスタデータの件数をチェックする
        /// </summary>
        /// <returns></returns>
        public bool CheckTableDatas()
        {
            bool rc = true;

            try
            {
                string[] tbls = new string[] {"M_JIGYOSHO", "M_SHOZOKU", "M_SHARYO"};

                int cnt = 0;
                foreach(string tbl in tbls)
                {
                    cnt = TableRecordCount(tbl);
                    if(0 == cnt) return false;
                }
                rc = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                rc = false;
            }

            return rc;

        }

               /// <summary>
        /// マスタデータの件数をチェックする
        /// </summary>
        /// <returns></returns>
        public int CheckTableDatasInt()
        {
            int rc = 0;

            try
            {
                string[] tbls = new string[] { "M_JIGYOSHO", "M_SHOZOKU", "M_SHARYO" };

                int cnt = 0;
                foreach (string tbl in tbls)
                {
                    cnt = TableRecordCount(tbl);
                    if (0 == cnt && tbl != "M_SHARYO") return 1;
                    if (0 == cnt && tbl == "M_SHARYO") return 2;
                }
                rc = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                rc = 1;
            }
            return rc;

        }

        /// <summary>
        /// テーブルの存在をチェックする
        /// </summary>
        /// <returns></returns>
        public bool ExistsTables(string[] tbls)
        {
            bool rc = false;
            try
            {

                int cnt = 0;
                foreach (string tbl in tbls)
                {
                    cnt = TableRecordCount(tbl);
                }
                rc = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return rc;
        }

    }
}
