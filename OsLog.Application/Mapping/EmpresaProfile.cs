using AutoMapper;
using OsLog.Application.DTOs.Empresa;
using OsLog.Domain.Entities;

namespace OsLog.Application.Mapping;

public class EmpresaProfile : Profile
{
    public EmpresaProfile()
    {
        // Empresa -> Listagem
        CreateMap<Empresa, EmpresaListDto>()
            .ForMember(d => d.Ativa, opt => opt.MapFrom(s => !s.FlExcluido));

        // Empresa -> Detalhe
        CreateMap<Empresa, EmpresaDetailDto>()
            .ForMember(d => d.Ativa, opt => opt.MapFrom(s => !s.FlExcluido));

        // Create -> Empresa
        CreateMap<EmpresaCreateDto, Empresa>()
            // Campos controlados pelo domínio/banco/serviço
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.FlExcluido, opt => opt.Ignore())
            .ForMember(d => d.DataCriacao, opt => opt.Ignore())
            .ForMember(d => d.DataAlteracao, opt => opt.Ignore())
            .ForMember(d => d.AlteradoPor, opt => opt.Ignore())
            .ForMember(d => d.Unidades, opt => opt.Ignore());

        // Unidade -> DTO
        CreateMap<Unidade, UnidadeDto>()
            .ForMember(d => d.Ativa, opt => opt.MapFrom(s => !s.FlExcluido));

        // Create -> Unidade
        CreateMap<UnidadeCreateDto, Unidade>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.EmpresaId, opt => opt.Ignore()) // setado na Service
            .ForMember(d => d.Empresa, opt => opt.Ignore())
            .ForMember(d => d.FlExcluido, opt => opt.Ignore())
            .ForMember(d => d.DataCriacao, opt => opt.Ignore())
            .ForMember(d => d.DataAlteracao, opt => opt.Ignore())
            .ForMember(d => d.AlteradoPor, opt => opt.Ignore())
            .ForMember(d => d.OrdensServico, opt => opt.Ignore());
    }
}
