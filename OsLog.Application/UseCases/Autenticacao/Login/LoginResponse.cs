using System;
using System.Collections.Generic;
using System.Text;

namespace OsLog.Application.UseCases.Autenticacao.Login;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    int UsuarioId,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyDictionary<string, string> Claims
);
