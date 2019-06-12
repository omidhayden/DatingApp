using System.Linq;
using AutoMapper;
using DatingApp.API.Models;
using DatingApp.API.ViewModels;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListViewModel>()
            .ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault( p => p.isMain).Url);

            })
            .ForMember(dest => dest.Age, opt => {
                opt.ResolveUsing(d => d.DateOfBirth.CalculateAge());
            });
            CreateMap<User, UserForDetailedViewModel>()
              .ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault( p => p.isMain).Url);
            })
            .ForMember(dest => dest.Age, opt => {
             opt.ResolveUsing(d => d.DateOfBirth.CalculateAge());
            });
            CreateMap<Photo, PhotosForDetailedViewModel>();

            CreateMap<UserForUpdateViewModel, User>();

            CreateMap<Photo, PhotoForReturnViewModel>();
            
            CreateMap<PhotoForCreationViewModel, Photo>();
            CreateMap<UserForRegisterViewModel,User>();
            CreateMap<MessageForCreationViewModel, Message>().ReverseMap();
            CreateMap<Message, MessageToReturnViewModel>()
                .ForMember(m => m.SenderPhotoUrl, opt => opt
                    .MapFrom( u => u.Sender.Photos.FirstOrDefault(p => p.isMain).Url))
                .ForMember(m => m.RecipientPhotoUrl, opt => opt
                    .MapFrom( u => u.Recipient.Photos.FirstOrDefault(p => p.isMain).Url));

        }
    }
}