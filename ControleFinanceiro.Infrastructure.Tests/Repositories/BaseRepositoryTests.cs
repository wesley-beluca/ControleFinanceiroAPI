using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Infrastructure.Data;
using ControleFinanceiro.Infrastructure.Repositories;
using ControleFinanceiro.Infrastructure.Tests.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ControleFinanceiro.Infrastructure.Tests.Repositories
{
    public class TestEntity : Entity
    {
        public string Nome { get; private set; }

        public TestEntity(string nome)
        {
            Nome = nome;
        }

        public void SetNome(string nome)
        {
            Nome = nome;
        }
    }

    public class BaseRepositoryTests : IDisposable
    {
        private readonly TestAppDbContext _context;
        private readonly BaseRepository<TestEntity> _repository;

        public BaseRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"InMemoryDbForTesting{Guid.NewGuid()}")
                .Options;

            _context = new TestAppDbContext(options);
            _repository = new BaseRepository<TestEntity>(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task AddAsync_DeveAdicionarEntidadeERetornarId()
        {
            // Arrange
            var entity = new TestEntity("Teste Entity");

            // Act
            var id = await _repository.AddAsync(entity);

            // Assert
            id.Should().NotBeEmpty();
            
            var savedEntity = await _context.Set<TestEntity>().FindAsync(id);
            savedEntity.Should().NotBeNull();
            savedEntity.Nome.Should().Be("Teste Entity");
            savedEntity.Excluido.Should().BeFalse();
            savedEntity.DataInclusao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task GetAllAsync_DeveRetornarTodasEntidadesNaoExcluidas()
        {
            // Arrange
            var entity1 = new TestEntity("Entity 1");
            var entity2 = new TestEntity("Entity 2");
            var entity3 = new TestEntity("Entity 3");
            
            await _context.AddRangeAsync(entity1, entity2, entity3);
            await _context.SaveChangesAsync();
            
            // Marca entity3 como excluído
            entity3.MarcarComoExcluido();
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            var entities = result.ToList();
            entities.Should().HaveCount(2);
            entities.Should().Contain(e => e.Nome == "Entity 1");
            entities.Should().Contain(e => e.Nome == "Entity 2");
            entities.Should().NotContain(e => e.Nome == "Entity 3");
        }

        [Fact]
        public async Task GetAllNoTrackingAsync_DeveRetornarEntidadesSemRastreamento()
        {
            // Arrange
            var entity = new TestEntity("Test No Tracking");
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllNoTrackingAsync();
            var retrievedEntity = result.First();
            retrievedEntity.SetNome("Modified Name");

            // Assert
            // Se o rastreamento estiver desativado, a mudança não será refletida no contexto
            var entityInContext = await _context.Set<TestEntity>().FindAsync(entity.Id);
            entityInContext.Nome.Should().Be("Test No Tracking");
        }

        [Fact]
        public async Task UpdateAsync_DeveAtualizarEntidadeManterIdEDataInclusao()
        {
            // Arrange
            var entity = new TestEntity("Original Name");
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            var originalId = entity.Id;
            var originalDataInclusao = entity.DataInclusao;
            var dataAnteriorAlteracao = entity.DataAlteracao;

            // Modificar a entidade
            entity.SetNome("Updated Name");

            // Act
            await _repository.UpdateAsync(entity);

            // Assert
            var updatedEntity = await _context.Set<TestEntity>().FindAsync(entity.Id);
            updatedEntity.Should().NotBeNull();
            updatedEntity.Id.Should().Be(originalId); // O ID não deve mudar
            updatedEntity.Nome.Should().Be("Updated Name"); // O nome deve ser atualizado
            updatedEntity.DataInclusao.Should().Be(originalDataInclusao); // Data de inclusão não deve mudar
            updatedEntity.DataAlteracao.Should().NotBe(dataAnteriorAlteracao); // Data de alteração deve ser atualizada
        }

        [Fact]
        public async Task DeleteAsync_DeveFazerSoftDelete()
        {
            // Arrange
            var entity = new TestEntity("Entity to Delete");
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id);

            // Assert
            var deletedEntity = await _context.Set<TestEntity>().FindAsync(entity.Id);
            deletedEntity.Should().NotBeNull(); // Ainda deve existir no banco
            deletedEntity.Excluido.Should().BeTrue(); // Mas deve estar marcado como excluído
        }

        [Fact]
        public async Task ExistsAsync_QuandoEntidadeExiste_DeveRetornarTrue()
        {
            // Arrange
            var entity = new TestEntity("Existing Entity");
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repository.ExistsAsync(entity.Id);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_QuandoEntidadeNaoExiste_DeveRetornarFalse()
        {
            // Act
            var exists = await _repository.ExistsAsync(Guid.NewGuid());

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsAsync_QuandoEntidadeExcluidaSoftDelete_DeveRetornarFalse()
        {
            // Arrange
            var entity = new TestEntity("Entity to Check");
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            entity.MarcarComoExcluido();
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repository.ExistsAsync(entity.Id);

            // Assert
            exists.Should().BeFalse();
        }
    }
} 