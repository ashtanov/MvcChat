using ASPChatProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASPChatProject.Global
{
    public static class CookieManager
    {
        public static void AddCookieForCurrentUser(this Controller contr, ChatUserModel model)
        {
            HttpCookie cc = new HttpCookie("UserToken", model._id.ToString());
            cc.Path = "/";
            cc.Expires = DateTime.Now.AddDays(7);                
            contr.Response.Cookies.Add(cc);
        }

        public static void LogOffCurrentUser(this Controller contr)
        {
            contr.Response.Cookies["UserToken"].Expires = DateTime.Now.AddMinutes(-10);
        }

    }
}