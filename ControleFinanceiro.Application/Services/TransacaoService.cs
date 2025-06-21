using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using ControleFinanceiro.Domain.Interfaces.Repositories;
using ControleFinanceiro.Domain.Notifications;
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
        private readonly INotificationService _notificationService;

        public TransacaoService(
            ITransacaoRepository transacaoRepository,
            TransacaoDTOValidator transacaoValidator,
            CreateTransacaoDTOValidator createTransacaoValidator,
            UpdateTransacaoDTOValidator updateTransacaoValidator,
            INotificationService notificationService)
        {
            _transacaoRepository = transacaoRepository;
            _transacaoValidator = transacaoValidator;
            _createTransacaoValidator = createTransacaoValidator;
            _updateTransacaoValidator = updateTransacaoValidator;
            _notificationService = notificationService;
        }

        public async Task<Result<TransacaoDTO>> GetByIdAsync(Guid id, Guid? usuarioId = null)
        {
            Transacao transacao;
            
            if (usuarioId.HasValue)
            {
                transacao = await _transacaoRepository.GetByIdAndUsuarioAsync(id, usuarioId.Value);
                if (transacao == null)
                    return Result<TransacaoDTO>.Fail("Transação não encontrada ou não pertence ao usuário");
            }
            else
            {
                transacao = await _transacaoRepository.GetByIdAsync(id);
                if (transacao == null)
                    return Result<TransacaoDTO>.Fail("Transação não encontrada");
            }

            TransacaoDTO transacaoDto = MapToDto(transacao);
            return Result<TransacaoDTO>.Ok(transacaoDto);
        }

        public async Task<Result<IEnumerable<TransacaoDTO>>> GetAllAsync(Guid? usuarioId = null)
        {
            IEnumerable<Transacao> transacoes;
            
            if (usuarioId.HasValue)
            {
                transacoes = await _transacaoRepository.GetAllByUsuarioAsync(usuarioId.Value);
            }
            else
            {
                transacoes = await _transacaoRepository.GetAllAsync();
            }
            
            IEnumerable<TransacaoDTO> transacoesDto = transacoes.Select(t => MapToDto(t));

            return Result<IEnumerable<TransacaoDTO>>.Ok(transacoesDto);
        }

        public async Task<Result<IEnumerable<TransacaoDTO>>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null)
        {
            // Validação das datas
            if (dataInicio > dataFim)
                return Result<IEnumerable<TransacaoDTO>>.Fail("A data inicial não pode ser maior que a data final");

            // Limitar o período de consulta para evitar sobrecarga
            if ((dataFim - dataInicio).TotalDays > 366)
                return Result<IEnumerable<TransacaoDTO>>.Fail("O período de consulta não pode ser maior que 1 ano");

            IEnumerable<Transacao> transacoes = await _transacaoRepository.GetByPeriodoAsync(dataInicio, dataFim, usuarioId);
            
            IEnumerable<TransacaoDTO> transacoesDto = transacoes.Select(t => MapToDto(t));

            return Result<IEnumerable<TransacaoDTO>>.Ok(transacoesDto);
        }

        public async Task<Result<IEnumerable<TransacaoDTO>>> GetByTipoAsync(int tipo, Guid? usuarioId = null)
        {
            if (!Enum.IsDefined(typeof(TipoTransacao), tipo))
                return Result<IEnumerable<TransacaoDTO>>.Fail("Tipo de transação inválido. Use 0 para Despesa ou 1 para Receita");

            IEnumerable<Transacao> transacoes = await _transacaoRepository.GetByTipoAsync((TipoTransacao)tipo, usuarioId);
            
            IEnumerable<TransacaoDTO> transacoesDto = transacoes.Select(t => MapToDto(t));

            return Result<IEnumerable<TransacaoDTO>>.Ok(transacoesDto);
        }

        public async Task<Result<Guid>> AddAsync(CreateTransacaoDTO transacaoDto, Guid? usuarioId = null)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            // Validação do DTO
            var validationResult = await _createTransacaoValidator.ValidateAsync(transacaoDto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    _notificationService.AddNotification(error.PropertyName, error.ErrorMessage);
                }
                return Result<Guid>.Fail("Erros de validação encontrados");
            }

            // Validação do tipo de transação
            if (!Enum.IsDefined(typeof(TipoTransacao), transacaoDto.Tipo))
            {
                _notificationService.AddNotification("Tipo", "Tipo de transação inválido. Use 0 para Despesa ou 1 para Receita");
                return Result<Guid>.Fail("Tipo de transação inválido");
            }

            try
            {
                // Criação da entidade com Notification Pattern
                var notification = new Notification();
                var transacao = new Transacao(
                    (TipoTransacao)transacaoDto.Tipo,
                    transacaoDto.Data,
                    transacaoDto.Descricao,
                    transacaoDto.Valor,
                    usuarioId,
                    notification
                );
                
                // Verifica se há erros de validação na entidade
                if (!notification.IsValid)
                {
                    // Transfere as notificações da entidade para o serviço de notificação
                    foreach (var item in notification.Notifications)
                    {
                        _notificationService.AddNotification(item.Key, item.Message);
                    }
                    return Result<Guid>.Fail("Erros de validação na entidade");
                }

                // Persistência
                await _transacaoRepository.AddAsync(transacao);
                return Result<Guid>.Ok(transacao.Id, "Transação criada com sucesso");
            }
            catch (ArgumentException ex)
            {
                _notificationService.AddNotification("Erro", ex.Message);
                return Result<Guid>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                _notificationService.AddNotification("Erro", $"Erro ao criar transação: {ex.Message}");
                return Result<Guid>.Fail($"Erro ao criar transação: {ex.Message}");
            }
        }

        public async Task<Result<bool>> UpdateAsync(Guid id, UpdateTransacaoDTO transacaoDto, Guid? usuarioId = null)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            // Validação do DTO
            var validationResult = await _updateTransacaoValidator.ValidateAsync(transacaoDto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    _notificationService.AddNotification(error.PropertyName, error.ErrorMessage);
                }
                return Result<bool>.Fail("Erros de validação encontrados");
            }

            // Validação do tipo de transação
            if (!Enum.IsDefined(typeof(TipoTransacao), transacaoDto.Tipo))
            {
                _notificationService.AddNotification("Tipo", "Tipo de transação inválido. Use 0 para Despesa ou 1 para Receita");
                return Result<bool>.Fail("Tipo de transação inválido");
            }

            try
            {
                // Verifica se a transação existe
                bool exists = await _transacaoRepository.ExistsAsync(id);
                if (!exists)
                {
                    _notificationService.AddNotification("Id", $"Transação com ID {id} não encontrada");
                    return Result<bool>.Fail($"Transação com ID {id} não encontrada");
                }

                // Busca a entidade
                Transacao transacao = await _transacaoRepository.GetByIdAsync(id);
                if (transacao == null)
                {
                    _notificationService.AddNotification("Id", $"Transação com ID {id} não encontrada");
                    return Result<bool>.Fail($"Transação com ID {id} não encontrada");
                }

                // Atualiza os dados da transação usando Notification Pattern
                Result<bool> resultadoAtualizacao = AtualizarDadosTransacao(transacao, transacaoDto, usuarioId);
                if (!resultadoAtualizacao.Success)
                    return resultadoAtualizacao;

                // Persistência
                await _transacaoRepository.UpdateAsync(transacao);
                return Result<bool>.Ok(true, "Transação atualizada com sucesso");
            }
            catch (Exception ex)
            {
                _notificationService.AddNotification("Erro", $"Erro ao atualizar transação: {ex.Message}");
                return Result<bool>.Fail($"Erro ao atualizar transação: {ex.Message}");
            }
        }

        private Result<bool> AtualizarDadosTransacao(Transacao transacao, UpdateTransacaoDTO transacaoDto, Guid? usuarioId = null)
        {
            // Usa o Notification Pattern para coletar erros
            var notification = new Notification();

            // Atualiza os dados usando o Notification Pattern
            transacao.SetTipo((TipoTransacao)transacaoDto.Tipo, notification);
            transacao.SetData(transacaoDto.Data, notification);
            transacao.SetDescricao(transacaoDto.Descricao, notification);
            transacao.SetValor(transacaoDto.Valor, notification);
            transacao.SetUsuario(usuarioId);

            // Verifica se há erros de validação
            if (!notification.IsValid)
            {
                // Transfere as notificações da entidade para o serviço de notificação
                foreach (var item in notification.Notifications)
                {
                    _notificationService.AddNotification(item.Key, item.Message);
                }
                
                // Retorna falha com a lista de erros
                var errorMessages = notification.Notifications.Select(e => e.Message).ToList();
                return Result<bool>.Fail(errorMessages);
            }

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> DeleteAsync(Guid id)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            try
            {
                // Verifica se a transação existe
                bool exists = await _transacaoRepository.ExistsAsync(id);
                if (!exists)
                {
                    _notificationService.AddNotification("Id", $"Transação com ID {id} não encontrada");
                    return Result<bool>.Fail($"Transação com ID {id} não encontrada");
                }

                // Persistência (utilizando soft delete)
                await _transacaoRepository.DeleteAsync(id);
                return Result<bool>.Ok(true, "Transação excluída com sucesso");
            }
            catch (Exception ex)
            {
                _notificationService.AddNotification("Erro", $"Erro ao excluir transação: {ex.Message}");
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
                DataAlteracao = transacao.DataAlteracao,
                UsuarioId = transacao.UsuarioId
            };
        }
    }
} 