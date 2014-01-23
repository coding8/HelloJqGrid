using MvcJqGrid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Helper
{
    public class JqGridHelper
    {
        /// <summary>返回JsonResult对象</summary>
        /// <typeparam name="T">实体或视图模型</typeparam>
        /// <param name="grid">grid属性</param>
        /// <param name="query">传入查询参数</param>
        /// <returns>JsonResult</returns>
        public static JsonResult GetQuery<T>(GridSettings grid, IQueryable<T> query)
        {
            //filtring
            if (grid.IsSearch)
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
            query = query.OrderBy<T>(grid.SortColumn, grid.SortOrder);

            //count
            var count = query.Count();

            //paging
            var data = query.Skip((grid.PageIndex - 1) * grid.PageSize).Take(grid.PageSize).ToList();

            //--------------------------
            var result = new
            {
                total = (int)Math.Ceiling((double)count / grid.PageSize),
                page = grid.PageIndex,
                records = count,
                rows = (from d in data
                        select d
                       ).ToArray()
            };
            //实例化JsonResult
            JsonResult jr = new JsonResult();
            jr.Data = result;
            jr.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jr;
        }

        /// <summary>json样式的字符串</summary>
        /// <typeparam name="T">实体或视图模型</typeparam>
        /// <param name="grid">grid属性</param>
        /// <param name="query">传入查询参数</param>
        /// <returns></returns>
        public static string GetJsonResult<T>(GridSettings grid, IQueryable<T> query)
        {
            //filtring
            if (grid.IsSearch)
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
            query = query.OrderBy<T>(grid.SortColumn, grid.SortOrder);

            //count
            var count = query.Count();

            //paging
            var data = query.Skip((grid.PageIndex - 1) * grid.PageSize).Take(grid.PageSize).ToList();

            //-2013.11.3 抽出result对象，然后转为json字符串传递给Controller中的GetData方法-------------------------
            var result = new
            {
                total = (int)Math.Ceiling((double)count / grid.PageSize),
                page = grid.PageIndex,
                records = count,
                rows = (from d in data
                        select d    //用viewmodel/model的字段，通过jqgrid的字段增减。
                       ).ToArray()
            };

            //return System.Web.Mvc.Controller.Json(result, JsonRequestBehavior.AllowGet);//错误	3	“System.Web.Mvc.Controller.Json(object)”不可访问，因为它受保护级别限制	

            return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        }

        /// <summary>输出筛选后的记录</summary>
        /// <param name="gird">Grid设置类</param>
        /// <param name="query">IQueryable类型的实体</param>
        /// <returns>返回一个泛型列表对象</returns>
        public static List<T>  GetFilteredData<T>(GridSettings grid, IQueryable<T> query)
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
            List<T> listData;
            return listData = query.OrderBy<T>(grid.SortColumn, grid.SortOrder).ToList();
        }
    }
}