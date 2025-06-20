using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControleFinanceiro.Application.Services
{
    public class TransacaoService : ITransacaoService
    {
        private readonly ITransacaoRepository _transacaoRepository;
        private readonly TransacaoDTOValidator _transacaoValidator;
        private readonly CreateTransacaoDTOValidator _createTransacaoValidator;
        private readonly UpdateTransacaoDTOValidator _updateTransacaoValidator;

        public TransacaoService(
            ITransacaoRepository transacaoRepository,
            TransacaoDTOValidator transacaoValidator,
            CreateTransacaoDTOValidator createTransacaoValidator,
            UpdateTransacaoDTOValidator updateTransacaoValidator)
        {
            _transacaoRepository = transacaoRepository;
            _transacaoValidator = transacaoValidator;
            _createTransacaoValidator = createTransacaoValidator;
            _updateTransacaoValidator = updateTransacaoValidator;
        }

        public async Task<Result<TransacaoDTO>> GetByIdAsync(Guid id)
        {
            Transacao transacao = await _transacaoRepository.GetByIdAsync(id);
            if (transacao == null)
                return Result<TransacaoDTO>.Fail("Transação não encontrada");

            TransacaoDTO transacaoDto = MapToDto(transacao);
            return Result<TransacaoDTO>.Ok(transacaoDto);
        }

        public async Task<Result<IEnumerable<TransacaoDTO>>> GetAllAsync()
        {
            IEnumerable<Transacao> transacoes = await _transacaoRepository.GetAllAsync();
            
            IEnumerable<TransacaoDTO> transacoesDto = transacoes.Select(t => MapToDto(t));

            return Result<IEnumerable<TransacaoDTO>>.Ok(transacoesDto);
        }

        public async Task<Result<IEnumerable<TransacaoDTO>>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            // Validação das datas
            if (dataInicio > dataFim)
                return Result<IEnumerable<TransacaoDTO>>.Fail("A data inicial não pode ser maior que a data final");

            // Limitar o período de consulta para evitar sobrecarga
            if ((dataFim - dataInicio).TotalDays > 366)
                return Result<IEnumerable<TransacaoDTO>>.Fail("O período de consulta não pode ser maior que 1 ano");

            IEnumerable<Transacao> transacoes = await _transacaoRepository.GetByPeriodoAsync(dataInicio, dataFim);
            
            IEnumerable<TransacaoDTO> transacoesDto = transacoes.Select(t => MapToDto(t));

            return Result<IEnumerable<TransacaoDTO>>.Ok(transacoesDto);
        }

        public async Task<Result<IEnumerable<TransacaoDTO>>> GetByTipoAsync(int tipo)
        {
            if (!Enum.IsDefined(typeof(TipoTransacao), tipo))
                return Result<IEnumerable<TransacaoDTO>>.Fail("Tipo de transação inválido. Use 0 para Despesa ou 1 para Receita");

            IEnumerable<Transacao> transacoes = await _transacaoRepository.GetByTipoAsync((TipoTransacao)tipo);
            
            IEnumerable<TransacaoDTO> transacoesDto = transacoes.Select(t => MapToDto(t));

            return Result<IEnumerable<TransacaoDTO>>.Ok(transacoesDto);
        }

        public async Task<Result<Guid>> AddAsync(CreateTransacaoDTO transacaoDto)
        {
            // Validação do DTO usando FluentValidation
            var validationResult = _createTransacaoValidator.Validate(transacaoDto);
            if (!validationResult.IsValid)
            {
                List<string> errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return Result<Guid>.Fail(errors);
            }

            // Validação do tipo de transação
            if (!Enum.IsDefined(typeof(TipoTransacao), transacaoDto.Tipo))
                return Result<Guid>.Fail("Tipo de transação inválido. Use 0 para Despesa ou 1 para Receita");

            try
            {
                // Criação da entidade com validações internas
                Transacao transacao = new Transacao(
                    (TipoTransacao)transacaoDto.Tipo,
                    transacaoDto.Data,
                    transacaoDto.Descricao,
                    transacaoDto.Valor
                );

                // Persistência
                Guid id = await _transacaoRepository.AddAsync(transacao);
                return Result<Guid>.Ok(id, "Transação cadastrada com sucesso");
            }
            catch (ArgumentException ex)
            {
                return Result<Guid>.Fail(ex.Message);
            }
            catch (Exception)
            {
                return Result<Guid>.Fail("Ocorreu um erro ao cadastrar a transação");
            }
        }

        public async Task<Result<bool>> UpdateAsync(Guid id, UpdateTransacaoDTO transacaoDto)
        {
            // Validação do DTO usando FluentValidation
            var validationResult = _updateTransacaoValidator.Validate(transacaoDto);
            if (!validationResult.IsValid)
            {
                List<string> errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return Result<bool>.Fail(errors);
            }

            // Validação do tipo de transação
            if (!Enum.IsDefined(typeof(TipoTransacao), transacaoDto.Tipo))
                return Result<bool>.Fail("Tipo de transação inválido. Use 0 para Despesa ou 1 para Receita");

            try
            {
                // Verifica se a transação existe
                bool exists = await _transacaoRepository.ExistsAsync(id);
                if (!exists)
                    return Result<bool>.Fail($"Transação com ID {id} não encontrada");

                // Busca a entidade
                Transacao transacao = await _transacaoRepository.GetByIdAsync(id);
                if (transacao == null)
                    return Result<bool>.Fail($"Transação com ID {id} não encontrada");

                // Atualiza os dados da transação de forma mais limpa
                Result<bool> resultadoAtualizacao = AtualizarDadosTransacao(transacao, transacaoDto);
                if (!resultadoAtualizacao.Success)
                    return resultadoAtualizacao;

                // Persistência
                await _transacaoRepository.UpdateAsync(transacao);
                return Result<bool>.Ok(true, "Transação atualizada com sucesso");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Erro ao atualizar transação: {ex.Message}");
            }
        }

        private Result<bool> AtualizarDadosTransacao(Transacao transacao, UpdateTransacaoDTO transacaoDto)
        {
            List<string> errors = new List<string>();

            // Método auxiliar para executar uma ação e capturar exceções
            void ExecutarAcaoSegura(Action acao, string contexto)
            {
                try
                {
                    acao();
                }
                catch (Exception ex)
                {
                    errors.Add($"{contexto}: {ex.Message}");
                }
            }

            ExecutarAcaoSegura(() => transacao.SetTipo((TipoTransacao)transacaoDto.Tipo), "Tipo");
            ExecutarAcaoSegura(() => transacao.SetData(transacaoDto.Data), "Data");
            ExecutarAcaoSegura(() => transacao.SetDescricao(transacaoDto.Descricao), "Descrição");
            ExecutarAcaoSegura(() => transacao.SetValor(transacaoDto.Valor), "Valor");

            if (errors.Count > 0)
                return Result<bool>.Fail(errors);

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            try
            {
                // Verifica se a transação existe
                bool exists = await _transacaoRepository.ExistsAsync(id);
                if (!exists)
                    return Result<bool>.Fail($"Transação com ID {id} não encontrada");

                // Persistência (utilizando soft delete)
                await _transacaoRepository.DeleteAsync(id);
                return Result<bool>.Ok(true, "Transação excluída com sucesso");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Erro ao excluir transação: {ex.Message}");
            }
        }

        // Método auxiliar para mapear entidade para DTO
        private TransacaoDTO MapToDto(Transacao transacao)
        {
            return new TransacaoDTO
            {
                Id = transacao.Id,
                Tipo = (int)transacao.Tipo,
                Data = transacao.Data,
                Descricao = transacao.Descricao,
                Valor = transacao.Valor,
                DataInclusao = transacao.DataInclusao,
                DataAlteracao = transacao.DataAlteracao
            };
        }
    }
} 