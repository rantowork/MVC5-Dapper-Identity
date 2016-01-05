using System;
using Microsoft.AspNet.Identity;

namespace DapperIdentity.Core.Entities
{
    /// <summary>
    /// Custom fields for your user object.  Id, UserName, PasswordHash and SecurityStamp are all required by Identity.
    /// TODO:  Add your own custom fields.  Don't forget to update your the database table and your SQL queries
    /// </summary>
    public class User : IUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsConfirmed { get; set; }
        public string ConfirmationToken { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
