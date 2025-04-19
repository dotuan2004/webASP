using System.Web;
using System.Web.Mvc;
using LTW.Models;

namespace LTW.Areas.Admin
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = (KhachHang)httpContext.Session["TaiKhoan"];
            return user != null && user.RoleID == 1;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/NguoiDung/DangNhap");
        }
    }
}