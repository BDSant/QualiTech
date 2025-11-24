using OsLog.Application.Interfaces.Repositories;

namespace OsLog.Application.Services;

public class PagamentoService
{
    private readonly IPagamentoRepository _pagamentoRepo;

    public PagamentoService(IPagamentoRepository pagamentoRepo)
    {
        _pagamentoRepo = pagamentoRepo;
    }
}
