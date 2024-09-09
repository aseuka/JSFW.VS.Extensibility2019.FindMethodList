using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.OleDb;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Collections;
using Newtonsoft.Json;

namespace JSFW
{
    public static class JSFW_UtilEx
    { 
        public static string QuoteName(this object obj, Type datatype, string quote)
        {
            string output = ("" + obj);
            if (datatype == typeof(DateTime))
            {
                output = obj.IsNull() ? "" : DateTime.ParseExact("" + obj, "yyyy-MM-dd tt h:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal).ToString("yyyy-MM-dd");
                if (output != "") output = string.Format("{1}{0}{1}", output, quote);
            }
            else if (IsQuoteType(datatype))
            {
                output = string.Format("{1}{0}{1}", obj, quote);
            }
            else if (IsNumeric(datatype))
            {
                decimal dm = 0;
                if (obj is double || obj is float)
                {
                    decimal.TryParse(string.Format("{0:r}", obj), out dm);
                }
                else
                {
                    decimal.TryParse(string.Format("{0}", obj), out dm);
                }
                output = string.Format("{0}", dm);

            }
            if (output == "")
            {
                output = "null";
            }
            return output;
        }

        private static bool IsNumeric(Type type)
        {
            bool IsTyped = false;
            switch (type.Name.ToLower())
            {
                case "int16":
                case "int32":
                case "int64":
                case "float":
                case "double":
                case "decimal":
                    IsTyped = true;
                    break;
            }
            return IsTyped;
        }

        public static string QuoteName(this object obj, string quote)
        {
            if (quote == "[" || quote == "]")
            {
                return string.Format("[{0}]", ("" + obj).Replace("''", "'"));
            }
            else if (quote == "(" || quote == ")")
            {
                return string.Format("({0})", ("" + obj).Replace("''", "'"), quote);
            }
            else
                return string.Format("{1}{0}{1}", ("" + obj).Replace("''", "'"), quote);
        }

        public static bool IsNull(this object obj)
        {
            return obj == null || obj == DBNull.Value;
        }

        private static bool IsQuoteType(Type type)
        {
            return IsQuoteType(type.Name);
        }

        private static bool IsQuoteType(string typeName)
        {
            bool IsTyped = false;
            switch (typeName.ToLower())
            {
                case "dbnull":
                case "string":
                case "datetime":
                case "char":
                    IsTyped = true;
                    break;
            }
            return IsTyped;
        }

        public static Type Get_DBToNet(string DbTypeName)
        {
            Type ttt = null;

            switch (DbTypeName.ToLower())
            {
                case "nchar":
                case "char":
                case "nvarchar":
                case "varchar":
                case "datetime":
                default:
                    ttt = typeof(string);
                    break;
                case "int":
                case "tinyint":
                    ttt = typeof(int);
                    break;
                case "numeric":
                    ttt = typeof(decimal);
                    break;
            }
            return ttt;
        }

        public static void DebugWarning(this string msg)
        {
            string title = "";
            try
            {
                System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(1);
                title = sf.GetMethod().Name + ":";
            }
            catch (Exception)
            {
            }

            System.Diagnostics.Debug.WriteLine(msg, title);
        }
        public static void DebugWarning(this string msg, string title)
        {
            System.Diagnostics.Debug.WriteLine(msg, title);
        }

        public static DateTime? ToDateTime(this object obj, string Fmt)
        {
            DateTime _datetime;
            //try
            //{ 
            //    _datetime = DateTime.ParseExact("" + obj, "yyyy-MM-dd tt H:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal);
            //    return _datetime;
            //}
            //catch (Exception)
            //{ } 

            if (string.IsNullOrEmpty(Fmt)) Fmt = "yyyy-MM-dd tt hh:mm:ss";

            //if (obj.IsNull() || string.IsNullOrEmpty(("" + obj))) return null;
            //else
            //{
            //    return DateTime.ParseExact("" + obj, Fmt, null, System.Globalization.DateTimeStyles.AssumeLocal);
            //}
            string dt = "" + obj;
            if (dt.Contains("오전") || dt.Contains("오후"))
            {
                if (dt.Length == "yyyy-MM-dd 오후 hh:mm:ss".Length)
                {
                    if (DateTime.TryParseExact("" + obj, "yyyy-MM-dd tt hh:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal, out _datetime))
                    {
                        return _datetime;
                    }
                }
                else if (dt.Length == "yyyy-MM-dd 오후 H:mm:ss".Length)
                {
                    if (DateTime.TryParseExact("" + obj, "yyyy-MM-dd tt H:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal, out _datetime))
                    {
                        return _datetime;
                    }
                }
            }
            else
            {
                if (dt.Length == "yyyy-MM-dd hh:mm:ss".Length)
                {
                    if (DateTime.TryParseExact("" + obj, "yyyy-MM-dd hh:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal, out _datetime))
                    {
                        return _datetime;
                    }
                }
                else if (dt.Length == "yyyy-MM-dd h:mm:ss".Length)
                {
                    if (DateTime.TryParseExact("" + obj, "yyyy-MM-dd h:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal, out _datetime))
                    {
                        return _datetime;
                    }
                }
                else if (dt.Length == "yyyy-MM-dd hh:mm:ss.fff".Length)
                {
                    if (DateTime.TryParseExact("" + obj, "yyyy-MM-dd hh:mm:ss.fff", null, System.Globalization.DateTimeStyles.AssumeLocal, out _datetime))
                    {
                        return _datetime;
                    }
                }
                else if (dt.Length == "yyyy-MM-dd h:mm:ss.fff".Length)
                {
                    if (DateTime.TryParseExact("" + obj, "yyyy-MM-dd h:mm:ss.fff", null, System.Globalization.DateTimeStyles.AssumeLocal, out _datetime))
                    {
                        return _datetime;
                    }
                }
                else if (dt.Length == "yyyy-MM-dd".Length)
                {
                    if (DateTime.TryParseExact("" + obj, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.AssumeLocal, out _datetime))
                    {
                        return _datetime;
                    }
                }
                else if (dt.Length == "yyyyMMdd".Length)
                {
                    if (DateTime.TryParseExact("" + obj, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AssumeLocal, out _datetime))
                    {
                        return _datetime;
                    }
                }
            }
            return null;
        }

        public static string Toyyyy_MM_dd(this DateTime datetime, string Fmt)
        {
            if (string.IsNullOrEmpty(Fmt)) Fmt = "yyyy-MM-dd";
            return datetime.ToString(Fmt);
        }

        public static T To<T>(this object obj, object DefaultValue) where T : IConvertible
        {
            if (typeof(T).BaseType == typeof(Enum))
            {
                if (Enum.IsDefined(typeof(T), obj))
                {
                    return (T)Enum.Parse(typeof(T), "" + obj);
                }
                else
                {
                    return (T)DefaultValue;
                }
            }

            TypeCode typecode = (TypeCode)Enum.Parse(typeof(TypeCode), typeof(T).Name);

            if (string.IsNullOrEmpty("" + obj))
            {
                switch (typecode)
                {
                    case TypeCode.Boolean:
                        break;
                    case TypeCode.Byte:
                        break;
                    case TypeCode.Char:
                        break;
                    case TypeCode.DBNull:
                        break;
                    case TypeCode.Empty:
                        break;
                    case TypeCode.Object:
                        break;
                    case TypeCode.SByte:
                        break;
                    case TypeCode.String:
                        break;

                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        obj = DefaultValue ?? "0";
                        break;
                    default:
                        obj = "";
                        break;
                    case TypeCode.DateTime:
                        return (T)obj;
                }
            }
            return (T)Convert.ChangeType(obj, typecode);
        }

        public static DialogResult Alert(this object msg)
        {
            return JSFW_MessageBox.Show("{0}", msg);
        }
        public static DialogResult AlertWarning(this string msg)
        {
            return JSFW_MessageBox.Show("{0}", msg, "경고");
        }

        /// <summary>
        /// Yes or No [ Question ]
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static DialogResult Confirm(this object msg)
        {
            return JSFW_MessageBox.Show("{0}", msg, "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        // 디렉토리  
        public static bool ExistsAndCreate(string DirectoryPath)
        {
            try
            {
                if (Directory.Exists(DirectoryPath))
                {
                    return true;
                }
                string tempFullPath = Path.GetFullPath(DirectoryPath).Replace(@"\\", @"\");
                string RootPath = Path.GetPathRoot(tempFullPath);
                string[] Split = tempFullPath.Split('\\');
                string PassDirectory = "";
                foreach (var item in Split)
                {
                    PassDirectory += item + @"\";
                    if (item == RootPath) continue;

                    if (Directory.Exists(PassDirectory)) continue;

                    Directory.CreateDirectory(PassDirectory);
                }

                return Directory.Exists(DirectoryPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // File copy! 
        public static string FileCopy(string RootFilePath, string id, string sourceFileName, bool isOverwirte)
        {
            string destFolderPath = Path.Combine(RootFilePath + "\\" + "Files\\", id);
            string fileName = DateTime.Now.ToString("yyyy-MM-dd HHmmss") + " ## " + Path.GetFileName(sourceFileName);
            string destFileName = Path.Combine(destFolderPath + "\\", fileName);
            if (File.Exists(sourceFileName))
            {
                if (!Directory.Exists(destFolderPath))
                {
                    Directory.CreateDirectory(destFolderPath);
                }
                File.Copy(sourceFileName, destFileName, true);
                return destFileName;
            }
            else
                return "";
        }

        /// <summary>
        /// Object To json
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="value">object Instance</param>
        /// <returns></returns>
        public static string Serialize<T>(this T value)
        {
            return JsonConvert.SerializeObject(value); 
        }

        /// <summary>
        /// json String !
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T DeSerialize<T>(this string json) where T : class, new()
        {
            return JsonConvert.DeserializeObject<T>(json); 
        }

        /// <summary>
        /// 컨트롤 비동기 호출! 
        /// </summary>
        /// <typeparam name="TControl"></typeparam>
        /// <param name="ctrl"></param>
        /// <param name="action"></param>
        public static void Invk<TControl>(this TControl ctrl, Action<TControl> action) where TControl : Control
        {
            if (ctrl == null) return;

            if (ctrl.InvokeRequired)
            {
                ctrl.Invoke(action, ctrl);
            }
            else
            {
                action?.Invoke(ctrl);
            }
        }
    }

    public class JSFW_MessageBox
    {
        public static DialogResult Show(object msg)
        {
            return Show("{0}", msg);
        }

        public static DialogResult Show(string fmt, object msg)
        {
            return MessageBox.Show(string.Format(fmt, msg));
        }

        public static DialogResult Show(string fmt, object msg, string caption)
        {
            return MessageBox.Show(string.Format(fmt, msg), caption);
        }

        public static DialogResult Show(string fmt, object msg, string caption, MessageBoxButtons btnenum)
        {
            return MessageBox.Show(string.Format(fmt, msg), caption, btnenum);
        }

        public static DialogResult Show(string fmt, object msg, string caption, MessageBoxButtons btnenum, MessageBoxIcon micon)
        {
            return MessageBox.Show(string.Format(fmt, msg), caption, btnenum, micon);
        }
    }



    /// <summary>
    /// Header List
    /// </summary>
    public class Headers : IList<DummyHeader>
    {
        protected List<DummyHeader> HeaderItems { get; set; }
        public List<DummyHeader> PrimaryItems { get; protected set; }

        public Headers()
        {
            HeaderItems = new List<DummyHeader>();
            PrimaryItems = new List<DummyHeader>();
        }

        public bool IsPrimaryKey(DummyHeader item)
        {
            return PrimaryItems != null && PrimaryItems.IndexOf(item) >= 0;
        }

        public DummyHeader this[string Key]
        {
            get
            {
                DummyHeader header = null;
                foreach (DummyHeader item in HeaderItems)
                {
                    if (item.Key == Key) { header = item; break; }
                }
                return header;
            }
        }

        public int IndexOf(DummyHeader item)
        {
            return HeaderItems.IndexOf(item);
        }

        public int IndexOf(string item)
        {
            return HeaderItems.IndexOf(this[item]);
        }

        public void Insert(int index, DummyHeader item)
        {
            HeaderItems.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            HeaderItems.RemoveAt(index);
        }

        public DummyHeader this[int index]
        {
            get
            {
                return HeaderItems[index];
            }
            set
            {
                HeaderItems[index] = value;
            }
        }

        public void Add(DummyHeader item)
        {
            HeaderItems.Add(item);
        }

        public void Clear()
        {
            HeaderItems.Clear();
        }

        public bool Contains(DummyHeader item)
        {
            return HeaderItems.Contains(item);
        }

        public void CopyTo(DummyHeader[] array, int arrayIndex)
        {
            HeaderItems.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return HeaderItems.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(DummyHeader item)
        {
            return HeaderItems.Remove(item);
        }

        public IEnumerator<DummyHeader> GetEnumerator()
        {
            return HeaderItems.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public string[] Keys
        {
            get
            {
                var s = from i in HeaderItems
                        select i.Key;
                return s.ToArray();
            }
        }
    }

    /// <summary>
    /// Header
    /// </summary>
    public class Header : DummyHeader
    {
        public bool AllowEditing { get; set; }
        public IList<SourceGrid.Cells.Controllers.IController> ControlCollection { get; set; }
        public Header(string key, string text, IList<SourceGrid.Cells.Controllers.IController> controlCollection)
            : this(key, text)
        {
            ControlCollection = controlCollection;
        }

        public Header(string key, string text)
            : base(key, text)
        {

        }
    }

    public class DummyHeader : SourceGrid.Cells.ColumnHeader
    {
        public bool Identity { get; set; }
        public string Key { get; protected set; }
        public int Width { get; set; }
        public Type DataType { get; set; }
        public bool Hidden { get; set; }
        public string Format { get; set; }
        public virtual DevAge.Drawing.ContentAlignment ContentAlign { get; set; }

        /// <summary>
        /// JSFW_GRID_TREE 에서 Tree 컬럼으로 사용됨.
        /// </summary>
        public bool IsTree { get; set; }

        /// <summary>
        /// 필수값 여부
        /// </summary>
        public bool IsValideted { get; set; }

        public DummyHeader(string key, string text)
            : base(text)
        {
            DataType = typeof(string);
            // ContentAlign = DevAge.Drawing.ContentAlignment.MiddleLeft;
            this.Key = key;
            AutomaticSortEnabled = false;
        }
    }
    /// <summary>
    /// List&lt; Header + DataRow &gt;
    /// </summary>
    public class QueryParams : IList<QueryParamValue>
    {
        internal Headers OriginalHeaders { get; set; }

        public List<QueryParamValue> Values { get; internal set; }
        public string IUD { get; internal set; }
        public string TableName { get; internal set; }

        public QueryParams()
        {
            Values = new List<QueryParamValue>();
        }

        public int IndexOf(QueryParamValue item)
        {
            return Values.IndexOf(item);
        }

        public void Insert(int index, QueryParamValue item)
        {
            Values.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Values.RemoveAt(index);
        }

        public QueryParamValue this[int index]
        {
            get
            {
                return Values[index];
            }
            set
            {
                Values[index] = value;
            }
        }

        public QueryParamValue this[string Key]
        {
            get
            {
                QueryParamValue data = null;
                foreach (QueryParamValue val in Values)
                {
                    if (val.Header.Key == Key)
                    {
                        data = val; break;
                    }
                }
                return data;
            }
        }

        public void Add(QueryParamValue item)
        {
            Values.Add(item);
        }

        public void Clear()
        {
            Values.Clear();
        }

        public bool Contains(QueryParamValue item)
        {
            return Values.Contains(item);
        }

        public void CopyTo(QueryParamValue[] array, int arrayIndex)
        {
            Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Values.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(QueryParamValue item)
        {
            return Values.Remove(item);
        }

        public IEnumerator<QueryParamValue> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public static List<QueryParams> GetQueryParams(DataTable source)
        {
            var GetStateFlag = new Func<DataRow, string>((dr) =>
            {

                string flag = "";
                if (dr.RowState == DataRowState.Added)
                {
                    flag = "I";
                }
                else if (dr.RowState == DataRowState.Deleted)
                {
                    flag = "D";
                }
                else if (dr.RowState == DataRowState.Modified)
                {
                    flag = "U";
                }
                return flag;
            });

            var GetParamsValues = new Func<DataTable, DataRow, List<QueryParamValue>>((dt, dr) =>
            {
                List<QueryParamValue> values = new List<QueryParamValue>();
                foreach (DataColumn col in dt.Columns)
                {
                    values.Add(new QueryParamValue(new DummyHeader(col.ColumnName, col.ColumnName), dr[col]));
                }
                return values;
            });

            var datalist = from dr in source.AsEnumerable()
                           where dr.RowState == DataRowState.Added || dr.RowState == DataRowState.Deleted || dr.RowState == DataRowState.Modified
                           let flag = GetStateFlag(dr)
                           select new QueryParams()
                           {
                               OriginalHeaders = null,
                               TableName = source.TableName,
                               IUD = flag,
                               Values = GetParamsValues(source, dr)
                           };
            return datalist.ToList<QueryParams>();

        }

        public static IList<QueryParams> GetQueryParams(string flag, List<DataRow> rowList, params DataColumn[] column)
        {
            var GetParamsValues = new Func<DataTable, DataRow, List<QueryParamValue>>((dt, dr) =>
            {
                List<QueryParamValue> values = new List<QueryParamValue>();
                foreach (DataColumn col in dt.Columns)
                {
                    var prmsValue = new QueryParamValue(new DummyHeader(col.ColumnName, col.ColumnName), dr[col]);
                    if (column != null && column.Length > 0)
                    {
                        var f = column.Where(w => w.ColumnName == col.ColumnName);
                        prmsValue.IsPrimaryKey = f.Count() > 0;
                    }
                    prmsValue.Header.DataType = col.DataType;
                    values.Add(prmsValue);
                }
                return values;
            });

            var datalist = from dr in rowList
                           select new QueryParams()
                           {
                               OriginalHeaders = null,
                               TableName = dr.Table.TableName,
                               IUD = flag,
                               Values = GetParamsValues(dr.Table, dr),

                           };
            return datalist.ToList<QueryParams>();
        }
    }

    /// <summary>
    /// Header + DataRow
    /// </summary>
    public class QueryParamValue
    {
        public DummyHeader Header { get; protected set; }
        private object value = null;
        public object Value
        {
            get
            {
                return value.QuoteName(Header.DataType, "'");
            }

        }
        public QueryParamValue(DummyHeader h, object val)
        {
            Header = h;
            value = val;

            if (h.Editor is SourceGrid.Cells.Editors.RichTextBox)
            {
                string rtf = "" + value;
                rtf = rtf.Trim("'".ToCharArray()).QuoteName("'");
                value = rtf;
            }
        }

        public void SetValue(object obj)
        {
            value = obj;
        }

        public override string ToString()
        {
            return string.Format("[{0}]={1}", Header.Key, Value);
        }

        /// <summary>
        /// Key!
        /// </summary>
        public bool IsPrimaryKey { get; set; }
    }

    /// <summary>
    /// Query 생성기!! 
    /// </summary>
    public class QueryBuilder
    {
        public static string Building(QueryParams qPrms)
        {
            string Querys = string.Empty;
            switch (qPrms.IUD)
            {
                case "I": Querys = InsertQuery(qPrms); break;
                case "U": Querys = UpdateQuery(qPrms); break;
                case "D": Querys = DeleteQuery(qPrms); break;
            }
            return Querys;
        }
        public static string[] Building(IList<QueryParams> prms)
        {
            List<string> Querys = new List<string>();
            foreach (var item in prms)
            {
                Querys.Add(Building(item));
            }
            return Querys.ToArray();
        }


        private static string DeleteQuery(QueryParams prms)
        {
            if (prms.IUD != "D") return "";
            string DeleteQueryString = string.Format(" DELETE FROM [{0}]" + Environment.NewLine, prms.TableName);
            DeleteQueryString += string.Format(" WHERE {0}" + Environment.NewLine, string.Join(Environment.NewLine + "AND\t", Array.ConvertAll<QueryParamValue, string>(GetQueryParamValues(prms, true), itm => itm.ToString())));
            return DeleteQueryString;
        }

        internal static string InsertQuery(QueryParams prms)
        {
            if (prms.IUD != "I") return "";
            string InsertQueryString = string.Format("INSERT INTO [{0}]" + Environment.NewLine, prms.TableName);
            InsertQueryString += string.Format("\t({0})" + Environment.NewLine, string.Join(",", GetKeys(prms, false)));
            InsertQueryString += string.Format("VALUES" + Environment.NewLine);
            InsertQueryString += string.Format("\t({0})" + Environment.NewLine, string.Join(",", (from v in prms.Values where v.Header.Identity == false select "" + v.Value).ToArray()));
            return InsertQueryString;
        }

        internal static string UpdateQuery(QueryParams prms)
        {
            if (prms.IUD != "U") return "";
            string UpdateQueryString = string.Format("UPDATE [{0}] SET" + Environment.NewLine, prms.TableName);
            UpdateQueryString += string.Format("\t{0}" + Environment.NewLine, string.Join(Environment.NewLine + ",\t", Array.ConvertAll<QueryParamValue, string>(GetQueryParamValues(prms, false), itm => itm.ToString())));
            UpdateQueryString += string.Format("WHERE\t{0}" + Environment.NewLine, string.Join(Environment.NewLine + "AND\t", Array.ConvertAll<QueryParamValue, string>(GetQueryParamValues(prms, true), itm => itm.ToString())));
            return UpdateQueryString;
        }

        internal static string[] GetKeys(QueryParams prms)
        {
            return GetKeys(prms, null);
        }

        internal static string[] GetKeys(QueryParams prms, bool? hasNotIdentity)
        {
            var s = from i in prms
                    where i.Header.Identity == (hasNotIdentity ?? i.Header.Identity)
                    select string.Format("[{0}]", i.Header.Key);
            return s.ToArray();
        }

        internal static QueryParamValue[] GetQueryParamValues(QueryParams prms, bool IsPrimaryKey)
        {
            if (prms.OriginalHeaders != null)
            {
                var s = from i in prms
                        where (prms.OriginalHeaders.PrimaryItems.IndexOf(i.Header) >= 0) == IsPrimaryKey
                        select i;
                return s.ToArray();
            }
            else
            {
                var s = from i in prms
                        where i.IsPrimaryKey
                        select i;
                return s.ToArray();
            }
        }
    }

}
