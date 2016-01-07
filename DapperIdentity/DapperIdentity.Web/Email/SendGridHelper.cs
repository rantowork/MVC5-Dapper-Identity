using System;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DapperIdentity.Core.Entities;
using Newtonsoft.Json;
using SendGrid;

namespace DapperIdentity.Web.Email
{
    public class SendGridHelper
    {
        public static string EncodeConfirmationToken(string confirmationToken, string email)
        {
            var token = new ConfirmationToken
            {
                Token = confirmationToken,
                Email = email
            };

            var jsonString = JsonConvert.SerializeObject(token);
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            var urlString = HttpServerUtility.UrlTokenEncode(bytes);
            return urlString;
        }

        public static ConfirmationToken DecodeConfirmationToken(string token)
        {
            var bytes = HttpServerUtility.UrlTokenDecode(token);
            var jsonString = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<ConfirmationToken>(jsonString);
        }

        public static async Task SendRegistrationEmail(string token, string email)
        {
            var message = new SendGridMessage
            {
                From = new MailAddress("you@youremail.com", "The Software Team")
            };

            var confirmationToken = EncodeConfirmationToken(token, email);

            message.AddTo(email);
            message.Subject = "Activate your account";

            var confirmationLink = String.Format("https://localhost/Account/ConfirmationLink?id={0}", confirmationToken);

            message.Html =
                "Hello!<br/><br/>Your Account has been created.  Please verify your email address by clicking this link to complete the signup process.<br/><br/>" +
                confirmationLink + "<br/><br/>If you did not initiate this request, you may safely ignore this one-time message.  The request will expire shortly.<br/><br/>Sincerely,<br/><br/>The Team";

            //TODO: Enter your SendGrid APIKEY
            var transportWeb = new SendGrid.Web("");
            await transportWeb.DeliverAsync(message);
        }
    }
}