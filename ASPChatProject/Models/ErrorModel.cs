using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASPChatProject.Models
{
    public class ErrorModel
    {
        public string ErrorMessage { get; set; }
        public static ErrorModel AccessDenied
        {
            get
            {
                return new ErrorModel { ErrorMessage = "Доступ запрещен!" };
            }
        }
        public static ErrorModel ChatDoesntExist
        {
            get
            {
                return new ErrorModel { ErrorMessage = "Такого чата не существует!" };
            }
        }
        public static ErrorModel LoginOrRegistration
        {
            get
            {
                return new ErrorModel { ErrorMessage = "Войдите или зарегистрируйтесь." };
            }
        }
    }
}