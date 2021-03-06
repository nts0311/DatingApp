using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await context.Users
            .Include(p => p.Photos)
            .ToListAsync();
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await context.Users.FindAsync(id);
        }
        public void Update(AppUser user)
        {
            context.Entry(user).State = EntityState.Modified;
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            var query = context.Users
                .AsQueryable()
                .Where(u => u.UserName != userParams.CurrentUsername)
                .Where(u => u.Gender == userParams.Gender)
                .Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };
                
            return await PagedList<MemberDto>.CreateAsync(
                query.ProjectTo<MemberDto>(mapper.ConfigurationProvider).AsNoTracking(), 
                userParams.PageNumber, userParams.PageSize);            
        }

        public async Task<MemberDto> GetMemberAsync(string username, bool includeUnapprovedPhoto)
        {
            var query = context.Users.Where(user => user.UserName == username);
            if(includeUnapprovedPhoto) query = query.IgnoreQueryFilters();

            return await query.ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<string> GetUserGender(string username)
        {
            return await context.Users.Where(user => user.UserName == username)
                .Select(user => user.Gender).FirstOrDefaultAsync();
        }

        public async Task<bool> NeedMainPhoto(int photoId)
        {
            return await context.Users
                .Include(u => u.Photos)
                .IgnoreQueryFilters()
                .Where(u => u.Photos.Any(p => p.Id == photoId))
                .Take(1)
                .Select(p => p.Photos)
                .AllAsync(p => p.All(x => !x.IsApproved));
        }

        // public async Task<AppUser> GetUserByPhoto(int photoId)
        // {
        //     return await context.Users
        //         .Include(u => u.Photos)
        //         .IgnoreQueryFilters()
        //         .Take(1)
        //         .Select(p => p.Photos)
        //         .AllAsync(p => p.All(x => !x.IsApproved));
        // }
    }
}