using System.Text;
using System.Web;
using DapperIdentity.Core.Entities;
using Newtonsoft.Json;

namespace DapperIdentity.Web.Email
{
    public class EmailConfirmationHelper
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

        //public static async Task SendRegistrationEmail(string token, string email)
        //{
        //    //TODO:  Implement logic to send e-mail for confirmation if that is something you want to do
        //}
    }
}