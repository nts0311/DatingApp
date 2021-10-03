using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class PhotoRepository : IPhotoRespository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public PhotoRepository(DataContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<Photo> GetPhotoById(int id)
        {
            return await context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(p=>p.Id==id);
        }

        public async Task<List<PhotoApprovalDto>> GetUnapprovedPhotos()
        {
            return await context.Photos
                .Where(p => !p.IsApproved)
                .IgnoreQueryFilters()
                .ProjectTo<PhotoApprovalDto>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public void RemovePhoto(Photo photo)
        {
            context.Photos.Remove(photo);
        }
    }
}