using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using FluentValidation;
using System;

namespace ControleFinanceiro.Application.Validations
{
    public class TransacaoDTOValidator : AbstractValidator<TransacaoDTO>
    {
        public TransacaoDTOValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O ID da transação é obrigatório.");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição da transação é obrigatória.")
                .MaximumLength(Transacao.DESCRICAO_MAX_LENGTH)
                .WithMessage($"A descrição deve ter no máximo {Transacao.DESCRICAO_MAX_LENGTH} caracteres.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor da transação deve ser maior que zero.");

            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("O tipo da transação é obrigatório.")
                .Must(tipo => Enum.IsDefined(typeof(TipoTransacao), tipo))
                .WithMessage("O tipo da transação deve ser 0 (Despesa) ou 1 (Receita).");

            RuleFor(x => x.Data)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Não é permitido registrar transações com data futura.")
                .GreaterThanOrEqualTo(DateTime.Now.AddYears(-5))
                .WithMessage("Não é permitido registrar transações com mais de 5 anos.");
        }
    }

    public class CreateTransacaoDTOValidator : AbstractValidator<CreateTransacaoDTO>
    {
        public CreateTransacaoDTOValidator()
        {
            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição da transação é obrigatória.")
                .MaximumLength(Transacao.DESCRICAO_MAX_LENGTH)
                .WithMessage($"A descrição deve ter no máximo {Transacao.DESCRICAO_MAX_LENGTH} caracteres.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor da transação deve ser maior que zero.");

            RuleFor(x => x.Tipo)
                .Must(tipo => Enum.IsDefined(typeof(TipoTransacao), tipo))
                .WithMessage("O tipo da transação deve ser 0 (Despesa) ou 1 (Receita).");

            RuleFor(x => x.Data)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Não é permitido registrar transações com data futura.")
                .GreaterThanOrEqualTo(DateTime.Now.AddYears(-5))
                .WithMessage("Não é permitido registrar transações com mais de 5 anos.");
        }
    }
    
    public class UpdateTransacaoDTOValidator : AbstractValidator<UpdateTransacaoDTO>
    {
        public UpdateTransacaoDTOValidator()
        {
            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição da transação é obrigatória.")
                .MaximumLength(Transacao.DESCRICAO_MAX_LENGTH)
                .WithMessage($"A descrição deve ter no máximo {Transacao.DESCRICAO_MAX_LENGTH} caracteres.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("O valor da transação deve ser maior que zero.");

            RuleFor(x => x.Tipo)
                .Must(tipo => Enum.IsDefined(typeof(TipoTransacao), tipo))
                .WithMessage("O tipo da transação deve ser 0 (Despesa) ou 1 (Receita).");

            RuleFor(x => x.Data)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Não é permitido registrar transações com data futura.")
                .GreaterThanOrEqualTo(DateTime.Now.AddYears(-5))
                .WithMessage("Não é permitido registrar transações com mais de 5 anos.");
        }
    }
} 