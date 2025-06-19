using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Application.Validations
{
    public static class TransacaoDTOValidator
    {
        public static ValidationResult Validate(TransacaoDTO transacaoDto)
        {
            if (string.IsNullOrWhiteSpace(transacaoDto.Descricao))
            {
                return new ValidationResult("A descrição da transação é obrigatória.");
            }

            if (transacaoDto.Descricao.Length > 100)
            {
                return new ValidationResult("A descrição deve ter no máximo 100 caracteres.");
            }

            if (transacaoDto.Valor <= 0)
            {
                return new ValidationResult("O valor da transação deve ser maior que zero.");
            }

            if (string.IsNullOrWhiteSpace(transacaoDto.Tipo))
            {
                return new ValidationResult("O tipo da transação é obrigatório.");
            }

            if (!Enum.TryParse<TipoTransacao>(transacaoDto.Tipo, true, out _))
            {
                return new ValidationResult("O tipo da transação deve ser 'Receita' ou 'Despesa'.");
            }

            return ValidationResult.Success;
        }
    }
} 