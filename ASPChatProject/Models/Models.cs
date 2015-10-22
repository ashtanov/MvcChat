using ASPChatProject.Global;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ASPChatProject.Models
{
    public class AddChatModel
    {
        [Required]
        [Display(Name = "Chat Name")]
        [Remote("CNExist", "Validation", ErrorMessage = "Плохое имя!")]
        public string name { get; set; }
        [Required]
        public string maxcount { get; set; }
    }


    public class RegisterUserModel
    {
        [Required]
        [Display(Name="User Name")]
        [Remote("UNExist","Validation", ErrorMessage="Плохое имя!")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name="Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Пароли не совпадают!")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginUserModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class WebChat
    {
        public ChatUserModel CurrentUser { get; set; }
        public List<Chat> AllChats { get; set; }
        public List<ChatUserModel> UsersOnline { get; private set; }
        public List<ChatUserModel> AllUsers { get; private set; }

        public WebChat()
        {
            AllChats = new List<Chat>();
            foreach (var chat in MongoDBConnector.Chats.Find(new BsonDocument()).ToListAsync().Result)
            {
                AllChats.Add(chat);
            }
            UsersOnline = new List<ChatUserModel>();
            AllUsers = new List<ChatUserModel>();

            var users = MongoDBConnector.Users.Find(new BsonDocument()).ToListAsync().Result;
            var online = users.Where(x => x.LastAction > DateTime.UtcNow.Add(new TimeSpan(0, -15, 0)));

            UsersOnline.AddRange(online);
            AllUsers.AddRange(users);
        }
    }

    public class Chat
    {
        [BsonId]
        public string name { get; set; }
        public List<string> AllowedUser { get; set; }
        public int MaxCount { get; set; }
    }

    public class ChatUserModel
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastAction { get; set; }
        public bool IsAdmin { get; set; }
    }
}