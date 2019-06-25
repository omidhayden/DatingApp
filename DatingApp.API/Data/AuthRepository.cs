using System;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private DataContext Context { get; set; }

        public AuthRepository(DataContext context)
        {
           Context = context;
        }

        public async Task<User> Login(string username, string password)
        {
            User user = await Context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x => x.UserName == username);
            return user ?? null;

            // if(!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            // {
            //     return null;
            // }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
             using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
             {
                 byte[] computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                 if (computedHash.Where((t, i) => t != passwordHash[i]).Any())
                     return false;
             }

             return true;
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);
            // user.PasswordHash = passwordHash;
            // user.PasswordSalt = passwordSalt;

            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();
            return user;
            
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            return await Context.Users.AnyAsync(x => x.UserName == username);
        }
    }
}