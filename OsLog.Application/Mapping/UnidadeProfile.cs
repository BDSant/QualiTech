using AutoMapper;
using OsLog.Application.DTOs.Unidade;
using OsLog.Domain.Entities;

namespace OsLog.Application.Mapping;

public class UnidadeProfile : Profile
{
    public UnidadeProfile()
    {
        // Unidade -> DTO
        CreateMap<Unidade, UnidadeDto>()
            .ForMember(d => d.Ativa, opt => opt.MapFrom(s => s.Ativa));

        CreateMap<UnidadeCreateDto, Unidade>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.EmpresaId, opt => opt.Ignore())
            .ForMember(d => d.Tipo, opt => opt.Ignore())
            .ForMember(d => d.Ativa, opt => opt.Ignore())
            .ForMember(d => d.DataCriacaoUtc, opt => opt.Ignore())
            .ForMember(d => d.Empresa, opt => opt.Ignore())
            .ForMember(d => d.UsuariosAcesso, opt => opt.Ignore());
    }
}