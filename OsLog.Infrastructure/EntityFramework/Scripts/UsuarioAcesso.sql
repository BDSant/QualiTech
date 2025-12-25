CREATE TABLE [Empresa].[UsuarioAcesso](
    [Id]            INT IDENTITY(1,1) NOT NULL,
    [UserId]        NVARCHAR(450) NOT NULL,      -- FK AspNetUsers.Id
    [EmpresaId]     INT NOT NULL,                -- FK Empresa.Empresa
    [UnidadeId]     INT NULL,                    -- FK Empresa.Unidade (NULL = todas unidades da empresa)
    [Perfil]        TINYINT NOT NULL,            -- 1=Dono, 2=Financeiro, 3=GerenteUnidade, 4=Tecnico, etc.
    [FlExcluido]    BIT NOT NULL CONSTRAINT DF_UsuarioAcesso_FlExcluido DEFAULT(0),
    [DataCriacao]   DATETIME2(7) NOT NULL,
    [DataAlteracao] DATETIME2(7) NULL,
    [AlteradoPor]   INT NULL,
    CONSTRAINT [PK_UsuarioAcesso] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

ALTER TABLE [Empresa].[UsuarioAcesso] WITH CHECK
ADD CONSTRAINT [FK_UsuarioAcesso_AspNetUsers_UserId]
    FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]);
GO

ALTER TABLE [Empresa].[UsuarioAcesso] WITH CHECK
ADD CONSTRAINT [FK_UsuarioAcesso_Empresa_EmpresaId]
    FOREIGN KEY([EmpresaId]) REFERENCES [Empresa].[Empresa]([Id]);
GO

ALTER TABLE [Empresa].[UsuarioAcesso] WITH CHECK
ADD CONSTRAINT [FK_UsuarioAcesso_Unidade_UnidadeId]
    FOREIGN KEY([UnidadeId]) REFERENCES [Empresa].[Unidade]([Id]);
GO

-- Evita duplicar o mesmo vínculo para o mesmo usuário
CREATE UNIQUE INDEX IX_UsuarioAcesso_User_Empresa_Unidade
ON [Empresa].[UsuarioAcesso]([UserId], [EmpresaId], [UnidadeId]);
GO
