using System;
using System.Collections;
using System.Web.Mvc;
using System.Linq;
using HelloJqGrid.Models;
using HelloJqGrid.ViewModel;
using System.Linq.Dynamic;
using MvcJqGrid;
using System.Collections.Generic;
using System.Web;
using System.IO;

namespace HelloJqGrid.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();

        }
        public ActionResult ShowLocalData()
        {
            return View();
        }

        public JsonResult GetLocalData(int? page, int? rows)
        {  //把数据放到一个动态数组里
            ArrayList list = new ArrayList();
            list.Add(new { id = "1", invdate = "2007-10-01", name = "test" });
            list.Add(new { id = "2", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "3", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "4", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "5", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "6", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "7", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "8", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "9", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "10", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "11", invdate = "2007-10-02", name = "test" });
            list.Add(new { id = "12", invdate = "2007-10-02", name = "test" });

            var myData = list.ToArray();//便于下面用linq分页

            //jqgrid的参数
            int pageNum = page.HasValue ? page.Value : 1;//当前显示哪一页
            int pageSize = rows.HasValue ? rows.Value : 10;  //每一页显示多少条记录
            int totalRecords = list.Count;//总记录数
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);//总页数
            var jsonData = new
            {
                total = totalPages,
                page = pageNum,
                records = totalRecords,
                rows = myData.Skip((pageNum - 1) * pageSize).Take(pageSize)//分页
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowServerData()
        {
            return View();
        }
        public ActionResult GetServerData(int? page, int? rows, string sidx, string sord)
        {
            MyContext db = new MyContext();
            var query = db.Members.ToList();

            //jqgrid的参数
            int pageNum = page.HasValue ? page.Value : 1;//当前显示哪一页
            int pageSize = rows.HasValue ? rows.Value : 10;  //每一页显示多少条记录
            int totalRecords = db.Members.Count();//总记录数
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);//总页数

            var jsonData = new
            {
                total = totalPages,
                page = pageNum,
                records = totalRecords,
                rows = query.Skip((pageNum - 1) * pageSize).Take(pageSize)//分页
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        //排序
        public ActionResult ShowSorting()
        {
            return View();
        }
        public ActionResult GetSortingData(JqGridViewModel grid)
        {
            MyContext db = new MyContext();
            var query = db.Members.AsQueryable();

            int pageNum = grid.page;  //当前显示哪一页
            int pageSize = grid.rows;  //每一页显示多少条记录
            int totalRecords = query.Count();//总记录数
            int totalPages = (int)Math.Ceiling((float)totalRecords / (float)pageSize);//总页数

            //排序-若出现：No property or field 'asc' exists in type 'Member'
            //解决办法：检查jqgrid里设置sortorder和sortname参数没有。
            query = query.OrderBy(grid.sidx + " " + grid.sord);

            var jsonData = new
            {
                total = totalPages,
                page = pageNum,
                records = totalRecords,
                rows = query.Skip((pageNum - 1) * pageSize).Take(pageSize)//分页
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult GetSortingData(GridSettings grid)
        //{          
        //    MyContext db = new MyContext();
        //    var query = db.Members as IQueryable<Member>;

        //    //return GridSearchHelper.GetJsonResult<Member>(grid, query);
        //    return GridSearchHelper.GetQuery(grid, query);
        //}

        public ActionResult CRUD()
        {
            return View();
        }

        //Add
        public ActionResult Add(Member m)
        {
            Member member = new Member();
            member.No = m.No;
            member.Name = m.Name;
            member.Email = m.Email;
            member.Birthday = m.Birthday;
            member.CreatedOn = m.CreatedOn;

            MyContext db = new MyContext();
            db.Members.Add(member);
            db.SaveChanges();
            return Json(new
            {
                msg = "success",
                No = member.No,    //返回给前端序号
                CreatedOn = member.CreatedOn//返回给前端
            });
        }

        //delete
        public ActionResult Delete(Member m)
        {
            MyContext db = new MyContext();
            Member member = db.Members.First(mb => mb.No == m.No);

            db.Members.Remove(member);

            db.SaveChanges();
            return Json(new { msg = "success" });
        }

        //update
        public ActionResult Update(Member m)
        {
            MyContext db = new MyContext();
            //Member member = db.Members.First(mb => mb.No == m.No);

            //member.Name = m.Name.Trim();
            //member.Email = m.Email;
            //member.Birthday = m.Birthday;
            //member.CreatedOn = m.CreatedOn;
            //db.SaveChanges();

            Member member = db.Members.Find(m.No);//Find查找主键
            //只更新修改的属性
            db.Entry(member).CurrentValues.SetValues(m);
            db.SaveChanges();

            return Json(new { msg = "success" });
        }
        //为客户端选中行提供数据
        //public ActionResult GetRowData(Member m)
        //{
        //    MyContext db = new MyContext();
        //    Member member = db.Members.Find(m.No);
        //    if (member == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    //返回member对象
        //    return Json(member, JsonRequestBehavior.AllowGet);
        //}
        public string GetRowData(Member m)
        {
            MyContext db = new MyContext();
            Member member = db.Members.Find(m.No);

            return Newtonsoft.Json.JsonConvert.SerializeObject(member);
        }

        //单条件查询
        public ActionResult SingleSearch()
        {
            return View();
        }
        public ActionResult GetSearchData(GridSettings grid)
        {
            MyContext db = new MyContext();
            var query = db.Members as IQueryable<Member>;

            return GridSearchHelper.GetQuery(grid, query);
        }

        //多条件查询
        public ActionResult MultipleSearch()
        {
            return View();
        }

        //subGrid
        public ActionResult SubGrid()
        {
            return View();
        }
        public ActionResult GetSubGridData(GridSettings grid)
        {
            MyContext db = new MyContext();
            var query = db.Guestbooks as IQueryable<Guestbook>;
            var id = Convert.ToInt32(Request.QueryString["MemberId"]);
            query = from g in query
                    where g.Members.No == id
                    select g;
            return GridSearchHelper.GetQuery(grid, query);
        }

        //MasterDetail
        public ActionResult MasterDetail()
        {
            return View();
        }
        public ActionResult GetDetailGridData(GridSettings grid, int id = 0)
        {
            MyContext db = new MyContext();
            var query = db.Guestbooks as IQueryable<Guestbook>;
            query = from g in query
                    where g.Members.No == id
                    select g;

            return GridSearchHelper.GetQuery(grid, query);
        }

        //上传文件
        [HttpGet]
        public ActionResult UploadFile() { return View(); }
        [HttpPost]
        public ActionResult UploadFile(IEnumerable<HttpPostedFileBase> uploadfile)//TODO:加入判断及错误捕捉
        {
            ViewBag.msg = "";
            foreach (var file in uploadfile)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                    //如遇相同文件则先删除再保存
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                    file.SaveAs(path);
                }
            }
            //导入excel数据，从文件夹中依次读取文件
            var folder = Server.MapPath("~/Uploads");
            var tbName = "";
            string[] files = Directory.GetFiles(folder);
            if (files.Length != 0)
            {
                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    switch (fileName)
                    {
                        case "Member":
                            tbName = "Member";
                            break;
                        default:
                            System.IO.File.Delete(file);
                            break;
                    }
                    try
                    {
                        //字段映射（Excel的列名与数据表字段对应）
                        var columnMapping = new List<string>();
                        columnMapping.Add("Name,Name");
                        columnMapping.Add("邮箱,Email");
                        columnMapping.Add("生日,Birthday");
                        columnMapping.Add("Age,Age");
                        //以下字段未在Excel表格中出现，需传给SqlBulkCopy。
                        columnMapping.Add("CreatedOn,CreatedOn");

                        ImportAndExport.ImportExcel(file, tbName, columnMapping);
                        System.IO.File.Delete(file);
                        return Json(new { success = true, message = "导入成功！", fileName = fileName }, "text/html");
                    }
                    catch (Exception ex)
                    {
                        System.IO.File.Delete(file);
                        return Json(new { success = false, message = "导入失败" + ex.Message, fileName = fileName }, "text/html");
                    }
                }
            }
            return View("UploadFile");
        }
    }
}