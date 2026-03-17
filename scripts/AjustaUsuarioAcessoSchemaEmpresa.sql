BEGIN TRANSACTION;
IF SCHEMA_ID(N'Auth') IS NULL EXEC(N'CREATE SCHEMA [Auth];');

IF SCHEMA_ID(N'Clientes') IS NULL EXEC(N'CREATE SCHEMA [Clientes];');

IF SCHEMA_ID(N'Empresa') IS NULL EXEC(N'CREATE SCHEMA [Empresa];');

IF SCHEMA_ID(N'OrdemServico') IS NULL EXEC(N'CREATE SCHEMA [OrdemServico];');

CREATE TABLE [Auth].[AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [Auth].[AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Clientes].[Cliente] (
    [Id] int NOT NULL IDENTITY,
    [EmpresaId] int NOT NULL,
    [UnidadeId] int NOT NULL,
    [Nome] nvarchar(150) NOT NULL,
    [Documento] nvarchar(20) NULL,
    [Telefone] nvarchar(20) NULL,
    [Email] nvarchar(150) NULL,
    [FlExcluido] bit NOT NULL,
    [DataCriacao] datetime2 NOT NULL,
    [DataAlteracao] datetime2 NULL,
    [AlteradoPor] int NULL,
    CONSTRAINT [PK_Cliente] PRIMARY KEY ([Id])
);

CREATE TABLE [Empresa].[Empresa] (
    [Id] int NOT NULL IDENTITY,
    [RazaoSocial] nvarchar(200) NOT NULL,
    [NomeFantasia] nvarchar(200) NOT NULL,
    [Cnpj] nvarchar(18) NULL,
    [FlExcluido] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DataCriacao] datetime2 NOT NULL,
    [DataAlteracao] datetime2 NULL,
    [AlteradoPor] int NULL,
    CONSTRAINT [PK_Empresa] PRIMARY KEY ([Id])
);

CREATE TABLE [OrdemServico].[PagamentoOs] (
    [Id] int NOT NULL IDENTITY,
    [OrdemServicoId] int NOT NULL,
    [TipoPagamento] tinyint NOT NULL,
    [FormaPagamento] tinyint NOT NULL,
    [Valor] decimal(18,2) NOT NULL,
    [StatusRegistro] tinyint NOT NULL,
    [DataRegistro] datetime2 NOT NULL,
    [DataConfirmacao] datetime2 NULL,
    [FlExcluido] bit NOT NULL,
    CONSTRAINT [PK_PagamentoOs] PRIMARY KEY ([Id])
);

CREATE TABLE [Auth].[SecurityKeys] (
    [Id] uniqueidentifier NOT NULL,
    [KeyId] nvarchar(max) NULL,
    [Type] nvarchar(max) NULL,
    [Use] nvarchar(max) NULL,
    [Parameters] nvarchar(max) NULL,
    [IsRevoked] bit NOT NULL,
    [RevokedReason] nvarchar(max) NULL,
    [CreationDate] datetime2 NOT NULL,
    [ExpiredAt] datetime2 NULL,
    CONSTRAINT [PK_SecurityKeys] PRIMARY KEY ([Id])
);

CREATE TABLE [OrdemServico].[StatusHistorico] (
    [Id] int NOT NULL IDENTITY,
    [OrdemServicoId] int NOT NULL,
    [TipoEvento] nvarchar(50) NOT NULL,
    [StatusOsAnterior] nvarchar(50) NULL,
    [StatusOsNovo] nvarchar(50) NULL,
    [DescricaoEvento] nvarchar(max) NULL,
    [DataEvento] datetime2 NOT NULL,
    [UsuarioId] int NULL,
    CONSTRAINT [PK_StatusHistorico] PRIMARY KEY ([Id])
);

CREATE TABLE [Empresa].[Tecnico] (
    [Id] int NOT NULL IDENTITY,
    [EmpresaId] int NOT NULL,
    [UnidadeId] int NOT NULL,
    [Nome] nvarchar(150) NOT NULL,
    [Apelido] nvarchar(50) NULL,
    [Ativo] bit NOT NULL,
    [FlExcluido] bit NOT NULL,
    [DataCriacao] datetime2 NOT NULL,
    [DataAlteracao] datetime2 NULL,
    [AlteradoPor] int NULL,
    CONSTRAINT [PK_Tecnico] PRIMARY KEY ([Id])
);

CREATE TABLE [Auth].[AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Auth].[AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Auth].[AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [Auth].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Auth].[AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [Auth].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Auth].[AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Auth].[AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [Auth].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Auth].[AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [Auth].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Auth].[RefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [TokenHash] nvarchar(64) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [ExpiresAtUtc] datetime2 NOT NULL,
    [RevokedAtUtc] datetime2 NULL,
    [ReplacedByTokenHash] nvarchar(max) NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [Auth].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Empresa].[Unidade] (
    [Id] int NOT NULL IDENTITY,
    [EmpresaId] int NOT NULL,
    [Nome] nvarchar(200) NOT NULL,
    [Cnpj] nvarchar(18) NULL,
    [InscricaoEstadual] nvarchar(50) NULL,
    [InscricaoMunicipal] nvarchar(50) NULL,
    [Endereco] nvarchar(300) NULL,
    [Telefone] nvarchar(50) NULL,
    [FlExcluido] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DataCriacao] datetime2 NOT NULL,
    [DataAlteracao] datetime2 NULL,
    [AlteradoPor] int NULL,
    CONSTRAINT [PK_Unidade] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Unidade_Empresa_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [Empresa].[Empresa] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Empresa].[UsuarioAcesso] (
    [Id] int NOT NULL IDENTITY,
    [Nome] nvarchar(150) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [IdentityUserId] nvarchar(450) NOT NULL,
    [EmpresaId] int NOT NULL,
    [UnidadeId] int NULL,
    [Perfil] tinyint NOT NULL,
    [FlAtivo] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DataCriacao] datetime2 NOT NULL,
    [DataAlteracao] datetime2 NULL,
    [AlteradoPor] int NULL,
    CONSTRAINT [PK_UsuarioAcesso] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UsuarioAcesso_Empresa_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [Empresa].[Empresa] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UsuarioAcesso_Unidade_UnidadeId] FOREIGN KEY ([UnidadeId]) REFERENCES [Empresa].[Unidade] ([Id])
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [Auth].[AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [Auth].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [Auth].[AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [Auth].[AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [Auth].[AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [Auth].[AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [Auth].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [Auth].[RefreshTokens] ([TokenHash]);

CREATE INDEX [IX_RefreshTokens_UserId] ON [Auth].[RefreshTokens] ([UserId]);

CREATE INDEX [IX_Unidade_EmpresaId] ON [Empresa].[Unidade] ([EmpresaId]);

CREATE INDEX [IX_UsuarioAcesso_EmpresaId] ON [Empresa].[UsuarioAcesso] ([EmpresaId]);

CREATE UNIQUE INDEX [IX_UsuarioAcesso_IdentityUserId] ON [Empresa].[UsuarioAcesso] ([IdentityUserId]);

CREATE INDEX [IX_UsuarioAcesso_IdentityUserId_EmpresaId] ON [Empresa].[UsuarioAcesso] ([IdentityUserId], [EmpresaId]);

CREATE INDEX [IX_UsuarioAcesso_UnidadeId] ON [Empresa].[UsuarioAcesso] ([UnidadeId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260316214154_AjustaUsuarioAcessoSchemaEmpresa', N'10.0.1');

COMMIT;
GO

