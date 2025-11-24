using AutoMapper;
using OsLog.Application.DTOs.Tecnicos;
using OsLog.Domain.Entities;

namespace OsLog.Application.Mapping;

public class TecnicoProfile : Profile
{
    public TecnicoProfile()
    {
        // Técnico -> DTO / CreateDto -> Técnico
        CreateMap<Tecnico, TecnicoDto>().ReverseMap();
        //CreateMap<TecnicoCreateDto, Tecnico>();
        CreateMap<TecnicoCreateDto, Tecnico>(MemberList.Source);
    }
}
