using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Validations;
using FluentValidation;
using FluentValidation.Results;
using System.Threading;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Tests.TestHelpers
{
    /// <summary>
    /// Classes de validadores de teste que herdam dos validadores reais e permitem configurar o resultado da validação
    /// </summary>
    public class TestTransacaoDTOValidator : TransacaoDTOValidator
    {
        private ValidationResult _validationResult;

        public TestTransacaoDTOValidator()
        {
            _validationResult = new ValidationResult();
        }

        public void SetValidationResult(ValidationResult validationResult)
        {
            _validationResult = validationResult;
        }

        public override ValidationResult Validate(ValidationContext<TransacaoDTO> context)
        {
            return _validationResult;
        }

        public override Task<ValidationResult> ValidateAsync(ValidationContext<TransacaoDTO> context, CancellationToken cancellation = default)
        {
            return Task.FromResult(_validationResult);
        }
    }

    public class TestCreateTransacaoDTOValidator : CreateTransacaoDTOValidator
    {
        private ValidationResult _validationResult;

        public TestCreateTransacaoDTOValidator()
        {
            _validationResult = new ValidationResult();
        }

        public void SetValidationResult(ValidationResult validationResult)
        {
            _validationResult = validationResult;
        }

        public override ValidationResult Validate(ValidationContext<CreateTransacaoDTO> context)
        {
            return _validationResult;
        }

        public override Task<ValidationResult> ValidateAsync(ValidationContext<CreateTransacaoDTO> context, CancellationToken cancellation = default)
        {
            return Task.FromResult(_validationResult);
        }
    }

    public class TestUpdateTransacaoDTOValidator : UpdateTransacaoDTOValidator
    {
        private ValidationResult _validationResult;

        public TestUpdateTransacaoDTOValidator()
        {
            _validationResult = new ValidationResult();
        }

        public void SetValidationResult(ValidationResult validationResult)
        {
            _validationResult = validationResult;
        }

        public override ValidationResult Validate(ValidationContext<UpdateTransacaoDTO> context)
        {
            return _validationResult;
        }

        public override Task<ValidationResult> ValidateAsync(ValidationContext<UpdateTransacaoDTO> context, CancellationToken cancellation = default)
        {
            return Task.FromResult(_validationResult);
        }
    }
} 