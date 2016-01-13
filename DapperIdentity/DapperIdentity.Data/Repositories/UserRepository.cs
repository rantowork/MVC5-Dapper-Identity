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
        /// <summary>
        /// User Repository constructor passing injected connection factory to the Base Repository
        /// </summary>
        /// <param name="connectionFactory">The injected connection factory.  It is injected with the constructor argument that is the connection string.</param>
        public UserRepository(IConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        /// <summary>
        /// INSERT operation for a new user.
        /// </summary>
        /// <param name="user">The User object must be passed in.  We create this during the Register Action.</param>
        /// <returns>Returns a 0 or 1 depending on whether operation is successful or not.</returns>
        public async Task CreateAsync(User user)
        {
            await WithConnection(async connection =>
            {
                string query = "INSERT INTO Users(Id,UserName,Nickname,PasswordHash,SecurityStamp,IsConfirmed,ConfirmationToken,CreatedDate) VALUES(@Id,@UserName,@Nickname,@PasswordHash,@SecurityStamp,@IsConfirmed,@ConfirmationToken,@CreatedDate)";
                user.Id = Guid.NewGuid().ToString();
                return await connection.ExecuteAsync(query, user);
            });
        }

        /// <summary>
        /// DELETE operation for a user.  This is not currently used, but required by .NET Identity.
        /// </summary>
        /// <param name="user">The User object</param>
        /// <returns>Returns a 0 or 1 depending on whether operation is successful or not.</returns>
        public async Task DeleteAsync(User user)
        {
            await WithConnection(async connection =>
            {
                string query = "DELETE FROM Users WHERE Id=@Id";
                return await connection.ExecuteAsync(query, new { @Id = user.Id });
            });
        }

        /// <summary>
        /// SELECT operation for finding a user by the Id value.  Our Id is currently a GUID but this can be another data type as well.
        /// </summary>
        /// <param name="userId">The Id of the user object.</param>
        /// <returns>Returns the User object for the supplied Id or null.</returns>
        public async Task<User> FindByIdAsync(string userId)
        {
            return await WithConnection(async connection =>
            {
                string query = "SELECT * FROM Users WHERE Id=@Id";
                var user = await connection.QueryAsync<User>(query, new { @Id = userId });
                return user.SingleOrDefault();
            });
        }

        /// <summary>
        /// SELECT operation for finding a user by the username.
        /// </summary>
        /// <param name="userName">The username of the user object.</param>
        /// <returns>Returns the User object for the supplied username or null.</returns>
        public async Task<User> FindByNameAsync(string userName)
        {
            return await WithConnection(async connection =>
            {
                string query = "SELECT * FROM Users WHERE LOWER(UserName)=LOWER(@UserName)";
                var user = await connection.QueryAsync<User>(query, new { @UserName = userName });
                return user.SingleOrDefault();
            });
        }

        /// <summary>
        /// UPDATE operation for updating a user.
        /// </summary>
        /// <param name="user">The user that will be updated.  The updated values must be passed in to this method.</param>
        /// <returns>Returns a 0 or 1 depending on whether operation is successful or not.</returns>
        public async Task UpdateAsync(User user)
        {
            await WithConnection(async connection =>
            {
                string query =
                    "UPDATE Users SET UserName=@UserName,Nickname=@Nickname,PasswordHash=@PasswordHash,SecurityStamp=@SecurityStamp,IsConfirmed=@IsConfirmed,CreatedDate=@CreatedDate,ConfirmationToken=@ConfirmationToken WHERE Id=@Id";
                return await connection.ExecuteAsync(query, user);
            });
        }

        /// <summary>
        /// INSERT operation for adding an external login such as Google for a new or existing account.
        /// </summary>
        /// <param name="user">The User object that will be associated with the external login information.</param>
        /// <param name="login">The user login information.  This object is constructed during the callback from the external authority.</param>
        /// <returns>Returns a 0 or 1 depending on whether operation is successful or not.</returns>
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

        /// <summary>
        /// DELETE operation for removing an external login from an existing user account.
        /// </summary>
        /// <param name="user">The user object that the external login will be removed from.</param>
        /// <param name="login">The external login that will be removed from the user account.</param>
        /// <returns>Returns a 0 or 1 depending on whether operation is successful or not.</returns>
        public async Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            await WithConnection(async connection =>
            {
                string query = "DELETE FROM ExternalLogins WHERE Id = @Id AND LoginProvider = @loginProvider AND ProviderKey = @providerKey";
                return await connection.ExecuteAsync(query, new { user.Id, login.LoginProvider, login.ProviderKey });
            });
        }

        /// <summary>
        /// SELECT operation for getting external logins for a user account.
        /// </summary>
        /// <param name="user">The user account to get external login information for.</param>
        /// <returns>List of UserLoginInfo objects that contain external login information for each associated external account.</returns>
        public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return await WithConnection(async connection =>
            {
                string query = "SELECT LoginProvider, ProviderKey FROM ExternalLogins WHERE UserId = @Id";
                var loginInfo = await connection.QueryAsync<UserLoginInfo>(query, user);
                return loginInfo.ToList();
            });
        }

        /// <summary>
        /// SELECT operation for getting the user object associated with a specific external login
        /// </summary>
        /// <param name="login">The external account</param>
        /// <returns>The User associated with the external account or null</returns>
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

        /// <summary>
        /// Method for setting the password hash for the user account.  This hash is used to encode the users password.
        /// </summary>
        /// <param name="user">The user to has the password for.</param>
        /// <param name="passwordHash">The password has to use.</param>
        /// <returns></returns>
        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Method for getting teh password hash for the user account.
        /// </summary>
        /// <param name="user">The user to get the password hash for.</param>
        /// <returns>The password hash.</returns>
        public Task<string> GetPasswordHashAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.PasswordHash);
        }

        /// <summary>
        /// Method for checking if an account has a password hash.
        /// </summary>
        /// <param name="user">The user to check for an existing password hash.</param>
        /// <returns>True of false depending on whether the password hash exists or not.</returns>
        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        /// <summary>
        /// Method for setting the security stamp for the user account.
        /// </summary>
        /// <param name="user">The user to set the security stamp for.</param>
        /// <param name="stamp">The stamp to set.</param>
        /// <returns></returns>
        public Task SetSecurityStampAsync(User user, string stamp)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Method for getting the security stamp for the user account.
        /// </summary>
        /// <param name="user">The user to get the security stamp for.</param>
        /// <returns>The security stamp.</returns>
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
