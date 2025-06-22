using System;
using ControleFinanceiro.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ControleFinanceiro.Domain.Tests.Entities
{
    public class EntityTests
    {
        // Como Entity é uma classe abstrata, precisamos de uma classe concreta para testá-la
        private class TestEntity : Entity
        {
            public TestEntity() : base()
            {
            }
        }

        [Fact]
        public void Constructor_DeveCriarEntidadeComValoresPadrao()
        {
            // Act
            var entity = new TestEntity();

            // Assert
            entity.Id.Should().NotBe(Guid.Empty);
            entity.DataInclusao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
            entity.DataAlteracao.Should().BeNull();
            entity.Excluido.Should().BeFalse();
        }

        [Fact]
        public void AtualizarDataModificacao_DeveAtualizarDataAlteracao()
        {
            // Arrange
            var entity = new TestEntity();
            
            // Garantir que DataAlteracao inicia como null
            entity.DataAlteracao.Should().BeNull();

            // Act
            entity.AtualizarDataModificacao();

            // Assert
            entity.DataAlteracao.Should().NotBeNull();
            entity.DataAlteracao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void MarcarComoExcluido_DeveMudarExcluidoParaTrue()
        {
            // Arrange
            var entity = new TestEntity();
            entity.Excluido.Should().BeFalse();

            // Act
            entity.MarcarComoExcluido();

            // Assert
            entity.Excluido.Should().BeTrue();
            entity.DataAlteracao.Should().NotBeNull();
            entity.DataAlteracao.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }
    }
} 