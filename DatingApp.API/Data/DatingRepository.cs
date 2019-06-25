using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private DataContext Context { get; set; }

        public DatingRepository(DataContext context)
        {
            Context = context;
        }


        public void Add<T>(T entity) where T : class
        {
            Context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
           Context.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
            User user = await Context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            IQueryable<User> users =  Context.Users.Include( p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);
            if(userParams.Likers)
            {
                IEnumerable<int> userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            if(userParams.Likees)
            {
                 IEnumerable<int> userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }
            if(userParams.MinAge != 18 || userParams.MaxAge !=99)
            {
                DateTime minDob= DateTime.Today.AddYears(-userParams.MaxAge - 1);
                DateTime maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if (string.IsNullOrEmpty(userParams.OrderBy))
                return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
            {
                switch (userParams.OrderBy)
                {
                    case "created" : users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            Photo photo = await Context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            User user= await Context.Users
            .Include(x => x.Likers)
            .Include(x => x.Likees)
            .FirstOrDefaultAsync(u => u.Id == id);

            return likers ? user.Likers.Where( u => u.LikeeId == id).Select(i => i.LikerId) : user.Likees.Where( u => u.LikerId == id).Select(i => i.LikeeId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await Context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.isMain);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await Context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await Context.Messages.FirstOrDefaultAsync(m => m.Id ==id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
           IQueryable<Message> messages = Context.Messages
           .Include(u => u.Sender).ThenInclude( p => p.Photos)
           .Include(u => u.Recipient).ThenInclude(p => p.Photos)
           .AsQueryable();


           switch( messageParams.MessageContainer)
           {
               case "Inbox":
               messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.RecipientDeleted==false);
               break;
               case "Outbox":
               messages = messages.Where( u=> u.SenderId == messageParams.UserId && u.SenderDeleted == false);
               break;
               default:
               messages = messages.Where( u=> u.RecipientId ==messageParams.UserId && u.RecipientDeleted == false && u.IsRead == false);
               break;
           }
           messages = messages.OrderByDescending(d => d.MessageSent);
           return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber,messageParams.PageSize);

        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
           List<Message> messages = await Context.Messages
           .Include(u => u.Sender).ThenInclude(p => p.Photos)
           .Include(u => u.Recipient).ThenInclude(p => p.Photos)
           .Where(m => m.RecipientId == userId && m.RecipientDeleted ==false && m.SenderId == recipientId
           || m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted ==false)
           .OrderByDescending(m => m.MessageSent)
           .ToListAsync();

           return messages;

        }
    }
}