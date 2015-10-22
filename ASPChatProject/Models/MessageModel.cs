using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASPChatProject.Models
{
    public class MessageModel
    {
        public ObjectId _id { get; set; } 
        public bool IsSystem { get; set; }
        public string MessageText { get; set; }
        public string Sender { get; set; }
        public DateTime Time { get; set; }
        public string FromChat { get; set; }
    }
}
