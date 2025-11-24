using AutoMapper;
using OsLog.Application.DTOs.OrdemServico;
using OsLog.Domain.Entities;

namespace OsLog.Application.Mapping;

public class OrdemServicoProfile : Profile
{
    public OrdemServicoProfile()
    {
        // =========================
        // Orçamento / Itens
        // =========================
        CreateMap<OrcamentoItem, OrcamentoItemDto>().ReverseMap();

        // =========================
        // Pagamentos
        // =========================
        CreateMap<PagamentoOs, PagamentoOsDto>().ReverseMap();

        // =========================
        // Status / Históricos
        // =========================
        CreateMap<StatusHistorico, StatusHistoricoDto>();

        // =========================
        // Acessórios
        // =========================
        CreateMap<OrdemServicoAcessorio, OrdemServicoAcessorioDto>().ReverseMap();

        // =========================
        // Fotos
        // =========================
        CreateMap<OrdemServicoFoto, OrdemServicoFotoDto>().ReverseMap();

        // =========================
        // Comissões
        // =========================
        CreateMap<OrdemServicoComissao, OrdemServicoComissaoDto>().ReverseMap();

        // =========================
        // Ordem de Serviço – listagem
        // =========================
        CreateMap<OrdemServico, OrdemServicoListDto>()
            .ForMember(d => d.ClienteNome, opt => opt.MapFrom(s => s.Cliente.Nome))
            .ForMember(d => d.TecnicoNome, opt => opt.MapFrom(s => s.Tecnico != null ? s.Tecnico.Nome : null));

        // =========================
        // Ordem de Serviço – detalhe
        // =========================
        CreateMap<OrdemServico, OrdemServicoDetailDto>()
            .ForMember(d => d.ClienteNome, opt => opt.MapFrom(s => s.Cliente.Nome))
            .ForMember(d => d.TecnicoNome, opt => opt.MapFrom(s => s.Tecnico != null ? s.Tecnico.Nome : null))
            .ForMember(d => d.Itens, opt => opt.MapFrom(s => s.Itens))
            .ForMember(d => d.Pagamentos, opt => opt.MapFrom(s => s.Pagamentos))
            .ForMember(d => d.Historicos, opt => opt.MapFrom(s => s.Historicos))
            .ForMember(d => d.Acessorios, opt => opt.MapFrom(s => s.Acessorios))
            .ForMember(d => d.Fotos, opt => opt.MapFrom(s => s.Fotos))
            .ForMember(d => d.Comissoes, opt => opt.MapFrom(s => s.Comissoes));

        // =========================
        // Ordem de Serviço – create
        // =========================
        CreateMap<OrdemServicoCreateDto, OrdemServico>(MemberList.Source);
    }
}

