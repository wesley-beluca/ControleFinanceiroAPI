using System;
using ControleFinanceiro.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ControleFinanceiro.Domain.Tests.Entities
{
    public class TransacaoTests
    {
        [Fact]
        public void CriarTransacao_ComDadosValidos_DeveCriarComSucesso()
        {
            // Arrange
            var tipo = TipoTransacao.Receita;
            var data = DateTime.Now.AddDays(-1);
            var descricao = "Pagamento de freelance";
            var valor = 1500.00m;

            // Act
            var transacao = new Transacao(tipo, data, descricao, valor);

            // Assert
            transacao.Tipo.Should().Be(tipo);
            transacao.Data.Should().Be(data);
            transacao.Descricao.Should().Be(descricao);
            transacao.Valor.Should().Be(valor);
            transacao.Id.Should().NotBe(Guid.Empty);
            transacao.DataInclusao.Should().NotBe(default(DateTime));
            transacao.Excluido.Should().BeFalse();
        }

        [Fact]
        public void SetTipo_ComTipoInvalido_DeveLancarExcecao()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            
            // Act & Assert
            // Tentamos passar um valor que não existe no enum TipoTransacao
            Action action = () => transacao.SetTipo((TipoTransacao)999);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("Tipo de transação inválido");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void SetDescricao_ComDescricaoInvalida_DeveLancarExcecao(string descricaoInvalida)
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);

            // Act & Assert
            Action action = () => transacao.SetDescricao(descricaoInvalida);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("A descrição da transação é obrigatória");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void SetValor_ComValorInvalido_DeveLancarExcecao(decimal valorInvalido)
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);

            // Act & Assert
            Action action = () => transacao.SetValor(valorInvalido);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("O valor da transação deve ser maior que zero");
        }

        [Fact]
        public void MarcarComoExcluido_DeveMarcarComoExcluido()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);

            // Act
            transacao.MarcarComoExcluido();

            // Assert
            transacao.Excluido.Should().BeTrue();
            transacao.DataAlteracao.Should().NotBeNull();
        }
    }
} 