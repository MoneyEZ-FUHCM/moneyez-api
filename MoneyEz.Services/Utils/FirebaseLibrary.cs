using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Utils
{
    public class FirebaseLibrary
    {
        public static async Task<string> SendMessageFireBase(string title, string body, string token)
        {
            try
            {
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body,
                        ImageUrl = "https://firebasestorage.googleapis.com/v0/b/exe201-9459a.appspot.com/o/EzMoney%2FEzMoney_v2%20(1).png?alt=media&token=ae09e2bf-928c-40d6-8da3-ec4458ef9187"
                    },
                    Token = token
                };

                var reponse = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                return reponse;
            }
            catch
            {
                return "";
            }
        }

        public static async Task<bool> SendRangeMessageFireBase(string title, string body, List<string> tokens)
        {
            var message = new MulticastMessage()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                    ImageUrl = "https://firebasestorage.googleapis.com/v0/b/exe201-9459a.appspot.com/o/EzMoney%2FEzMoney_v2%20(1).png?alt=media&token=ae09e2bf-928c-40d6-8da3-ec4458ef9187"
                },
                Tokens = tokens
            };

            var reponse = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            return true;

        }

        public static async Task<string> SendMessagePaymentFireBase(string title, string body, string token)
        {
            try
            {
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = body,
                        ImageUrl = "https://firebasestorage.googleapis.com/v0/b/exe201-9459a.appspot.com/o/EzMoney%2FEzMoney_v2%20(1).png?alt=media&token=ae09e2bf-928c-40d6-8da3-ec4458ef9187"
                    },
                    Token = token
                };

                var reponse = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                return reponse;
            }
            catch
            {
                return "";
            }
        }
    }
}
