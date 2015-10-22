using ASPChatProject.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ASPChatProject.Global
{
    public class MongoDBConnector
    {
        static IMongoClient _cli;
        static IMongoDatabase _db;
        static MongoDBConnector()
        {
            _cli = new MongoClient(ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString);
            _db = _cli.GetDatabase("WebChat");
        }
        public static IMongoDatabase Database
        {
            get
            {
                return _db;
            }
        }
        public static IMongoCollection<ChatUserModel> Users
        {
            get
            {
                return _db.GetCollection<ChatUserModel>("Users");
            }
        }

        public static ChatUserModel GetUser(string UserName)
        {
            var filter = Builders<ChatUserModel>.Filter.Eq(x => x.UserName, UserName);
            var user = MongoDBConnector.Users.Find(filter).ToListAsync().Result;
            if (user.Count > 0)
                return user.First();
            else
                return null;
        }

        public static void SetUserLastActionTime(string UserName, DateTime ActivityTime)
        {
            Users.FindOneAndUpdateAsync<ChatUserModel>(
                    Builders<ChatUserModel>.Filter.Eq(x => x.UserName, UserName), 
                    Builders<ChatUserModel>.Update.Set(y => y.LastAction, ActivityTime)
                    );
        }
        public static List<MessageModel> GetMessagesFromChat(string chatName)
        {
            return Messages.Find(Builders<MessageModel>.Filter.Eq(x => x.FromChat, chatName)).ToListAsync().Result;
        }

        public static IMongoCollection<MessageModel> Messages
        {
            get
            {
                return _db.GetCollection<MessageModel>("Messages");
            }
        }
        public static IMongoCollection<Chat> Chats
        {
            get
            {
                return _db.GetCollection<Chat>("Chats");
            }
        }
        private  MongoDBConnector()
        {
        }
    }
}