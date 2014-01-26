using Models;
using System.Data.Entity;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebMatrix.WebData;

namespace HelloJqGrid
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //避免数据库被自动创建或自动迁移(注意顺序：1、数据表 2、实体类）
            //System.Data.Entity.Database.SetInitializer<Models.MyContext>(null);
            
            //使用数据库初始化器自动迁移
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MyContext, MyConfiguration>());
            
            //Simplemembership
            WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", true);
        }
    }

    public class MyConfiguration : System.Data.Entity.Migrations.DbMigrationsConfiguration<MyContext>
    {
        public MyConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;//自动迁移
        }
    }
}