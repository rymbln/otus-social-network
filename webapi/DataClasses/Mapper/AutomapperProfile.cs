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
            .ForMember(d => d.Id, opt => opt.MapFrom(v => v.Id))
            .ForMember(d => d.AuthorUserId, opt => opt.MapFrom(v => v.AuthorUserId))
            .ForMember(d => d.AuthorUserName, opt => opt.MapFrom(v => ""))
            .ForMember(d => d.Text, opt => opt.MapFrom(v => v.Text))
            .ForMember(d => d.TimeStamp, opt => opt.MapFrom(v => v.TimeStamp))
            ;
        CreateMap<PostView, PostDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(v => v.PostId))
            .ForMember(d => d.AuthorUserId, opt => opt.MapFrom(v => v.FriendId))
            .ForMember(d => d.AuthorUserName, opt => opt.MapFrom(v => v.FriendName))
            .ForMember(d => d.Text, opt => opt.MapFrom(v => v.PostText))
            .ForMember(d => d.TimeStamp, opt => opt.MapFrom(v => v.Timestamp))
            ;
        CreateMap<FriendView, FriendDto>();
    }
}
