using System;
using System.Collections.Generic;
using ControleFinanceiro.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ControleFinanceiro.Domain.Tests.Entities
{
    public class ResultTests
    {
        [Fact]
        public void Ok_ComDadosValidos_DeveRetornarResultadoDeSuccesso()
        {
            // Arrange
            var data = "TestData";
            var message = "Operação realizada com sucesso";

            // Act
            var result = Result<string>.Ok(data, message);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be(message);
            result.Data.Should().Be(data);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Ok_SemMensagem_DeveUsarMensagemPadrao()
        {
            // Arrange
            var data = 42;

            // Act
            var result = Result<int>.Ok(data);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Operação realizada com sucesso");
            result.Data.Should().Be(data);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Fail_ComMensagem_DeveRetornarResultadoDeErro()
        {
            // Arrange
            var errorMessage = "Ocorreu um erro";

            // Act
            var result = Result<bool>.Fail(errorMessage);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(errorMessage);
            result.Data.Should().Be(default);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Fail_ComListaDeErros_DeveRetornarResultadoDeErroComLista()
        {
            // Arrange
            var errors = new List<string> { "Erro 1", "Erro 2", "Erro 3" };

            // Act
            var result = Result<decimal>.Fail(errors);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Ocorreram erros durante a operação");
            result.Data.Should().Be(default);
            result.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void Fail_ComMensagemEListaDeErros_DeveRetornarResultadoDeErroCompleto()
        {
            // Arrange
            var errorMessage = "Erro na validação";
            var errors = new List<string> { "Campo 1 inválido", "Campo 2 obrigatório" };

            // Act
            var result = Result<Guid>.Fail(errorMessage, errors);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(errorMessage);
            result.Data.Should().Be(Guid.Empty);
            result.Errors.Should().BeEquivalentTo(errors);
        }
    }
} 