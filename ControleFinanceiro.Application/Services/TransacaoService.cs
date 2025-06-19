using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Services
{
    public class TransacaoService : ITransacaoService
    {
        private readonly ITransacaoRepository _transacaoRepository;

        public TransacaoService(ITransacaoRepository transacaoRepository)
        {
            _transacaoRepository = transacaoRepository;
        }

        public async Task<TransacaoDTO> GetByIdAsync(Guid id)
        {
            var transacao = await _transacaoRepository.GetByIdAsync(id);
            if (transacao == null) return null;

            return new TransacaoDTO
            {
                Id = transacao.Id,
                Tipo = transacao.Tipo.ToString(),
                Data = transacao.Data,
                Descricao = transacao.Descricao,
                Valor = transacao.Valor
            };
        }

        public async Task<IEnumerable<TransacaoDTO>> GetAllAsync()
        {
            var transacoes = await _transacaoRepository.GetAllAsync();
            return transacoes.Select(t => new TransacaoDTO
            {
                Id = t.Id,
                Tipo = t.Tipo.ToString(),
                Data = t.Data,
                Descricao = t.Descricao,
                Valor = t.Valor
            });
        }

        public async Task<IEnumerable<TransacaoDTO>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            var transacoes = await _transacaoRepository.GetByPeriodoAsync(dataInicio, dataFim);
            return transacoes.Select(t => new TransacaoDTO
            {
                Id = t.Id,
                Tipo = t.Tipo.ToString(),
                Data = t.Data,
                Descricao = t.Descricao,
                Valor = t.Valor
            });
        }

        public async Task<IEnumerable<TransacaoDTO>> GetByTipoAsync(string tipo)
        {
            if (!Enum.TryParse<TipoTransacao>(tipo, true, out var tipoEnum))
                throw new ArgumentException("Tipo de transação inválido");

            var transacoes = await _transacaoRepository.GetByTipoAsync(tipoEnum);
            return transacoes.Select(t => new TransacaoDTO
            {
                Id = t.Id,
                Tipo = t.Tipo.ToString(),
                Data = t.Data,
                Descricao = t.Descricao,
                Valor = t.Valor
            });
        }

        public async Task<Guid> AddAsync(TransacaoDTO transacaoDto)
        {
            var validationResult = TransacaoDTOValidator.Validate(transacaoDto);
            if (validationResult != ValidationResult.Success)
                throw new ArgumentException(validationResult.ErrorMessage);

            if (!Enum.TryParse<TipoTransacao>(transacaoDto.Tipo, true, out var tipoEnum))
                throw new ArgumentException("Tipo de transação inválido");

            var transacao = new Transacao(
                tipoEnum,
                transacaoDto.Data,
                transacaoDto.Descricao,
                transacaoDto.Valor
            );

            return await _transacaoRepository.AddAsync(transacao);
        }

        public async Task UpdateAsync(TransacaoDTO transacaoDto)
        {
            var validationResult = TransacaoDTOValidator.Validate(transacaoDto);
            if (validationResult != ValidationResult.Success)
                throw new ArgumentException(validationResult.ErrorMessage);

            if (!Enum.TryParse<TipoTransacao>(transacaoDto.Tipo, true, out var tipoEnum))
                throw new ArgumentException("Tipo de transação inválido");

            var transacao = await _transacaoRepository.GetByIdAsync(transacaoDto.Id);
            if (transacao == null)
                throw new KeyNotFoundException($"Transação com ID {transacaoDto.Id} não encontrada");

            transacao.Tipo = tipoEnum;
            transacao.Data = transacaoDto.Data;
            transacao.Descricao = transacaoDto.Descricao;
            transacao.Valor = transacaoDto.Valor;

            await _transacaoRepository.UpdateAsync(transacao);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _transacaoRepository.DeleteAsync(id);
        }
    }
} 