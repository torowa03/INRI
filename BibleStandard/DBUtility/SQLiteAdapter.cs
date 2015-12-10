using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.Data.Common;
using System.IO;

namespace DBUtility
{
    /// <summary>
    /// SQLite接続クラス
    /// </summary>
    class SQLiteAdapter : IDisposable
    {
        #region フィールドとプロパティ

        /// <summary>
        /// SQLの実体
        /// </summary>
        private SQLiteConnection con;

        /// <summary>
        /// クエリの実行
        /// </summary>
        private SQLiteCommand cmd;

        /// <summary>
        /// トランザクション
        /// </summary>
        private SQLiteTransaction tran;

        /// <summary>
        /// データアダプタ
        /// </summary>
        private SQLiteDataAdapter da;

        /// <summary>
        /// fileストリーム
        /// </summary>
        private static FileStream fs;

        /// <summary>
        /// 手動でトランザクションを行う
        /// </summary>
        private bool autoTran;

        /// <summary>
        /// 既にDisposeメソッドが呼び出されたかどうか
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// 自動でトランザクションを行う（初期値はtrue)
        /// </summary>
        public bool AutoTransaction
        {
            get { return autoTran; }
            set { autoTran = value; }
        }

        #endregion

        #region 定数

        //各SQL
        private const string CONNECTION_DB = "Data Source=";    //DB接続
        private const string EXIST_TABLE_QUERY = "COUNT(*) FROM sqlite_master WHERE name = '@tablename'";   //テーブル存在チェック
        private const string CREATE_TABLE_QUERY = "CREATE TABLE ";              //テーブル作成
        private const string ATTACH_QUERY = "ATTACH DATABASE '{0}' AS {1}";     //アタッチ
        private const string DETACH_QUERY = "DETACH DATABASE {0}";              //デタッチ
        //private const string PASS = "sd7rc2";

        //パラメータ
        private const string PARAM_EXIST_TABLE = "tablename";

        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dbFileName"></param>
        public SQLiteAdapter(string dbFileName)
        {
            //VSSにDBファイルを入れると勝手に読み取り属性になるので解除する
            try
            {
                if (System.IO.File.Exists(dbFileName) == true)
                {
                    FileAttributes fas = File.GetAttributes(dbFileName);
                    fas = fas & ~FileAttributes.ReadOnly;
                    File.SetAttributes(dbFileName, fas);
                }

            }catch(Exception ex)
            {
                throw ex;
            }


            try
            {
                //デフォルトは自動でトランザクションを行う
                this.autoTran = true;

                //データベースファイルに接続を行う
                if ((con = new SQLiteConnection(CONNECTION_DB + dbFileName)) == null)
                {
                    throw new SQLiteException("DB接続失敗!");
                }

                //コマンド作成
                if ((cmd = con.CreateCommand()) == null)
                {
                    throw new SQLiteException("コマンドが実行できません!");
                }

                //データベースを開く
                this.con.Open();
            }
            catch (SQLiteException ex)
            {
                throw new SQLiteException(ex.Message);
            } 
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~SQLiteAdapter()
        {
            ///Dispose呼出
            Dispose(false);
        }

        #region DBに対する処理

        /// <summary>
        /// DBファイルの新規作成
        /// </summary>
        /// <param name="dbFileName"></param>
        public static void CreateDB(string dbFileName)
        {
            try
            {
                using (fs = new FileStream(dbFileName, FileMode.Create))
                {
                    fs = File.Create(dbFileName);
                }
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        /// DBのアタッチ
        /// </summary>
        /// <param name="addDbFileName">追加するファイル名</param>
        /// <param name="addDbName">追加するDB名</param>
        public void AttachDatabase(string addDbFileName, string addDbName)
        {
            try
            {
                ///コマンド作成
                string sql = string.Format(ATTACH_QUERY, addDbFileName, addDbName);
                cmd.CommandText = sql;
                ///コマンド実行
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                throw new SQLiteException(e.Message);
            }
        }

        /// <summary>
        /// DBのデタッチ
        /// </summary>
        /// <param name="delDbName">切り離すDB名</param>
        public void DetachDatabase(string delDbName)
        {
            try
            {
                ///コマンド作成
                string sql = string.Format(DETACH_QUERY, delDbName);
                cmd.CommandText = sql;
                ///コマンド実行
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                throw new SQLiteException(e.Message);
            }
        }

        ///必要？？？？
        /// <summary>
        /// /テーブル作成
        /// </summary>
        /// <param name="tableName">作成するテーブル名</param>
        /// <param name="tableDefine">テーブル定義</param>
        public void CreateTable(string tableName, string tableDefine)
        {
            ///既にテーブルが存在しないか確認
            string sql = EXIST_TABLE_QUERY;
            string[] paramKey = { PARAM_EXIST_TABLE };
            string[] paramVal = { tableName };

            List<string[]> List = new List<string[]>();
            List = ExecuteReader(sql, paramKey, paramVal);

            if (List[0][0] == "0")
            {
                ///テーブルが存在しない場合、コマンド実行
                sql = CREATE_TABLE_QUERY + tableName + tableDefine;
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region トランザクション

        /// <summary>
        /// トランザクションの開始
        /// </summary>
        private void TransactionStart()
        {
            //トランザクションの開始
            tran = con.BeginTransaction();
        }

        /// <summary>
        /// トランザクションのコミット
        /// </summary>
        private void TransactionCommit()
        {
            if (tran == null)
                return;
            //トランザクションのコミット
            tran.Commit();
            //トランザクションの解放
            tran.Dispose();
            tran = null;
        }

        /// <summary>
        /// トランザクションのロールバック
        /// </summary>
        private void TransactionRollBack()
        {
            if (tran == null)
                return;
            //トランザクションのロールバック
            tran.Rollback();
            //トランザクションの解放
            tran.Dispose();
            tran = null;
        }

        /// <summary>
        /// トランザクション開始（自動指定しなかった場合）
        /// </summary>
        public void transactionStart()
        {
            if (autoTran)
            {
                throw new SQLiteException("自動でトランザクションを開始するように設定されています。");
            }

            TransactionStart();
        }

        /// <summary>
        /// トランザクションのコミット（自動指定しなかった場合）
        /// </summary>
        public void transactionCommit()
        {
            if (autoTran)
            {
                throw new SQLiteException("自動でトランザクションを開始するように設定されています。");
            }

            TransactionCommit();
        }

        /// <summary>
        /// トランザクションのロールバック（自動指定シなかった場合）
        /// </summary>
        public void transactionRollback()
        {
            if (autoTran)
            {
                throw new SQLiteException("自動でトランザクションを開始するように設定されています。");
            }

            TransactionRollBack();
        }

        #endregion


        #region 結果を返さないクエリの実行

        /// <summary>
        /// 結果を返さないクエリの実行(INSERT,UPDATE,DELETEなど）
        /// </summary>
        /// <param name="sql">実行するSQL</param>
        /// <param name="arg">パラメータ</param>
        public void ExecuteNonQuery(string sql, params object[] paramVal)
        {
            if (autoTran)
            {
                //トランザクションの開始
                TransactionStart();
            }

            try
            {
                cmd = con.CreateCommand(); // ITI add 2012/08/23

                //パラメータのセット
                SetParameter(sql, paramVal);

                //SQL実行
                cmd.ExecuteNonQuery();

                if (autoTran)
                {
                    //自動でトランザクションが行われる場合
                    //トランザクションのコミット
                    TransactionCommit();
                }
            }
            catch (SQLiteException e)
            {
                if (autoTran)
                {
                    //自動でトランザクションが行われる場合
                    //トランザクションのロールバック
                    TransactionRollBack();
                }
                //throw new SQLiteException(e.Message);
                throw new SQLiteException(string.Format("{0}\n SQL:[{1}]", e.Message, cmd.CommandText));
            }
        }

        /// <summary>
        /// 結果を返さないクエリの実行(INSERT,UPDATE,DELETEなど）
        /// </summary>
        /// <param name="sql">実行するSQL</param>
        public void ExecuteNonQuery(string sql)
        {
            if (autoTran)
            {
                //トランザクションの開始
                TransactionStart();
            }

            try
            {
                //SQL実行
                //コマンド設定
                cmd = con.CreateCommand(); // ITI add 2012/08/23
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                if (autoTran)
                {
                    //自動でトランザクションが行われる場合
                    //トランザクションのコミット
                    TransactionCommit();
                }
            }
            catch (SQLiteException e)
            {

                if (autoTran)
                {
                    //自動でトランザクションが行われる場合
                    //トランザクションのロールバック
                    TransactionRollBack();
                }
                //throw new SQLiteException(e.Message);
                throw new SQLiteException(string.Format("{0}\n SQL:[{1}]", e.Message, cmd.CommandText));

            }
        }

        /// <summary>
        /// Vacuumの実行
        /// </summary>
        public void ExecuteVacuum()
        {
            try
            {
                //SQL実行
                //コマンド設定
                cmd = con.CreateCommand();
                cmd.CommandText = "vacuum;";
                cmd.ExecuteNonQuery();

            }
            catch (SQLiteException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                //Vacuumに失敗してもエラーを発生させない
            }
        }

        #endregion

        #region 結果を取得するクエリの実行

        /// <summary>
        /// SELECTの実行
        /// </summary>
        /// <param name="sql">実行するSQL</param>
        /// <param name="args">パラメータ</param>
        /// <returns>取得結果</returns>
        public List<string[]> ExecuteReader(string sql, params object[] paramVal)
        {
            try
            {
                //パラメータのセット
                cmd = con.CreateCommand();
                SetParameter(sql, paramVal);

                //取得結果を読み込む
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    List<string[]> ListData = new List<string[]>();
                    //1行ずつ読込
                    ReadDB(out ListData, reader);
                    return ListData;
                }
            }
            catch (SQLiteException e)
            {
                //throw new SQLiteException(e.Message);
                throw new SQLiteException(string.Format("{0}\n SQL:[{1}]", e.Message, cmd.CommandText));

            }
        }

        /// <summary>
        /// SELECTの実行（パラメータなし）
        /// </summary>
        /// <param name="sql"></param>
        /// <returns>取得結果</returns>
        public List<string[]> ExecuteReader(string sql)
        {
            try
            {
                //SQL実行
                //コマンド設定
                cmd = con.CreateCommand(); 
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                //取得結果を読み込む
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    List<string[]> ListData = new List<string[]>();
                    ///Readerの読込
                    ReadDB(out ListData, reader);
                    return ListData;
                }
            }
            catch (SQLiteException e)
            {
                //throw new SQLiteException(e.Message);
                throw new SQLiteException(string.Format("{0}\n SQL:[{1}]", e.Message, cmd.CommandText));

            }
        }

        /// <summary>
        /// Readerの読込
        /// </summary>
        /// <param name="ListData">読込データ格納用</param>
        /// <param name="reader">StreamReader</param>
        /// <returns>読込データ</returns>
        private List<string[]> ReadDB(out List<string[]> ListData, SQLiteDataReader reader)
        {
            ListData = new List<string[]>();

            while (reader.Read())
            {
                //1行ずつ読込
                string[] column = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    //1カラムずつ読込
                    column[i] = reader[i].ToString();
                }
                ListData.Add(column);
            }
            return ListData;
        }

        /// <summary>
        /// パラメータの設定
        /// </summary>
        /// <param name="sql">対象SQL</param>
        /// <param name="paramVal">セットするパラメータ（値）</param>
        public void SetParameter(string sql, params object[] paramVal)
        {
            //コマンド設定
            cmd.CommandText = sql;
            cmd.Parameters.Clear();

            foreach (object obj in paramVal)
            {
                //バインド引数に値をセット
                SQLiteParameter param = new SQLiteParameter();
                param.Value = obj;
                cmd.Parameters.Add(param);
            }

            //パラメータをセット
            cmd.Prepare();
        }

        /// <summary>
        /// データセット取得
        /// </summary>
        /// <param name="ds">格納用データセット</param>
        /// <param name="tableName">格納用テーブル名</param>
        /// <param name="sql">実行するクエリ</param>
        /// <param name="paramVal">パラメータ（値）</param>
        /// <returns>データセット/returns>
        public DataSet GetDataset(DataSet ds, string tableName, string sql, object[] paramVal)
        {
            try
            {
                ///既存のデータテーブルを削除
                DeleteDataTable(ds, tableName);
                ///データセット取得
                using (da = new SQLiteDataAdapter())
                {
                    CreateCommand(ds, tableName, sql, paramVal);
                }
                return ds;
            }
            catch (SQLiteException ex)
            {
                //throw new SQLiteException(ex.Message);
                throw new SQLiteException(string.Format("{0}\n SQL:[{1}]", ex.Message, cmd.CommandText));

            }
        }

        /// <summary>
        /// 既存のデータテーブルを削除
        /// </summary>
        /// <param name="ds">データセット</param>
        /// <param name="tableName">対象テーブル名</param>
        private void DeleteDataTable(DataSet ds, string tableName)
        {
            foreach (DataTable dt in ds.Tables)
            {
                if (dt.TableName == tableName)
                {
                    ds.Tables.Remove(tableName);
                    break;
                }
            }
        }

        /// <summary>
        /// データセットの更新
        /// </summary>
        /// <param name="ds">格納用データセット</param>
        /// <param name="tableName">格納用テーブル名</param>
        /// <param name="sql">Selectクエリ</param>
        /// <param name="paramVal">Selectパラメータ（値）</param>
        public void UpdateDataSet(DataSet ds, string tableName, string sql, object[] paramVal)
        {
            try
            {
                if (autoTran)
                {
                    //トランザクションの開始
                    TransactionStart();
                }
                ///データアダプタよりデータセット取得
                using (da = new SQLiteDataAdapter())
                {
                    CreateCommandUp(ds, tableName, sql, paramVal);
                }
                if (autoTran)
                {
                    //自動でトランザクションが行われる場合
                    //トランザクションのコミット
                    TransactionCommit();
                }
            }
            catch (SQLiteException e)
            {
                if (autoTran)
                {
                    //自動でトランザクションが行われる場合
                    //トランザクションのロールバック
                    TransactionRollBack();
                }
                //throw new SQLiteException(e.Message);
                throw new SQLiteException(string.Format("{0}\n SQL:[{1}]", e.Message, cmd.CommandText));

            }
        }

        /// <summary>
        /// コマンドオブジェクト作成（Select）
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="tableName"></param>
        /// <param name="sql"></param>
        /// <param name="paramVal"></param>
        private void CreateCommand(DataSet ds, string tableName, string sql, object[] paramVal)
        {
            //SELECT用コマンドオブジェクト作成
            using (cmd = con.CreateCommand())
            {
                //コマンドオブジェクト設定
                da.SelectCommand = cmd;
                SetParameter(sql, paramVal);
                //データセットへのデータ格納
                da.Fill(ds, tableName);
            }
        }


        /// <summary>
        /// コマンドオブジェクト作成（Update）
        /// </summary>
        /// <param name="ds">格納用データセット</param>
        /// <param name="tableName">格納用テーブル名</param>
        /// <param name="sql">Selectクエリ</param>
        /// <param name="paramVal">Selectパラメータ（値）</param>
        private void CreateCommandUp(DataSet ds, string tableName, string sql, object[] paramVal)
        {
            ///Selectコマンドオブジェクト作成
            using (cmd = con.CreateCommand())
            {
                //Selectコマンドオブジェクト設定
                da.SelectCommand = cmd;
                SetParameter(sql, paramVal);

                using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder(da))
                {
                    //更新コマンド作成
                    da.UpdateCommand = builder.GetUpdateCommand();  //更新
                    da.InsertCommand = builder.GetInsertCommand();  //追加
                    da.DeleteCommand = builder.GetDeleteCommand();  //削除
                    //データセットの更新
                    da.Update(ds.Tables[tableName]);
                }
            }
        }

        /// <summary>
        /// スキーマ情報の取得
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchema()
        {
            DataTable dt = con.GetSchema("TABLES");
            return dt;
        }

        /// <summary>
        /// [issue#511] 締日対応
        /// DataTableの内容を一時テーブルに入れた後、一時テーブルに対してSelectを行った
        /// 結果をデータ・セットで取得する
        /// </summary>
        /// <param name="dt">値が入ったデータ・セット</param>
        /// <param name="dbPath">SQLiteデータベースファイル(dtgmgrを指定)</param>
        /// <param name="tempTableName">一時テーブル名</param>
        /// <param name="selectSql">検索SQL</param>
        /// <param name="pms">検索時のパラメータ</param>
        /// <returns></returns>
        public DataSet GetDatasetbyTempTable(DataTable dt, string tempTableName, string selectSql, object[] pms)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            DataSet ds = new DataSet();
            string createTableSql = __getTempTableCreateSQL(dt, tempTableName);
            //テンポラリテーブルを作成する
            ExecuteNonQuery(createTableSql);

            //テンポラリテーブルにデータを追加する
            foreach (DataRow r in dt.Rows)
            {
                string insSql = string.Empty;
                List<object> vals = new List<object>();
                __getTempTableInsertSQL(dt, r, out insSql, out vals, tempTableName);
                ExecuteNonQuery(insSql, vals.ToArray());
            }
            ds = GetDataset(ds, "table", selectSql, pms);
            sw.Stop();
            System.Diagnostics.Debug.WriteLine(string.Format("[issue#511] 一時テーブルで検索 行数:{0} 処理時間:{1}",dt.Rows.Count, sw.Elapsed));
            return ds;
        }

        /// <summary>
        /// [issue#511] 締日対応
        /// DataTableの内容を一時テーブルに入れた後、一時テーブルに対してSelectを行った
        /// 結果をデータ・セットで取得する
        /// 高速化版
        /// </summary>
        /// <param name="dt">値が入ったデータ・セット</param>
        /// <param name="dbPath">SQLiteデータベースファイル(dtgmgrを指定)</param>
        /// <param name="tempTableName">一時テーブル名</param>
        /// <param name="selectSql">検索SQL</param>
        /// <param name="pms">検索時のパラメータ</param>
        /// <returns></returns>
        public DataSet GetDatasetbyTempTableFast(DataTable dt, string tempTableName, string selectSql, object[] pms)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            DataSet ds = new DataSet();

            AutoTransaction = false;
            transactionStart();

            sw.Reset();
            sw.Start();
            string createTableSql = __getTempTableCreateSQL(dt, tempTableName);
            //テンポラリテーブルを作成する
            ExecuteNonQuery(createTableSql);
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("[issue#511] 一時テーブル作成 time={0}", sw.Elapsed);

            //テンポラリテーブルにデータを追加する
            sw.Reset();
            sw.Start();
            List<string> insSql = new List<string>();
            __getTempTableInsertSQLFast(dt, out insSql, tempTableName);
            foreach (string sql in insSql)
            {
                ExecuteNonQuery(sql, new object[] { });
            }
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("[issue#511] 一時テーブルへレコード追加 count={0}, time={1}", dt.Rows.Count, sw.Elapsed);

            sw.Reset();
            sw.Start();
            ds = GetDataset(ds, "table", selectSql, pms);
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("[issue#511] 一時テーブルの集計 count={0}, time={1}", ds.Tables[0].Rows.Count, sw.Elapsed);

            transactionCommit();

            return ds;
        }

        /// <summary>
        /// [issue#511] 締日対応
        /// 一時テーブルへの行追加SQLの取得
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="r">行オブジェクト</param>
        /// <param name="sql">InsertSQL (out)</param>
        /// <param name="values">検索時のパラメータ (out)</param>
        /// <param name="tablename">一時テーブル名</param>
        private void __getTempTableInsertSQL(DataTable dt, DataRow r, out string sql, out List<object> values, string tablename = "tmp_table")
        {
            List<string> marks = new List<string>();
            values = new List<object>();   //値リスト
            for (int c = 0; c < dt.Columns.Count; c++)
            {
                marks.Add("?");
                values.Add(r[c]);
            }

            //InsertのSQL
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("insert into " + tablename);
            sb.AppendLine("values(");
            sb.AppendLine(string.Join(",", marks.ToArray()));
            sb.AppendLine(")");
            sql = sb.ToString();
        }

        /// <summary>
        /// [issue#511] 締日対応
        /// 一時テーブルへの行追加SQLの取得
        /// インサート高速化版 まとめてインサート
        /// insert into [table]
        /// select aaa, bbb 
        /// union all select aaa1, bbb1
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="outSQLList"></param>
        /// <param name="tablename"></param>
        private void __getTempTableInsertSQLFast(DataTable dt, out List<string> outSQLList, string tablename = "tmp_table")
        {
            List<string> sqlList = new List<string>();  //複数のSQLを格納するリスト
            StringBuilder sql = new StringBuilder();    //1回で実行するSQLの構築用
            List<string> marks = new List<string>();    //列の値配列をつくる際のstring.format用テンプレート
            foreach (DataColumn c in dt.Columns)
            {
                //DataTable取得時の列の型をみて、文字列の場合は、''で囲む
                //SQLの取得の仕方によって、この部分は列の型の条件を増やす必要があるかもしれない
                if (c.DataType.ToString() == "System.String")
                {
                    marks.Add("'{0}'");
                }
                else
                {
                    marks.Add("{0}");
                }
            }
            for (int r = 0; r < dt.Rows.Count; r++)
            {
                List<string> vals = new List<string>();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    vals.Add(string.Format(marks[i], dt.Rows[r][i].ToString()));
                }
                StringBuilder sb = new StringBuilder();
                if (sql.Length == 0)
                {
                    sql.AppendLine("insert into " + tablename);
                    sb.Append("select ");
                }
                else
                {
                    sb.Append(" union all select ");
                }
                sb.Append(string.Join(",", vals.ToArray()));
                sql.AppendLine(sb.ToString());

                if (r >= 480)
                {
                    //一度に500件以上インサートできない
                    sqlList.Add(sql.ToString());
                    sql.Clear();
                }
            }
            sqlList.Add(sql.ToString());
            outSQLList = sqlList;
        }

        /// <summary>
        /// [issue#511] 締日対応
        /// DataTableから一時テーブルCreateのSQLを作成
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="tablename">一時テーブル名</param>
        /// <returns>SQL</returns>
        private string __getTempTableCreateSQL(DataTable dt, string tablename = "tmp_table")
        {
            StringBuilder sb = new StringBuilder();
            if (dt == null) return sb.ToString();

            //DataTableから列名を取得する
            var columnNames = from DataColumn dc in dt.Columns select dc.ColumnName;

            //テーブル作成のSQL
            sb.AppendLine("create TEMPORARY table " + tablename);
            sb.AppendLine("(");
            sb.AppendLine(string.Join(",", columnNames.ToArray()));
            sb.AppendLine(")");

            return sb.ToString();
        }

        /// <summary>
        /// 別DBから同じテーブル名のデータをコピーする
        /// </summary>
        /// <param name="fileLoc"></param>
        /// <param name="table"></param>
        public void ImportData(String fileLoc, string table)
        {
            string SQL = "ATTACH '" + fileLoc + "' AS TOMERGE";
            cmd = con.CreateCommand();
            cmd.CommandText = SQL;

            int retval = 0;
            try
            {
                retval = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }

            SQL = string.Format("INSERT INTO {0} SELECT * FROM TOMERGE.{0}",table);
            cmd = con.CreateCommand();
            cmd.CommandText = SQL;
            retval = 0;
            try
            {
                retval = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                cmd.Dispose();
            }
        }


        #endregion

        #region IDisposableメンバ

        /// <summary>
        /// 資源の解放
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            //ファイナライザ－を呼び出さないように制御
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 資源の解放（本体）
        /// </summary>
        public void Dispose(bool disposing)
        {
            //既に解放済みかどうか
            if (_disposed)
            {
                //終了
                return;
            }
            this._disposed = true;

            if (disposing)
            {
                //コマンドを閉じる
                cmd.Dispose();
                //データベースを閉じる
                con.Close();
            }
        }

        #endregion
    }
}
