using OsLog.Application.Common.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace OsLog.Application.UseCases.Autenticacao.Login;

public interface IAutenticacaoUseCase
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
}