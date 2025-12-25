using AutoMapper;
using OsLog.Application.DTOs.Empresa;
using OsLog.Domain.Entities;

namespace OsLog.Application.Mapping;

public class UsuarioAcessoProfile : Profile
{
    public UsuarioAcessoProfile()
    {
        // Mapeia um vínculo UsuarioAcesso específico para um DTO de unidade
        CreateMap<UsuarioAcesso, UnidadeAcessoDto>()
            .ForMember(dest => dest.UnidadeId,
                opt => opt.MapFrom(src => src.UnidadeId!.Value))   // já filtramos null antes
            .ForMember(dest => dest.NomeUnidade,
                opt => opt.MapFrom(src => src.Unidade!.Nome));
    }
}
