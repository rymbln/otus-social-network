using AutoMapper;

using OtusSocialNetwork.Database.Entities;
using OtusSocialNetwork.DataClasses.Dtos;
using OtusSocialNetwork.DataClasses.Requests;

namespace OtusSocialNetwork.DataClasses.Mapper;

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<RegisterReq, UserEntity>()
            .ForMember(e => e.Id, opt => opt.MapFrom(r => Guid.NewGuid().ToString()))
            ;
        CreateMap<UserEntity, UserDto>();
        CreateMap<NewTableEntity, NewTableReq>().ReverseMap();
        CreateMap<PostEntity, PostDto>()
            .ForMember(d => d.Text, opt => opt.MapFrom(e => e.Text))
            .ForMember(d => d.AuthorUserId, opt => opt.MapFrom(e => e.AuthorUserId))
            ;

        CreateMap<FriendView, FriendDto>();
    }
}
