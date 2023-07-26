using AutoMapper;

using OtusClasses;
using OtusClasses.DataClasses.Dtos;

using static OtusClasses.DialogReply.Types;

namespace OtusDialogsGrpc.Mapper;

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<DialogMessageDTO, MessageReply>()
            .ForMember(r => r.Id, opt => opt.MapFrom(d => d.Id))
            .ForMember(r => r.From, opt => opt.MapFrom(d => d.From))
            .ForMember(r => r.To, opt => opt.MapFrom(d => d.To))
            .ForMember(r => r.Message, opt => opt.MapFrom(d => d.Text))
            .ForMember(r => r.Timestamp, opt => opt.MapFrom(d => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d.TimeStamp)))
            ;
        CreateMap<DialogMessageDTO, DialogMessage>()
            .ForMember(r => r.From, opt => opt.MapFrom(d => d.From))
            .ForMember(r => r.To, opt => opt.MapFrom(d => d.To))
            .ForMember(r => r.Message, opt => opt.MapFrom(d => d.Text))
            .ForMember(r => r.Timestamp, opt => opt.MapFrom(d => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d.TimeStamp)))
            ;

    }
}
