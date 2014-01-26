using Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace HelloJqGrid.Controllers
{
    public class UserManagementController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        //Add role 添加角色
        public ActionResult AddRole(string RoleName)
        {
            Roles.CreateRole(RoleName);
            return View("Index");
        }

        //Delete Role 删除角色
        public ActionResult DeleteRole(string RoleName)
        {
            Roles.DeleteRole(RoleName);
            return View("Index");
        }

        //Add user 注册用户
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            // 尝试注册用户
            try
            {
                WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                //WebSecurity.Login(model.UserName, model.Password);
                return Json(new { success = true, msg = "Add " + model.UserName + " success!" });
            }
            catch (MembershipCreateUserException e)
            {
                return Json(new { success = false, msg = "Add " + model.UserName + " fail: " + e.Message });
            }
        }

        //删除用户及成员
        [HttpPost]
        public ActionResult DeleteUser(string UserName, FormCollection form)
        {
            try
            {
                if (Roles.GetRolesForUser(UserName).Count() > 0)
                {
                    Roles.RemoveUserFromRoles(UserName, Roles.GetRolesForUser(UserName));
                }
                ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(UserName); // deletes record from webpages_Membership table
                ((SimpleMembershipProvider)Membership.Provider).DeleteUser(UserName, true); // deletes record from UserProfile table

                //return View("Index");
                return Json(new { success = true, msg = "Delete " + UserName + " success!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Delete " + UserName + " fail: " + ex.Message });
                //return View(UserName);
            }
        }

        //添加用户到角色
        public ActionResult AddUserToRole(string User, string Role)
        {
            try
            {
                Roles.AddUserToRole(User, Role);
                return Json(new { success = true, msg = "Add " + User + " Success!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Add " + User + " Fail: " + ex.Message });
            }

            //return View("Index");
        }

        //从角色中移除用户
        public ActionResult RemoveUserFromRole(string User, string Role)
        {
            try
            {
                Roles.RemoveUserFromRole(User, Role);
                return Json(new { success = true, msg = "Remove " + User + " Success!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Remove " + User + " Fail: " + ex.Message });
            }
        }

        //TODO 解锁用户

        //重置密码
        public ActionResult ResetPwd(string user)
        {
            var token = WebSecurity.GeneratePasswordResetToken(user);
            bool b = WebSecurity.ResetPassword(token, "123");
            if (b == true)
            {
                return Json(new { success = true, msg = "success！" });
            }
            else
            {
                return Json(new { success = false, msg = "error:" });
            }
        }
    }
}
