﻿using System;
using System.Collections.Generic;
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

        ///<summary>导入Excel数据</summary>
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

        ///<summary>复制数据-不带事务的</summary>
        /// <param name="dt">源数据</param>
        /// <param name="tbName">目标数据表名称</param>
        ///<param name="columnMapping">完整的字段映射</param>

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

        ///<summary>导出到Excel</summary>
        ///<param name="ctrl">包含数据的控件</param>
        ///<param name="fileName">文件名</param>
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

        ///<summary>字段映射</summary>
        ///<param name="fileName">文件名称</param>
        ///<param name="columnMapping">输出映射表</param>
        ///<param name="tbName">数据库表名</param>
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
                    case "COOIS":
                       //do sth.
                        break;
                    default:
                        break;
                }
                ////以下字段未在Excel表格中出现，需传给SqlBulkCopy。
                columnMapping.Add("CreatedBy,CreatedBy");
                columnMapping.Add("CreatedOn,CreatedOn");
            }
        }
    }
}