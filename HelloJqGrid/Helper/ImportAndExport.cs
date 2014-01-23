using MvcJqGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Helper
{
    public class ImportAndExport
    {
        //从web.config中取值
        public static string sqlConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        /// <summary>导入Excel数据</summary>
        /// <param name="filePath">上传文件路径</param>
        /// <param name="dbTableName">数据库表名</param>
        /// <param name="columnMapping">数据表列映射</param>
        public static void ImportExcel(string filePath, string dbTableName, List<string> columnMapping = null)
        {
            //Create connection string to Excel work book
            string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1';";
            //Create Connection to Excel work book
            using (OleDbConnection excelConnection = new OleDbConnection(excelConnectionString))
            {
                using (OleDbCommand excelCommand = new OleDbCommand("Select * from [Sheet1$]", excelConnection))
                {
                    excelConnection.Open();//打开连接
                    OleDbDataReader excelReader = excelCommand.ExecuteReader();
                    using (DataTable dt = new DataTable())
                    {
                        dt.Load(excelReader);//转为DataTable
                        //已映射的列会传递给批量写入模块，此处只添加不在Excel表格中出现的字段，如一些共有字段或特殊字段。
                        dt.Columns.Add(new DataColumn("CreatedBy", typeof(System.String)));
                        dt.Columns.Add(new DataColumn("CreatedOn", typeof(System.DateTime)));

                        //填充字段值        
                        foreach (DataRow dr in dt.Rows)
                        {
                            dr["CreatedBy"] = "Test";
                            dr["CreatedOn"] = DateTime.Now;
                        }

                        try
                        {//包含增加的字段映射传递给批量写入模块
                            BatchCopy0(dt, dbTableName, columnMapping);
                        }
                        catch (Exception)
                        {
                            excelCommand.Dispose();
                            excelConnection.Close();//关闭excel连接
                            throw;
                        }
                    }
                    excelConnection.Close();//关闭连接
                }
            }
        }

        /// <summary>导出到Excel（use WebControl）</summary>
        /// <param name="ctrl">包含数据的控件</param>
        /// <param name="fileName">文件名</param>
        public static void ExportToExcel(WebControl ctrl, string fileName)
        {
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ContentType = "application/ms-excel";
            HttpContext.Current.Response.Charset = "UTF-8";
            HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
            //HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + "" + fileName + ".xls");
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName, System.Text.Encoding.UTF8) + ".xls");//这样的话，可以设置文件名为中文，且文件名不会乱码。其实就是将汉字转换成UTF8
            HttpContext.Current.Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());

            StringWriter sw = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htw = new System.Web.UI.HtmlTextWriter(sw);
            ctrl.RenderControl(htw);
            HttpContext.Current.Response.Write(sw.ToString());
            HttpContext.Current.Response.End();
        }

        /// <summary>导出到Excel（use DataTable）
        /// </summary>
        /// <param name="dt"></param>
        public static void ExportToExcel(DataTable dt, string fileName)
        {
            string outputFileName = null;  
            string browser = HttpContext.Current.Request.UserAgent.ToUpper();  
  
            //消除文件名乱码。如果是IE则编码文件名，如果是FF则在文件名前后加双引号。
            if (browser.Contains("MS") == true && browser.Contains("IE") == true)
                outputFileName = HttpUtility.UrlEncode(fileName);  //%e5%90%8d%e5%8d%95
            else if (browser.Contains("FIREFOX") == true)  
                outputFileName = "\"" + fileName + ".xls\"";  //"名单.xls" 
            else  
                outputFileName = HttpUtility.UrlEncode(fileName);

            HttpResponse Response = HttpContext.Current.Response;

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=" + outputFileName + ".xls");
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("gb2312");
            Response.Charset = "gb2312";
            Response.ContentType = "application/ms-excel";

            string tab = "";
            foreach (DataColumn dc in dt.Columns)
            {
                HttpContext.Current.Response.Write(tab + dc.ColumnName);
                tab = "\t";
            }
            HttpContext.Current.Response.Write("\n");

            int i;
            foreach (DataRow dr in dt.Rows)
            {
                tab = "";
                for (i = 0; i < dt.Columns.Count; i++)
                {
                    HttpContext.Current.Response.Write(tab + dr[i].ToString());
                    tab = "\t";
                }
                HttpContext.Current.Response.Write("\n");
            }
            HttpContext.Current.Response.End();
        }

        /// <summary>复制数据-不带事务的</summary>
        /// <param name="dt">源数据</param>
        /// <param name="tbName">目标数据表名称</param>
        /// <param name="columnMapping">完整的字段映射</param>
        public static void BatchCopy0(DataTable dt, string tbName, List<string> columnMapping)
        {
            using (SqlBulkCopy sbc = new SqlBulkCopy(sqlConnectionString, SqlBulkCopyOptions.KeepNulls))
            {
                sbc.DestinationTableName = tbName;

                if (columnMapping != null)
                {
                    foreach (var mapping in columnMapping)
                    {
                        var split = mapping.Split(new[] { ',' });
                        sbc.ColumnMappings.Add(split.First(), split.Last());
                    }
                }
                try
                {
                    sbc.WriteToServer(dt);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>复制数据-带事务</summary>
        /// <param name="dt">DataTable</param>
        /// <param name="destinationTableName">目标数据表名称</param>
        /// <param name="columnMapping">映射列</param>
        public static void BatchCopy(DataTable dt, string destinationTableName, List<string> columnMapping)
        {
            //bool IsOK = true;
            using (SqlConnection cnn = new SqlConnection(sqlConnectionString))
            {
                cnn.Open();//打开连接
                using (SqlTransaction tran = cnn.BeginTransaction())//数据库级别的事务
                {
                    using (SqlBulkCopy sqlCopy = new SqlBulkCopy(cnn, SqlBulkCopyOptions.KeepIdentity, tran))
                    //using (SqlBulkCopy sqlCopy = new SqlBulkCopy(cnn))
                    {
                        sqlCopy.DestinationTableName = destinationTableName;
                        if (columnMapping != null)
                        {
                            foreach (var mapping in columnMapping)
                            {
                                var split = mapping.Split(new[] { ',' });
                                sqlCopy.ColumnMappings.Add(split.First(), split.Last());
                            }
                        }
                        try
                        {
                            sqlCopy.WriteToServer(dt);//写入数据库
                            tran.Commit();//提交事务
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();//回滚事务
                            cnn.Close();    //关闭数据库连接
                            throw ex;       //抛出异常
                            //IsOK = false;
                        }
                    }
                }
                cnn.Close();//关闭连接
            }
            //return IsOK;
        }

        /// <summary>字段映射</summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="columnMapping">输出映射表</param>
        /// <param name="tbName">数据库表名</param>
        public static void MappingColumn(string fileName, out string tbName, out List<string> columnMapping)
        {
            tbName = "";
            columnMapping = new List<string>();

            if (fileName != null)
            {
                switch (fileName)
                {
                    case "Member":
                        tbName = "Member";
                        columnMapping.Add("姓名,Name");
                        columnMapping.Add("邮箱,Email");
                        columnMapping.Add("生日,Birthday");
                        columnMapping.Add("年龄,Age");
                        break;
                    case "Guestbook":
                        tbName = "Guestbooks";
                        columnMapping.Add("消息,Message");
                        break;
                    default:
                        break;
                }
                ////以下字段未在Excel表格中出现，需传给SqlBulkCopy。
                columnMapping.Add("CreatedBy,CreatedBy");
                columnMapping.Add("CreatedOn,CreatedOn");
            }
        }

        /// <summary>循环上传文件写入数据库(Uploadify)</summary>
        /// <param name="server">Web服务器</param>
        /// <returns name="str">上传文件名及上传情况</returns>
        public static string BatchUpload(HttpServerUtilityBase server)
        {
            var folder = server.MapPath("~/Uploads");
            var tbName = "";
            string[] files = Directory.GetFiles(folder);

            string str = "";
            if (files.Length != 0)
            {
                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    var columnMapping = new List<string>();
                    switch (fileName)
                    {
                        case "Member":
                            tbName = "Member";
                            columnMapping.Add("姓名,Name");
                            columnMapping.Add("邮箱,Email");
                            columnMapping.Add("生日,Birthday");
                            columnMapping.Add("年龄,Age");
                            break;
                        case "Guestbook":
                            tbName = "Guestbooks";
                            columnMapping.Add("消息,Message");
                            break;
                        default:
                            break;
                    }
                    try
                    {
                        ////以下字段未在Excel表格中出现，需传给SqlBulkCopy。
                        columnMapping.Add("CreatedBy,CreatedBy");
                        columnMapping.Add("CreatedOn,CreatedOn");

                        ImportAndExport.ImportExcel(file, tbName, columnMapping);
                        System.IO.File.Delete(file);//文件正由另一进程使用，因此该进程无法访问此文件。
                        str += "<p style='background-color:#5DA30C'>写入成功: " + fileName + "</p>";

                        //想使用ViewBag或ViewData有问题，似乎和ajax有关：
                        //If you are making an ajax post to the controller action Refresh() and returning Json data, 
                        //the page does not reload and the ViewBag is not opened.  
                        //If you post the page and return a view, then you will probably get the result you want.  
                        //If you want to do it with ajax, then send the boolean with the json, open it on the client in the ajax success callback and use jquery to show/hide the part of the page you want.
                        //http://forums.asp.net/t/1848093.aspx
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(file);
                        str += "<p style='background-color:orange'>写入失败: " + fileName + " [" + ex.Message + "]</p>";
                    }
                }
            }
            return str;
        }

        /// <summary>泛型转换为DataTable</summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="data">泛型列表</param>
        /// <returns>返回一个DataTable对象</returns>
        // public static DataTable ConvertToDatatable<T>(IList<T> data)//(this IList<T> data) remove "this" if not on C# 3.0 / .NET 3.5*/
        public static DataTable ConvertToDatatable<T>(IEnumerable<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();

            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                //table.Columns.Add(prop.Name, prop.PropertyType);//出错！DataSet 不支持 System.Nullable<>。
                //因为 DataColumn 不支持 Nullable<T> 类型，空值只能使用DBNull。
                table.Columns.Add(prop.Name);
            }

            object[] values = new object[props.Count];

            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        /// <summary>为JqGrid导出到Excel提供数据(输出参数和return哪个更有效率？)</summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="grid">表格属性</param>
        /// <param name="query">数据源</param>
        /// <param name="data">输出给Excel的数据，是一个泛型List类型"/></param>
        public static void ForExcel<T>(GridSettings grid, IQueryable<T> query, out List<T> data)
        {
            //filtring
            if (grid.IsSearch)  //_search == true
            {
                //And
                if (grid.Where.groupOp == "AND")
                    foreach (var rule in grid.Where.rules)
                        query = query.Where<T>(rule.field, rule.data, (WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule.op));
                else
                {  //更新--2013.10.26
                    IQueryable<T> temp = null;
                    foreach (var rule in grid.Where.rules)
                    {
                        var rule1 = rule;
                        var t = query.Where<T>(rule1.field, rule1.data, (WhereOperation)StringEnum.Parse(typeof(WhereOperation), rule1.op));

                        if (temp == null)
                            temp = t;
                        else
                            //temp = temp.Union(t); // Union!!
                            temp = temp.Concat<T>(t);//使用union会导致查不出数据
                    }
                    query = temp.Distinct<T>();
                }
            }
            //sorting
            data = query.OrderBy<T>(grid.SortColumn, grid.SortOrder).ToList();
        }
    }
}