using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.OrdemServico;
using OsLog.Domain.Entities;

namespace OsLog.Application.Services;

public class OrdemServicoService
{
    private readonly IUnitOfWork _UnitOfWork;
    private readonly IMapper _mapper;

    public OrdemServicoService(IUnitOfWork uow, IMapper mapper)
    {
        _UnitOfWork = uow;
        _mapper = mapper;
    }

    public async Task<int> AbrirOsAsync(
        OrdemServicoCreateDto dto,
        int usuarioId,
        CancellationToken ct)
    {
        // TODO: validar se Empresa/Unidade existem via _uow.Empresas/_uow.Unidades

        var os = new OrdemServico
        {
            EmpresaId = dto.EmpresaId,
            UnidadeId = dto.UnidadeId,
            ClienteId = dto.ClienteId,
            TecnicoId = dto.TecnicoId,
            DescricaoProblema = dto.DescricaoProblema,
            SinalObrigatorio = dto.SinalObrigatorio,
            ValorSinal = dto.ValorSinal,
            StatusOs = "PENDENTE_ANALISE",
            StatusOrcamento = "PENDENTE",
            DataCriacao = DateTime.UtcNow,
            AlteradoPor = usuarioId,
            FlExcluido = false
        };

        await _UnitOfWork.OrdensServico.AddAsync(os, ct);

        return os.Id;
    }



    public async Task<List<OrdemServicoListDto>> ListarAsync(CancellationToken ct)
    {
        var lista = await _UnitOfWork.OrdensServico.ListAsync(ct);
        return lista
            .Where(o => !o.FlExcluido)
            .Select(_mapper.Map<OrdemServicoListDto>)
            .ToList();
    }

    public async Task<OrdemServicoDetailDto?> ObterPorIdAsync(int id, CancellationToken ct)
    {
        var os = await _UnitOfWork.OrdensServico.GetDetalhadaAsync(id, ct);

        if (os is null || os.FlExcluido)
            return null;

        return _mapper.Map<OrdemServicoDetailDto>(os);
    }

    public async Task AtualizarStatusAsync(int idOs, string novoStatus, int usuarioId, CancellationToken ct)
    {
        var os = await _UnitOfWork.OrdensServico.GetByIdAsync(idOs, ct)
                 ?? throw new InvalidOperationException($"Ordem de serviço {idOs} não encontrada.");

        var anterior = os.StatusOs;

        os.StatusOs = novoStatus;
        os.DataAlteracao = DateTime.UtcNow;
        os.AlteradoPor = usuarioId;

        _UnitOfWork.OrdensServico.Update(os);

        var hist = new StatusHistorico
        {
            OrdemServicoId = idOs,
            TipoEvento = "ALTERACAO_STATUS",
            StatusOsAnterior = anterior,
            StatusOsNovo = novoStatus,
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

    public async Task ConfirmarPagamentoSinalAsync(int idOs, int usuarioId, CancellationToken ct)
    {
        var os = await _UnitOfWork.OrdensServico.GetByIdAsync(idOs, ct)
                 ?? throw new InvalidOperationException($"Ordem de serviço {idOs} não encontrada.");

        if (!os.SinalObrigatorio || os.SinalPago)
            return;

        os.SinalPago = true;
        os.DataAlteracao = DateTime.UtcNow;
        os.AlteradoPor = usuarioId;

        _UnitOfWork.OrdensServico.Update(os);

        var hist = new StatusHistorico
        {
            OrdemServicoId = idOs,
            TipoEvento = "CONFIRMACAO_SINAL",
            DescricaoEvento = "Sinal confirmado.",
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

    public async Task SoftDeleteAsync(int idOs, int usuarioId, CancellationToken ct)
    {
        var os = await _UnitOfWork.OrdensServico.GetByIdAsync(idOs, ct)
                 ?? throw new InvalidOperationException($"Ordem de serviço {idOs} não encontrada.");

        if (os.FlExcluido)
            return;

        os.FlExcluido = true;
        os.DataAlteracao = DateTime.UtcNow;
        os.AlteradoPor = usuarioId;

        _UnitOfWork.OrdensServico.Update(os);

        var hist = new StatusHistorico
        {
            OrdemServicoId = idOs,
            TipoEvento = "EXCLUSAO_LOGICA",
            DescricaoEvento = "OS marcada como excluída.",
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

    public async Task<List<OrcamentoItemDto>> ListarItensAsync(int osId, CancellationToken ct)
    {
        var itens = await _UnitOfWork.OrcamentoItens
            .ListAsync(i => i.OrdemServicoId == osId, ct);

        return itens.Select(_mapper.Map<OrcamentoItemDto>).ToList();
    }

    public async Task AdicionarItemAsync(int osId, string tipoItem, string descricao, decimal quantidade, decimal valorUnitario, int usuarioId, CancellationToken ct)
    {
        var os = await _UnitOfWork.OrdensServico.GetByIdAsync(osId, ct)
                 ?? throw new InvalidOperationException($"Ordem de serviço {osId} não encontrada.");

        var item = new OrcamentoItem
        {
            OrdemServicoId = osId,
            TipoItem = tipoItem,
            DescricaoItem = descricao,
            Quantidade = quantidade,
            ValorUnitario = valorUnitario,
            DataCriacao = DateTime.UtcNow,
            AlteradoPor = usuarioId,
            FlExcluido = false
        };

        await _UnitOfWork.OrcamentoItens.AddAsync(item, ct);

        // Atualiza valor total de orçamento (se existir campo na entidade OS)
        var valorItem = quantidade * valorUnitario;
        os.ValorOrcamentoTotal = (os.ValorOrcamentoTotal ?? 0m) + valorItem;
        os.DataAlteracao = DateTime.UtcNow;
        os.AlteradoPor = usuarioId;

        _UnitOfWork.OrdensServico.Update(os);

        var hist = new StatusHistorico
        {
            OrdemServicoId = osId,
            TipoEvento = "INCLUSAO_ITEM_ORCAMENTO",
            DescricaoEvento = $"Item incluído: {descricao} (R$ {valorItem:0.00})",
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

    public async Task RemoverItemAsync(int osId, int itemId, int usuarioId, CancellationToken ct)
    {
        var os = await _UnitOfWork.OrdensServico.GetByIdAsync(osId, ct)
                 ?? throw new InvalidOperationException($"Ordem de serviço {osId} não encontrada.");

        var item = await _UnitOfWork.OrcamentoItens.GetByIdAsync(itemId, ct)
                   ?? throw new InvalidOperationException($"Item {itemId} não encontrado.");

        if (item.OrdemServicoId != osId)
            throw new InvalidOperationException("Item não pertence à OS informada.");

        var valorItem = item.Quantidade * item.ValorUnitario;

        _UnitOfWork.OrcamentoItens.Remove(item);

        if (os.ValorOrcamentoTotal.HasValue)
        {
            os.ValorOrcamentoTotal -= valorItem;
            if (os.ValorOrcamentoTotal < 0) os.ValorOrcamentoTotal = 0;
        }

        os.DataAlteracao = DateTime.UtcNow;
        os.AlteradoPor = usuarioId;

        _UnitOfWork.OrdensServico.Update(os);

        var hist = new StatusHistorico
        {
            OrdemServicoId = osId,
            TipoEvento = "REMOCAO_ITEM_ORCAMENTO",
            DescricaoEvento = $"Item removido: {item.DescricaoItem} (R$ {valorItem:0.00})",
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

    public async Task<List<OrdemServicoAcessorioDto>> ListarAcessoriosAsync(int osId, CancellationToken ct)
    {
        var acessorios = await _UnitOfWork.Acessorios
            .ListAsync(a => a.OrdemServicoId == osId && !a.FlExcluido, ct);

        return acessorios.Select(_mapper.Map<OrdemServicoAcessorioDto>).ToList();
    }

    public async Task AdicionarAcessorioAsync(int osId, string descricao, int usuarioId, CancellationToken ct)
    {
        var os = await _UnitOfWork.OrdensServico.GetByIdAsync(osId, ct)
                 ?? throw new InvalidOperationException($"Ordem de serviço {osId} não encontrada.");

        var acessorio = new OrdemServicoAcessorio
        {
            OrdemServicoId = osId,
            Descricao = descricao,
            Devolvido = false,
            FlExcluido = false,
            DataCriacao = DateTime.UtcNow,
            AlteradoPor = usuarioId
        };

        await _UnitOfWork.Acessorios.AddAsync(acessorio, ct);

        var hist = new StatusHistorico
        {
            OrdemServicoId = osId,
            TipoEvento = "INCLUSAO_ACESSORIO",
            DescricaoEvento = $"Acessório incluído: {descricao}",
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

    public async Task AtualizarDevolucaoAcessorioAsync(int osId, int acessorioId, bool devolvido, int usuarioId, CancellationToken ct)
    {
        var acessorio = await _UnitOfWork.Acessorios.GetByIdAsync(acessorioId, ct)
                        ?? throw new InvalidOperationException($"Acessório {acessorioId} não encontrado.");

        if (acessorio.OrdemServicoId != osId)
            throw new InvalidOperationException("Acessório não pertence à OS informada.");

        acessorio.Devolvido = devolvido;
        acessorio.DataAlteracao = DateTime.UtcNow;
        acessorio.AlteradoPor = usuarioId;

        _UnitOfWork.Acessorios.Update(acessorio);

        var hist = new StatusHistorico
        {
            OrdemServicoId = osId,
            TipoEvento = "ATUALIZACAO_DEVOLUCAO_ACESSORIO",
            DescricaoEvento = $"Acessório '{acessorio.Descricao}' marcado como {(devolvido ? "devolvido" : "não devolvido")}.",
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

    public async Task RemoverAcessorioAsync(int osId, int acessorioId, int usuarioId, CancellationToken ct)
    {
        var acessorio = await _UnitOfWork.Acessorios.GetByIdAsync(acessorioId, ct)
                        ?? throw new InvalidOperationException($"Acessório {acessorioId} não encontrado.");

        if (acessorio.OrdemServicoId != osId)
            throw new InvalidOperationException("Acessório não pertence à OS informada.");

        if (acessorio.FlExcluido)
            return;

        acessorio.FlExcluido = true;
        acessorio.DataAlteracao = DateTime.UtcNow;
        acessorio.AlteradoPor = usuarioId;

        _UnitOfWork.Acessorios.Update(acessorio);

        var hist = new StatusHistorico
        {
            OrdemServicoId = osId,
            TipoEvento = "EXCLUSAO_LOGICA_ACESSORIO",
            DescricaoEvento = $"Acessório excluído: {acessorio.Descricao}",
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

    public async Task<List<OrdemServicoFotoDto>> ListarFotosAsync(int osId, CancellationToken ct)
    {
        var fotos = await _UnitOfWork.Fotos
            .ListAsync(f => f.OrdemServicoId == osId && !f.FlExcluido, ct);

        return fotos.Select(_mapper.Map<OrdemServicoFotoDto>).ToList();
    }

    public async Task AdicionarFotoAsync(int osId, byte[] foto, string? descricao, int usuarioId, CancellationToken ct)
    {
        var os = await _UnitOfWork.OrdensServico.GetByIdAsync(osId, ct)
                 ?? throw new InvalidOperationException($"Ordem de serviço {osId} não encontrada.");

        var objfoto = new OrdemServicoFoto
        {
            OrdemServicoId = osId,
            Foto = foto,
            Descricao = descricao,
            DataCadastro = DateTime.UtcNow,
            FlExcluido = false,
            AlteradoPor = usuarioId
        };

        await _UnitOfWork.Fotos.AddAsync(objfoto, ct);

        var hist = new StatusHistorico
        {
            OrdemServicoId = osId,
            TipoEvento = "INCLUSAO_FOTO",
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

    public async Task RemoverFotoAsync(int osId, int fotoId, int usuarioId, CancellationToken ct)
    {
        var foto = await _UnitOfWork.Fotos.GetByIdAsync(fotoId, ct)
                   ?? throw new InvalidOperationException($"Foto {fotoId} não encontrada.");

        if (foto is null || foto.OrdemServicoId != osId)
            throw new InvalidOperationException("Foto não pertence à OS informada.");

        if (foto.FlExcluido)
            return;

        foto.FlExcluido = true;
        foto.DataAlteracao = DateTime.UtcNow;
        foto.AlteradoPor = usuarioId;

        _UnitOfWork.Fotos.Update(foto);

        var hist = new StatusHistorico
        {
            OrdemServicoId = osId,
            TipoEvento = "EXCLUSAO_LOGICA_FOTO",
            DataEvento = DateTime.UtcNow,
            UsuarioId = usuarioId
        };

        await _UnitOfWork.StatusHistoricos.AddAsync(hist, ct);
        // Commit no controller
    }

}

