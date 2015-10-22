using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using ASPChatProject.Models;
using ASPChatProject.Global;
using MongoDB.Driver;

namespace ASPChatProject.Controllers
{
    public class ValidationController : Controller
    {
        // GET: Validation
        public JsonResult UNExist(RegisterUserModel user)
        {
            var cnt = MongoDBConnector.Users.CountAsync(Builders<ChatUserModel>.Filter.Eq(x => x.UserName, user.UserName)).Result;
            if (cnt > 0)
                return Json("Error Name!", JsonRequestBehavior.AllowGet);
            else return Json(true, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CNExist(AddChatModel chat)
        {
            var cnt = MongoDBConnector.Chats.CountAsync(Builders<Chat>.Filter.Eq(x => x.name, chat.name)).Result;
            if (cnt > 0)
                return Json("Error Name!", JsonRequestBehavior.AllowGet);
            else return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}