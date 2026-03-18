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
        CreateMap<Empresa, EmpresaListDto>();

        // Empresa -> Detalhe
        CreateMap<Empresa, EmpresaDetailDto>();

        // Create -> Empresa
        CreateMap<EmpresaCreateDto, Empresa>()
            // Campos controlados pelo domínio/banco/serviço
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Ativa, opt => opt.Ignore())
            .ForMember(d => d.DataCriacaoUtc, opt => opt.Ignore())
            .ForMember(d => d.Unidades, opt => opt.Ignore())
            .ForMember(d => d.UsuariosAcesso, opt => opt.Ignore());
    }
}
