using DapperIdentity.Core.Entities;
using Microsoft.AspNet.Identity;

namespace DapperIdentity.Core.Interfaces
{
    public interface IUserRepository : IUserStore<User>, IUserLoginStore<User>, IUserPasswordStore<User>, IUserSecurityStampStore<User>
    {
    }
}
