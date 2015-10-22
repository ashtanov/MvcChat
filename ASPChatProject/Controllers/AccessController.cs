using ASPChatProject.Global;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ASPChatProject.Models
{
    public class AccessController : Controller
    {
        // GET: Registr
        [HttpGet]
        public ActionResult Create()
        {
            var um = new RegisterUserModel();
            return View(um);
        }
        [HttpPost]
        public ActionResult Create(RegisterUserModel um)
        {
            var ph = MD5Hasher.GetHash(um.Password);
            var user = new ChatUserModel { _id = ObjectId.GenerateNewId(), IsAdmin = false, LastAction = DateTime.UtcNow, PasswordHash = ph, UserName = um.UserName};
            MongoDBConnector.Users.InsertOneAsync(user);
            this.AddCookieForCurrentUser(user);
            return RedirectToAction("StartRoom","Home",null);
        }

        [HttpGet]
        public ActionResult Login()
        {
            var ul = new LoginUserModel();
            return View(ul);
        }
        [HttpPost]
        public ActionResult Login(LoginUserModel ul)
        {
            var ph = MD5Hasher.GetHash(ul.Password);
            var userinbase = MongoDBConnector.Users
                .Find(
                Builders<ChatUserModel>.Filter.And(new FilterDefinition<ChatUserModel>[] { 
                     Builders<ChatUserModel>.Filter.Eq(x => x.UserName,ul.UserName),
                     Builders<ChatUserModel>.Filter.Eq(x => x.PasswordHash,ph)})).ToListAsync().Result;

            if (userinbase.Count > 0)
            {
                MongoDBConnector.SetUserLastActionTime(userinbase.First().UserName, DateTime.UtcNow);
                this.AddCookieForCurrentUser(userinbase.First());
                return RedirectToAction("StartRoom", "Home", null);
            }
            else
                return RedirectToAction("ErrorPage", "Home", new ErrorModel { ErrorMessage = "Пользователя не существует либо пароль не верный" });
        }
    }
}