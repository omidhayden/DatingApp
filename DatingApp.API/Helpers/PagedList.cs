using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers
{
    //API Pagination proccess
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }


        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;

            //Number of pages = total of users(itmes) / how many item per page
            TotalPages = (int)Math.Ceiling(count / (double) PageSize);
            this.AddRange(items);
        }


        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            //Exp you have 13 users, you want to get first 5 users. It means you request the page 1 , 
            // the first line put in page number 1, equel 0 * 0. and will take the first 5 users for you.
            var items = await source.Skip((pageNumber - 1)* pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items,count, pageNumber, pageSize);
        }
    }
}