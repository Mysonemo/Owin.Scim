﻿namespace Owin.Scim.Repository.InMemory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    using Model.Users;

    using NContext.Security.Cryptography;

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly IList<User> _Users;

        public InMemoryUserRepository()
        {
            _Users = new List<User>();
        }

        public async Task<User> CreateUser(User user)
        {
            user.Id = Guid.NewGuid().ToString();

            _Users.Add(user);

            return user;
        }

        public async Task<User> GetUser(string userId)
        {
            return _Users.SingleOrDefault(u => u.Id.Equals(userId));
        }

        public async Task UpdateUser(User user)
        {
            var userRecord = _Users.SingleOrDefault(u => u.Id.Equals(user.Id));
            if (userRecord == null) throw new Exception();

            userRecord = user;
        }

        public async Task<Unit> DeleteUser(string userId)
        {
            var userRecord = _Users.SingleOrDefault(u => u.Id.Equals(userId));
            if (userRecord == null) throw new Exception();

            _Users.Remove(userRecord);

            return default(Unit);
        }

        public async Task<bool> IsUserNameAvailable(string userName)
        {
            /* Before comparing or evaluating the uniqueness of a "userName" or 
               "password" attribute, service providers MUST use the preparation, 
               enforcement, and comparison of internationalized strings (PRECIS) 
               preparation and comparison rules described in Sections 3 and 4, 
               respectively, of [RFC7613], which is based on the PRECIS framework
               specification [RFC7564]. */

            return _Users.All(u => !CryptographyUtility.CompareBytes(Encoding.UTF8.GetBytes(u.UserName), Encoding.UTF8.GetBytes(userName)));
        }
    }
}