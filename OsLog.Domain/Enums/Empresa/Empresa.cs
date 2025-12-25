using System;
using System.Collections.Generic;
using System.Text;

namespace OsLog.Domain.Enums.Empresa;

public enum PerfilAcessoEmpresa : byte
{
    Dono = 1,
    Financeiro = 2,
    GerenteUnidade = 3,
    Tecnico = 4,
    Atendente = 5,
    Cliente = 6
}