using System;
using System.Linq;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(
                    src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(
                    src => src.DateOfBirth.CalculateAge()
                ));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();

            CreateMap<Message, MessageDto>()
                .ForMember(dto => dto.SenderPhotoUrl, opt => opt.MapFrom(
                    mess => mess.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(dto => dto.RecipientPhotoUrl, opt => opt.MapFrom(
                    mess => mess.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url)); 
        }
    }
}