-- Cria uma nova tabela com a estrutura correta
CREATE TABLE [Transacoes_New] (
    [Id] uniqueidentifier NOT NULL,
    [Tipo] int NOT NULL,
    [Data] datetime2 NOT NULL,
    [Descricao] nvarchar(100) NOT NULL,
    [Valor] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Transacoes_New] PRIMARY KEY ([Id])
);

-- Insere dados da tabela antiga para a nova com GUIDs gerados
INSERT INTO [Transacoes_New] ([Id], [Tipo], [Data], [Descricao], [Valor])
SELECT NEWID(), [Tipo], [Data], [Descricao], [Valor]
FROM [Transacoes];

-- Remove a tabela antiga
DROP TABLE [Transacoes];

-- Renomeia a nova tabela para o nome original
EXEC sp_rename 'Transacoes_New', 'Transacoes'; 