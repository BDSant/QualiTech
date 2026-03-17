using AutoMapper;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.DTOs.Unidade;
using OsLog.Domain.Entities;

namespace OsLog.Application.Mapping;

public class EmpresaProfile : Profile
{
    public EmpresaProfile()
    {
        // Empresa -> Listagem
        CreateMap<Empresa, EmpresaListDto>()
            .ForMember(d => d.Ativa, opt => opt.MapFrom(s => !s.Ativa));

        // Empresa -> Detalhe
        CreateMap<Empresa, EmpresaDetailDto>()
            .ForMember(d => d.Ativa, opt => opt.MapFrom(s => !s.Ativa));

        // Create -> Empresa
        CreateMap<EmpresaCreateDto, Empresa>()
            // Campos controlados pelo domínio/banco/serviço
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Ativa, opt => opt.Ignore())
            .ForMember(d => d.DataCriacaoUtc, opt => opt.Ignore())
            .ForMember(d => d.Unidades, opt => opt.Ignore())
            .ForMember(d => d.UsuariosAcesso, opt => opt.Ignore());

        // Unidade -> DTO
        CreateMap<Unidade, UnidadeDto>()
            .ForMember(d => d.Ativa, opt => opt.MapFrom(s => !s.Ativa));

        // Create -> Unidade
        CreateMap<UnidadeCreateDto, Unidade>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.EmpresaId, opt => opt.Ignore()) // setado na Service
            .ForMember(d => d.Empresa, opt => opt.Ignore())
            .ForMember(d => d.Ativa, opt => opt.Ignore())
            .ForMember(d => d.DataCriacaoUtc, opt => opt.Ignore())
            .ForMember(d => d.UsuariosAcesso, opt => opt.Ignore());
    }
}
