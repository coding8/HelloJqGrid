using Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HelloJqGrid.Controllers
{
    public class UploadifyController : Controller
    {
        //
        // GET: /Uploadify/

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //uploadify是逐次触发Uploadify的，所以每次只有一个文件，不需要foreach，
        public ActionResult Uploadify(HttpPostedFileBase uploadfile)
        {
            if (uploadfile != null && uploadfile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(uploadfile.FileName);
                var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
                uploadfile.SaveAs(path);
            }
            return Content("");
        }
        public ContentResult WriteExcelToDatabase()
        {
            string str = ImportAndExport.BatchUpload(Server);
            return Content(str);
        }
    }
}
