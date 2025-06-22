using System;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ControleFinanceiro.Application.Tests.Validations
{
    public class TransacaoDTOValidatorTests
    {
        private readonly TransacaoDTOValidator _validator;

        public TransacaoDTOValidatorTests()
        {
            _validator = new TransacaoDTOValidator();
        }

        [Fact]
        public void TransacaoDTO_ComDadosValidos_DevePassarNaValidacao()
        {
            // Arrange
            var transacaoDTO = new TransacaoDTO
            {
                Id = Guid.NewGuid(),
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Pagamento de freelance",
                Valor = 1000.00m,
                DataInclusao = DateTime.Now.AddDays(-1),
                DataAlteracao = null
            };

            // Act
            var result = _validator.TestValidate(transacaoDTO);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validar_DescricaoVaziaOuNula_DeveRetornarErro(string descricao)
        {
            // Arrange
            var transacaoDTO = new TransacaoDTO
            {
                Id = Guid.NewGuid(),
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = descricao,
                Valor = 100m
            };

            // Act
            var result = _validator.TestValidate(transacaoDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Descricao);
        }

        [Fact]
        public void Validar_DescricaoMuitoLonga_DeveRetornarErro()
        {
            // Arrange
            var descricaoLonga = new string('A', Transacao.DESCRICAO_MAX_LENGTH + 1);
            var transacaoDTO = new TransacaoDTO
            {
                Id = Guid.NewGuid(),
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = descricaoLonga,
                Valor = 100m
            };

            // Act
            var result = _validator.TestValidate(transacaoDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Descricao)
                .WithErrorMessage($"A descrição deve ter no máximo {Transacao.DESCRICAO_MAX_LENGTH} caracteres.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Validar_ValorMenorOuIgualZero_DeveRetornarErro(decimal valorInvalido)
        {
            // Arrange
            var transacaoDTO = new TransacaoDTO
            {
                Id = Guid.NewGuid(),
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Teste",
                Valor = valorInvalido
            };

            // Act
            var result = _validator.TestValidate(transacaoDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Valor);
        }

        [Theory]
        [InlineData(-1)]  // Valor não existe no enum
        [InlineData(2)]   // Valor não existe no enum
        [InlineData(999)] // Valor não existe no enum
        public void Validar_TipoInvalido_DeveRetornarErro(int tipoInvalido)
        {
            // Arrange
            var transacaoDTO = new TransacaoDTO
            {
                Id = Guid.NewGuid(),
                Tipo = tipoInvalido,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Teste",
                Valor = 100m
            };

            // Act
            var result = _validator.TestValidate(transacaoDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Tipo);
        }
    }

    public class CreateTransacaoDTOValidatorTests
    {
        private readonly CreateTransacaoDTOValidator _validator;

        public CreateTransacaoDTOValidatorTests()
        {
            _validator = new CreateTransacaoDTOValidator();
        }

        [Fact]
        public void CreateTransacaoDTO_ComDadosValidos_DevePassarNaValidacao()
        {
            // Arrange
            var createDTO = new CreateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Pagamento de freelance",
                Valor = 1000.00m
            };

            // Act
            var result = _validator.TestValidate(createDTO);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validar_DescricaoVaziaOuNula_DeveRetornarErro(string descricao)
        {
            // Arrange
            var createDTO = new CreateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = descricao,
                Valor = 100m
            };

            // Act
            var result = _validator.TestValidate(createDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Descricao);
        }

        [Fact]
        public void Validar_DescricaoMuitoLonga_DeveRetornarErro()
        {
            // Arrange
            var descricaoLonga = new string('A', Transacao.DESCRICAO_MAX_LENGTH + 1);
            var createDTO = new CreateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = descricaoLonga,
                Valor = 100m
            };

            // Act
            var result = _validator.TestValidate(createDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Descricao);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Validar_ValorMenorOuIgualZero_DeveRetornarErro(decimal valorInvalido)
        {
            // Arrange
            var createDTO = new CreateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Teste",
                Valor = valorInvalido
            };

            // Act
            var result = _validator.TestValidate(createDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Valor);
        }

        [Theory]
        [InlineData(-1)]  // Valor não existe no enum
        [InlineData(2)]   // Valor não existe no enum
        [InlineData(999)] // Valor não existe no enum
        public void Validar_TipoInvalido_DeveRetornarErro(int tipoInvalido)
        {
            // Arrange
            var createDTO = new CreateTransacaoDTO
            {
                Tipo = tipoInvalido,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Teste",
                Valor = 100m
            };

            // Act
            var result = _validator.TestValidate(createDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Tipo);
        }
    }

    public class UpdateTransacaoDTOValidatorTests
    {
        private readonly UpdateTransacaoDTOValidator _validator;

        public UpdateTransacaoDTOValidatorTests()
        {
            _validator = new UpdateTransacaoDTOValidator();
        }

        [Fact]
        public void UpdateTransacaoDTO_ComDadosValidos_DevePassarNaValidacao()
        {
            // Arrange
            var updateDTO = new UpdateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Pagamento de freelance atualizado",
                Valor = 1200.00m
            };

            // Act
            var result = _validator.TestValidate(updateDTO);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validar_DescricaoVaziaOuNula_DeveRetornarErro(string descricao)
        {
            // Arrange
            var updateDTO = new UpdateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = descricao,
                Valor = 100m
            };

            // Act
            var result = _validator.TestValidate(updateDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Descricao);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Validar_ValorMenorOuIgualZero_DeveRetornarErro(decimal valorInvalido)
        {
            // Arrange
            var updateDTO = new UpdateTransacaoDTO
            {
                Tipo = (int)TipoTransacao.Receita,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Teste",
                Valor = valorInvalido
            };

            // Act
            var result = _validator.TestValidate(updateDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Valor);
        }

        [Theory]
        [InlineData(-1)]  // Valor não existe no enum
        [InlineData(2)]   // Valor não existe no enum
        [InlineData(999)] // Valor não existe no enum
        public void Validar_TipoInvalido_DeveRetornarErro(int tipoInvalido)
        {
            // Arrange
            var updateDTO = new UpdateTransacaoDTO
            {
                Tipo = tipoInvalido,
                Data = DateTime.Now.AddDays(-1),
                Descricao = "Teste",
                Valor = 100m
            };

            // Act
            var result = _validator.TestValidate(updateDTO);

            // Assert
            result.ShouldHaveValidationErrorFor(t => t.Tipo);
        }
    }
} 