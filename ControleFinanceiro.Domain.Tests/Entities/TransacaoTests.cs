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
        public void SetTipo_ComTipoValido_DeveAlterarTipo()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            var novoTipo = TipoTransacao.Despesa;

            // Act
            transacao.SetTipo(novoTipo);

            // Assert
            transacao.Tipo.Should().Be(novoTipo);
            transacao.DataAlteracao.Should().NotBeNull();
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

        [Fact]
        public void SetData_ComDataValida_DeveAlterarData()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-10), "Teste", 100m);
            var novaData = DateTime.Now.AddDays(-1);

            // Act
            transacao.SetData(novaData);

            // Assert
            transacao.Data.Should().Be(novaData);
            transacao.DataAlteracao.Should().NotBeNull();
        }

        [Fact]
        public void SetData_ComDataFutura_DeveLancarExcecao()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            var dataFutura = DateTime.Now.AddDays(1);

            // Act & Assert
            Action action = () => transacao.SetData(dataFutura);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("Não é permitido registrar transações com data futura");
        }

        [Fact]
        public void SetData_ComDataMuitoAntiga_DeveLancarExcecao()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            var dataMuitoAntiga = DateTime.Now.AddYears(-6);

            // Act & Assert
            Action action = () => transacao.SetData(dataMuitoAntiga);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("Não é permitido registrar transações com mais de 5 anos");
        }

        [Fact]
        public void SetDescricao_ComDescricaoValida_DeveAlterarDescricao()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            var novaDescricao = "Nova descrição";

            // Act
            transacao.SetDescricao(novaDescricao);

            // Assert
            transacao.Descricao.Should().Be(novaDescricao);
            transacao.DataAlteracao.Should().NotBeNull();
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

        [Fact]
        public void SetDescricao_ComDescricaoMuitoLonga_DeveLancarExcecao()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            var descricaoMuitoLonga = new string('A', Transacao.DESCRICAO_MAX_LENGTH + 1);

            // Act & Assert
            Action action = () => transacao.SetDescricao(descricaoMuitoLonga);
            action.Should().Throw<ArgumentException>()
                  .WithMessage($"A descrição deve ter no máximo {Transacao.DESCRICAO_MAX_LENGTH} caracteres");
        }

        [Fact]
        public void SetValor_ComValorValido_DeveAlterarValor()
        {
            // Arrange
            var transacao = new Transacao(TipoTransacao.Receita, DateTime.Now.AddDays(-1), "Teste", 100m);
            var novoValor = 200m;

            // Act
            transacao.SetValor(novoValor);

            // Assert
            transacao.Valor.Should().Be(novoValor);
            transacao.DataAlteracao.Should().NotBeNull();
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