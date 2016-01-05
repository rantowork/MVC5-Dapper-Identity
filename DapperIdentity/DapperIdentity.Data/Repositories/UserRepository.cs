using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DapperIdentity.Core.Entities;
using DapperIdentity.Core.Interfaces;
using Microsoft.AspNet.Identity;

namespace DapperIdentity.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task CreateAsync(User user)
        {
            await WithConnection(async connection =>
            {
                string query = "INSERT INTO Users(Id,UserName,Nickname,PasswordHash,SecurityStamp,IsConfirmed,ConfirmationToken,CreatedDate) VALUES(@Id,@UserName,@Nickname,@PasswordHash,@SecurityStamp,@IsConfirmed,@ConfirmationToken,@CreatedDate)";
                user.Id = Guid.NewGuid().ToString();
                return await connection.ExecuteAsync(query, user);
            });
        }

        public async Task DeleteAsync(User user)
        {
            await WithConnection(async connection =>
            {
                string query = "DELETE FROM Users WHERE Id=@Id";
                return await connection.ExecuteAsync(query, new { @Id = user.Id });
            });
        }

        public async Task<User> FindByIdAsync(string userId)
        {
            return await WithConnection(async connection =>
            {
                string query = "SELECT * FROM Users WHERE Id=@Id";
                var user = await connection.QueryAsync<User>(query, new { @Id = userId });
                return user.SingleOrDefault();
            });
        }

        public async Task<User> FindByNameAsync(string userName)
        {
            return await WithConnection(async connection =>
            {
                string query = "SELECT * FROM Users WHERE LOWER(UserName)=LOWER(@UserName)";
                var user = await connection.QueryAsync<User>(query, new { @UserName = userName });
                return user.SingleOrDefault();
            });
        }

        public async Task UpdateAsync(User user)
        {
            await WithConnection(async connection =>
            {
                string query =
                    "UPDATE Users SET UserName=@UserName,Nickname=@Nickname,PasswordHash=@PasswordHash,SecurityStamp=@SecurityStamp,IsConfirmed=@IsConfirmed,CreatedDate=@CreatedDate,ConfirmationToken=@ConfirmationToken WHERE Id=@Id";
                return await connection.ExecuteAsync(query, user);
            });
        }

        public async Task AddLoginAsync(User user, UserLoginInfo login)
        {
            await WithConnection(async connection =>
            {
                string query =
                    "INSERT INTO ExternalLogins(ExternalLoginId, UserId, LoginProvider, ProviderKey) VALUES(@externalLoginId, @userId, @loginProvider, @providerKey)";
                return
                    await
                        connection.ExecuteAsync(query,
                            new
                            {
                                externalLoginId = Guid.NewGuid(),
                                userId = user.Id,
                                loginProvider = login.LoginProvider,
                                providerKey = login.ProviderKey
                            });
            });
        }

        public async Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            await WithConnection(async connection =>
            {
                string query = "DELETE FROM ExternalLogins WHERE Id = @Id AND LoginProvider = @loginProvider AND ProviderKey = @providerKey";
                return await connection.ExecuteAsync(query, new { user.Id, login.LoginProvider, login.ProviderKey });
            });
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return await WithConnection(async connection =>
            {
                string query = "SELECT LoginProvider, ProviderKey FROM ExternalLogins WHERE UserId = @Id";
                var loginInfo = await connection.QueryAsync<UserLoginInfo>(query, user);
                return loginInfo.ToList();
            });
        }

        public async Task<User> FindAsync(UserLoginInfo login)
        {
            await WithConnection(async connection =>
            {
                string query =
                    "SELECT u.* FROM Users u INNER JOIN ExternalLogins e ON e.UserId = u.Id WHERE e.LoginProvider = @loginProvider and e.ProviderKey = @providerKey";
                var account = await connection.QueryAsync<User>(query, login);
                return account.SingleOrDefault();
            });
            return null;
        }

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        public Task SetSecurityStampAsync(User user, string stamp)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.SecurityStamp);
        }

        public void Dispose()
        {
        }
    }
}
