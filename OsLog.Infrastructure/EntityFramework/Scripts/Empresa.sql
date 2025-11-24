--------------------------------------------------------------------------------
-- MASSA DE DADOS INICIAL PARA Empresa e Unidade
-- Ajuste o NOME DO BANCO se quiser usar USE [SeuBanco]
--------------------------------------------------------------------------------
USE [OsLog];
GO

--------------------------------------------------------------------------------
-- ATENÇÃO: se já houver OrdemServico apontando para essas empresas/unidades,
-- NÃO execute os DELETEs abaixo sem ter certeza.
--------------------------------------------------------------------------------
-- Limpa unidades e empresas (ordem correta por FK)
-- DELETE FROM [Empresa].[Unidade];
-- DELETE FROM [Empresa].[Empresa];
GO

DECLARE @AgoraUtc datetime2 = SYSUTCDATETIME();
DECLARE @UsuarioSistema int = 1; -- usuário "sistema" ou admin

--------------------------------------------------------------------------------
-- EMPRESA 1: ConsertaSmart ME
--------------------------------------------------------------------------------
INSERT INTO [Empresa].[Empresa]
    (RazaoSocial,           NomeFantasia,       Cnpj,              FlExcluido, DataCriacao,      DataAlteracao, AlteradoPor)
VALUES
    (N'ConsertaSmart ME',   N'ConsertaSmart',   N'12.345.678/0001-99', 0,      @AgoraUtc,        NULL,          @UsuarioSistema);

DECLARE @Empresa1Id int = SCOPE_IDENTITY();

-- Unidade Matriz (mesmo CNPJ da empresa)
INSERT INTO [Empresa].[Unidade]
    (EmpresaId, Nome,        Cnpj,                      InscricaoEstadual, InscricaoMunicipal,
     Endereco,                                      Telefone,
     FlExcluido, DataCriacao, DataAlteracao, AlteradoPor)
VALUES
    (@Empresa1Id, N'Matriz', N'12.345.678/0001-99',    N'ISENTO',         N'123456',
     N'Rua das Flores, 1000 - Centro - Cidade A',      N'(11) 99999-0001',
     0,        @AgoraUtc,   NULL,        @UsuarioSistema);

DECLARE @Empresa1_MatrizId int = SCOPE_IDENTITY();

-- Unidade Lapa
INSERT INTO [Empresa].[Unidade]
    (EmpresaId, Nome,                    Cnpj,                      InscricaoEstadual, InscricaoMunicipal,
     Endereco,                                                Telefone,
     FlExcluido, DataCriacao, DataAlteracao, AlteradoPor)
VALUES
    (@Empresa1Id, N'Lapa',              N'12.345.678/0002-70',     N'ISENTO',         N'123457',
     N'Av. Rio Branco, 250 - Lapa - Cidade A',                     N'(11) 98888-0002',
     0,        @AgoraUtc,   NULL,        @UsuarioSistema);

DECLARE @Empresa1_LapaId int = SCOPE_IDENTITY();

--------------------------------------------------------------------------------
-- EMPRESA 2: ConsertaSmart Serviços Digitais LTDA
--------------------------------------------------------------------------------
INSERT INTO [Empresa].[Empresa]
    (RazaoSocial,                            NomeFantasia,        Cnpj,              FlExcluido, DataCriacao,  DataAlteracao, AlteradoPor)
VALUES
    (N'ConsertaSmart Serviços Digitais LTDA', N'ConsertaSmart Digital', N'98.765.432/0001-55', 0, @AgoraUtc,    NULL,          @UsuarioSistema);

DECLARE @Empresa2Id int = SCOPE_IDENTITY();

-- Unidade Matriz Digital
INSERT INTO [Empresa].[Unidade]
    (EmpresaId, Nome,        Cnpj,                      InscricaoEstadual, InscricaoMunicipal,
     Endereco,                                      Telefone,
     FlExcluido, DataCriacao, DataAlteracao, AlteradoPor)
VALUES
    (@Empresa2Id, N'Matriz', N'98.765.432/0001-55',    N'ISENTO',         N'987654',
     N'Rua da Tecnologia, 500 - Centro - Cidade B',    N'(21) 97777-1000',
     0,        @AgoraUtc,   NULL,        @UsuarioSistema);

DECLARE @Empresa2_MatrizId int = SCOPE_IDENTITY();

-- Unidade Shopping Center
INSERT INTO [Empresa].[Unidade]
    (EmpresaId, Nome,                    Cnpj,                      InscricaoEstadual, InscricaoMunicipal,
     Endereco,                                                Telefone,
     FlExcluido, DataCriacao, DataAlteracao, AlteradoPor)
VALUES
    (@Empresa2Id, N'Shopping Center',    N'98.765.432/0002-26',     N'ISENTO',         N'987655',
     N'Shopping Center B, Piso 2, Loja 210 - Cidade B',             N'(21) 96666-2000',
     0,        @AgoraUtc,   NULL,        @UsuarioSistema);

DECLARE @Empresa2_ShoppingId int = SCOPE_IDENTITY();

--------------------------------------------------------------------------------
-- EMPRESA 3: TechFix Mobile Assistência Técnica
--------------------------------------------------------------------------------
INSERT INTO [Empresa].[Empresa]
    (RazaoSocial,                                    NomeFantasia,      Cnpj,              FlExcluido, DataCriacao,  DataAlteracao, AlteradoPor)
VALUES
    (N'TechFix Mobile Assistência Técnica LTDA',     N'TechFix Mobile', N'45.678.910/0001-11', 0, @AgoraUtc, NULL, @UsuarioSistema);

DECLARE @Empresa3Id int = SCOPE_IDENTITY();

-- Unidade Matriz
INSERT INTO [Empresa].[Unidade]
    (EmpresaId, Nome,        Cnpj,                      InscricaoEstadual, InscricaoMunicipal,
     Endereco,                                      Telefone,
     FlExcluido, DataCriacao, DataAlteracao, AlteradoPor)
VALUES
    (@Empresa3Id, N'Matriz', N'45.678.910/0001-11',    N'ISENTO',         N'456789',
     N'Av. das Torres, 1500 - Centro - Cidade C',      N'(31) 95555-3000',
     0,        @AgoraUtc,   NULL,        @UsuarioSistema);

DECLARE @Empresa3_MatrizId int = SCOPE_IDENTITY();

-- Unidade Bairro Industrial
INSERT INTO [Empresa].[Unidade]
    (EmpresaId, Nome,                      Cnpj,                      InscricaoEstadual, InscricaoMunicipal,
     Endereco,                                                    Telefone,
     FlExcluido, DataCriacao, DataAlteracao, AlteradoPor)
VALUES
    (@Empresa3Id, N'Bairro Industrial',    N'45.678.910/0002-90',     N'ISENTO',         N'456790',
     N'Rua da Produção, 800 - Bairro Industrial - Cidade C',         N'(31) 94444-3001',
     0,        @AgoraUtc,   NULL,        @UsuarioSistema);

DECLARE @Empresa3_IndustrialId int = SCOPE_IDENTITY();

--------------------------------------------------------------------------------
-- CONSULTAS RÁPIDAS PARA CONFERÊNCIA
--------------------------------------------------------------------------------
SELECT TOP 10 *
FROM [Empresa].[Empresa]
ORDER BY Id;

SELECT TOP 10 *
FROM [Empresa].[Unidade]
ORDER BY EmpresaId, Id;
GO
