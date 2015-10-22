using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ASPChatProject.Models;
using ASPChatProject.Global;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace ASPChatProject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (CurrentUser != null)
            {
                return RedirectToAction("StartRoom");
            }
            else
                return View();

        }


        public ActionResult AdminRoom()
        {
            var user = CurrentUser;
            if(user != null)
                if (user.IsAdmin)
                    return View(user);
            return RedirectToAction("ErrorPage", "Home", ErrorModel.AccessDenied);
        }

        public ActionResult UserProperties(string name)
        {
            var user = CurrentUser;
            if(user != null)
            {
                if (user.IsAdmin)
                {
                    var up = MongoDBConnector.GetUser(name);
                    if (up != null)
                        return View(new WebChat() { CurrentUser = up });
                }
            }
            return RedirectToAction("ErrorPage", "Home", ErrorModel.AccessDenied); 
                       
        }

        [HttpPost]
        public ActionResult SetAdminRight(string UserName)
        {
            if (CurrentUser.IsAdmin)
            {
                string ResponseMessage;
                var filter = Builders<ChatUserModel>.Filter.Eq(x => x.UserName, UserName);
                var user = MongoDBConnector.GetUser(UserName);
                if (user != null)
                {
                    MongoDBConnector.Users.UpdateOneAsync(filter, Builders<ChatUserModel>.Update.Set(x => x.IsAdmin, true));
                    ResponseMessage = "OK";
                }
                else
                    ResponseMessage = "Пользователь не найден!";
                return Content(ResponseMessage);
            }
            else
                return RedirectToAction("ErrorPage", "Home", ErrorModel.AccessDenied);
        }
        

        [HttpPost]
        public ActionResult SetAccessToRoom(string UserName, string ChatName)
        {
            if(CurrentUser.IsAdmin)
            {
                string ResponseMessage;
                var chat = MongoDBConnector.Chats.Find(Builders<Chat>.Filter.Eq(x => x.name, ChatName)).ToListAsync().Result;
                if (chat.Count > 0)
                {
                    var user = MongoDBConnector.GetUser(UserName);
                    if (user != null)
                    {
                        if (!chat.First().AllowedUser.Contains(user.UserName))
                        {
                            if(chat.First().AllowedUser.Count + 1 <= chat.First().MaxCount) 
                            {
                                MongoDBConnector.Chats.UpdateOneAsync(
                                    Builders<Chat>.Filter.Eq(x => x.name, ChatName),
                                    Builders<Chat>.Update.AddToSet(y => y.AllowedUser, user.UserName)
                                    );
                                ResponseMessage = "OK";
                            }
                            else
                                ResponseMessage = "Чат заполнен!";
                        }
                        else 
                            ResponseMessage = "Доступ уже предоставлен!";
                    }
                    else
                        ResponseMessage = "Пользователь не найден!";
                }
                else
                    ResponseMessage = "Чат не найден!";
                return Content(ResponseMessage);
            }
            else
                return RedirectToAction("ErrorPage", "Home", ErrorModel.AccessDenied);

        }

        [HttpPost]
        public ActionResult AddNewChat(AddChatModel model)
        {
            var res = MongoDBConnector.Chats.Find(Builders<Chat>.Filter.Eq(x => x.name, model.name)).ToListAsync().Result;
            if (model != null && model.maxcount != null && model.name != null)
            {
                int r;
                if (int.TryParse(model.maxcount, out r))
                {
                    if (Regex.IsMatch(model.name, @"^[\d \w]+$") && res.Count == 0)
                    {
                        MongoDBConnector.Chats.InsertOneAsync(new Chat { name = model.name, AllowedUser = new List<string>(), MaxCount = r });
                        Response.StatusCode = 200;
                        return Content("Успех");
                    }
                }
            }
            Response.StatusCode = 500;
            if(res.Count > 0)
                return Content("Чат с таким названием уже существует");
            else
                return Content("Неверное название либо отсутствуют необходимые параметры");
        }

        [HttpPost]
        public ActionResult AddNewMessage(MessageModel model)
        {
            if (model.MessageText != null)
                if (!string.IsNullOrWhiteSpace(model.MessageText.Trim()))
                {
                    var user = CurrentUser;
                    if (user != null)
                    {
                        var chat = MongoDBConnector.Chats.Find(Builders<Chat>.Filter.Eq(x => x.name, model.FromChat)).ToListAsync().Result;
                        if (chat.First().AllowedUser.Contains(user.UserName) || user.IsAdmin)
                        {
                            model.IsSystem = false;
                            model.Sender = user.UserName;
                            model.Time = DateTime.UtcNow;
                            MongoDBConnector.Messages.InsertOneAsync(model);
                            MongoDBConnector.SetUserLastActionTime(model.Sender, model.Time);
                            Response.StatusCode = 200;
                            return Json(true);
                        }
                    }
                }
            Response.StatusCode = 400;
            return Json(false);
        }

        [HttpPost]
        public ActionResult GetCurrentState()
        {
            WebChat wc = new WebChat() { CurrentUser = CurrentUser };
            return PartialView(wc);
        }

        [HttpPost]
        public ActionResult GetChatMessages(string chatName)
        {
            var user = CurrentUser;
            if (user != null)
            {
                MongoDBConnector.SetUserLastActionTime(user.UserName, DateTime.UtcNow);
                var res = MongoDBConnector.GetMessagesFromChat(chatName).OrderByDescending(x => x.Time).Take(100).OrderBy(y => y.Time);
                return PartialView(new Tuple<IEnumerable<MessageModel>, bool>(res, user.IsAdmin));
            }
            return RedirectToAction("ErrorPage", "Home", ErrorModel.AccessDenied);
        }

        [HttpPost]
        public ActionResult DeleteMessage(string id)
        {
            var user = CurrentUser;
            if (user != null)
            {
                if (user.IsAdmin)
                {
                    MongoDBConnector.Messages.DeleteOneAsync(Builders<MessageModel>.Filter.Eq(x => x._id, ObjectId.Parse(id)));
                    return Content("OK");
                }
            }
            return RedirectToAction("ErrorPage", "Home", ErrorModel.AccessDenied);
        }

        
        [HttpPost]
        public ActionResult DeleteUserFromChat(string UserName, string Chat)
        {
            var user = CurrentUser;
            if (user != null)
            {
                if (user.IsAdmin)
                {
                    MongoDBConnector.Chats.UpdateOneAsync(
                        Builders<Chat>.Filter.Eq(x => x.name, Chat),
                        Builders<Chat>.Update.Pull(y => y.AllowedUser, UserName));
                    return Content("OK");
                }
            }
            return RedirectToAction("ErrorPage", "Home", ErrorModel.AccessDenied);
        }


        public ActionResult StartRoom()
        {
            ChatUserModel user = CurrentUser;
            WebChat WC = new WebChat() { CurrentUser = user };
            if (user != null)
            {
                MongoDBConnector.SetUserLastActionTime(user.UserName, DateTime.UtcNow);
                if (!user.IsAdmin)
                    WC.AllChats = WC.AllChats.Where(c => c.AllowedUser.Contains(user.UserName)).ToList();
                return View(WC);
            }
            else
                return RedirectToAction("ErrorPage", "Home", ErrorModel.LoginOrRegistration);
        }
        public ActionResult LogOff()
        {
            this.LogOffCurrentUser();
            return View("Index");
        }

        [HttpGet]
        public ActionResult ErrorPage(ErrorModel err)
        {
            return View(err);
        }


        public ActionResult Chat(string name)
        {
            var chat = MongoDBConnector.Chats.Find(Builders<Chat>.Filter.Eq(x => x.name, name)).ToListAsync().Result;
            var user = CurrentUser;
            if (chat.Count == 0)
            {
                return RedirectToAction("ErrorPage", "Home", ErrorModel.ChatDoesntExist );
            }
            else
            {
                if (user != null)
                {
                    MongoDBConnector.SetUserLastActionTime(user.UserName, DateTime.UtcNow);
                    if (chat.First().AllowedUser.Contains(user.UserName) || user.IsAdmin)
                    {
                        MongoDBConnector.SetUserLastActionTime(user.UserName, DateTime.UtcNow);
                        var sysMessage = new MessageModel
                        {
                            IsSystem = true,
                            MessageText = string.Format("{0} вошел в чат", user.UserName),
                            Sender = user.UserName,
                            Time = DateTime.UtcNow,
                            FromChat = name
                        };
                        MongoDBConnector.Messages.InsertOneAsync(sysMessage);
                        return View(new Tuple<ChatUserModel, Chat>(user, chat.First()));
                    }
                }
                return RedirectToAction("ErrorPage", "Home", ErrorModel.AccessDenied );
            }
        }

        public ChatUserModel CurrentUser
        {
            get
            {
                try
                {
                    if (Request.Cookies.Count != 0 && Request.Cookies["UserToken"].Value != null)
                        return MongoDBConnector.Users.Find(Builders<ChatUserModel>.Filter.Eq(x => x._id, ObjectId.Parse(Request.Cookies["UserToken"].Value))).FirstAsync().Result;
                    else
                        return null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return null;
                }
            }
        }


    }
}