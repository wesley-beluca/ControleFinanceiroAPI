using AutoMapper;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.Interfaces;
using ControleFinanceiro.Application.Validations;
using ControleFinanceiro.Domain.Constants;
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
        private readonly IMapper _mapper;

        public TransacaoService(
            ITransacaoRepository transacaoRepository,
            TransacaoDTOValidator transacaoValidator,
            CreateTransacaoDTOValidator createTransacaoValidator,
            UpdateTransacaoDTOValidator updateTransacaoValidator,
            INotificationService notificationService,
            IMapper mapper)
        {
            _transacaoRepository = transacaoRepository;
            _transacaoValidator = transacaoValidator;
            _createTransacaoValidator = createTransacaoValidator;
            _updateTransacaoValidator = updateTransacaoValidator;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<TransacaoDTO> GetByIdAsync(Guid id, Guid? usuarioId = null)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            Transacao transacao;
            
            if (usuarioId.HasValue)
            {
                transacao = await _transacaoRepository.GetByIdAndUsuarioAsync(id, usuarioId.Value);
                if (transacao == null)
                {
                    _notificationService.AddNotification("Id", "Transação não encontrada ou não pertence ao usuário");
                    return null;
                }
            }
            else
            {
                transacao = await _transacaoRepository.GetByIdAsync(id);
                if (transacao == null)
                {
                    _notificationService.AddNotification("Id", "Transação não encontrada");
                    return null;
                }
            }

            return MapToDto(transacao);
        }

        public async Task<IEnumerable<TransacaoDTO>> GetAllAsync(Guid? usuarioId = null)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            IEnumerable<Transacao> transacoes;
            
            if (usuarioId.HasValue)
            {
                transacoes = await _transacaoRepository.GetAllByUsuarioAsync(usuarioId.Value);
            }
            else
            {
                transacoes = await _transacaoRepository.GetAllAsync();
            }
            
            // Otimização: usar AsParallel para mapear grandes conjuntos de dados
            if (transacoes.Count() > 100)
            {
                return _mapper.Map<IEnumerable<TransacaoDTO>>(transacoes.AsParallel().ToList());
            }
            
            return _mapper.Map<IEnumerable<TransacaoDTO>>(transacoes);
        }

        public async Task<IEnumerable<TransacaoDTO>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim, Guid? usuarioId = null)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            // Validação das datas
            if (dataInicio > dataFim)
            {
                _notificationService.AddNotification(ChavesNotificacao.DataInicio, MensagensErro.DataInicioMaiorQueFinal);
                return new List<TransacaoDTO>();
            }

            // Limitar o período de consulta para evitar sobrecarga
            if ((dataFim - dataInicio).TotalDays > 366)
            {
                _notificationService.AddNotification("Periodo", "O período de consulta não pode ser maior que 1 ano");
                return new List<TransacaoDTO>();
            }

            IEnumerable<Transacao> transacoes = await _transacaoRepository.GetByPeriodoAsync(dataInicio, dataFim, usuarioId);
            
            // Otimização: usar AsParallel para mapear grandes conjuntos de dados
            if (transacoes.Count() > 100)
            {
                return transacoes.AsParallel().Select(t => MapToDto(t)).ToList();
            }
            
            return transacoes.Select(t => MapToDto(t)).ToList();
        }

        public async Task<IEnumerable<TransacaoDTO>> GetByTipoAsync(int tipo, Guid? usuarioId = null)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            // Validação do tipo
            if (!Enum.IsDefined(typeof(TipoTransacao), tipo))
            {
                _notificationService.AddNotification(ChavesNotificacao.Tipo, MensagensErro.TipoTransacaoInvalido);
                return new List<TransacaoDTO>();
            }

            IEnumerable<Transacao> transacoes;
            
            transacoes = await _transacaoRepository.GetByTipoAsync((TipoTransacao)tipo, usuarioId);
            
            return _mapper.Map<IEnumerable<TransacaoDTO>>(transacoes);
        }

        public async Task<Guid> AddAsync(CreateTransacaoDTO transacaoDto, Guid? usuarioId = null)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            try
            {
                // Validação do DTO
                var validationResult = await _createTransacaoValidator.ValidateAsync(transacaoDto);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        _notificationService.AddNotification(error.PropertyName, error.ErrorMessage);
                    }
                    return Guid.Empty;
                }

                // Criação da entidade
                var transacao = new Transacao(
                    (TipoTransacao)transacaoDto.Tipo,
                    transacaoDto.Data,
                    transacaoDto.Descricao,
                    transacaoDto.Valor,
                    usuarioId
                );

                // Persistência
                await _transacaoRepository.AddAsync(transacao);
                return transacao.Id;
            }
            catch (Exception ex)
            {
                _notificationService.AddNotification(ChavesNotificacao.Erro, $"Erro ao criar transação: {ex.Message}");
                return Guid.Empty;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTransacaoDTO transacaoDto, Guid? usuarioId = null)
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
                return false;
            }

            // Validação do tipo de transação
            if (!Enum.IsDefined(typeof(TipoTransacao), transacaoDto.Tipo))
            {
                _notificationService.AddNotification(ChavesNotificacao.Tipo, MensagensErro.TipoTransacaoInvalido);
                return false;
            }

            try
            {
                // Verifica se a transação existe e pertence ao usuário
                Transacao transacao;
                
                if (usuarioId.HasValue)
                {
                    transacao = await _transacaoRepository.GetByIdAndUsuarioAsync(id, usuarioId.Value);
                    if (transacao == null)
                    {
                        _notificationService.AddNotification(ChavesNotificacao.Transacao, MensagensErro.TransacaoNaoPertenceUsuario);
                        return false;
                    }
                }
                else
                {
                    transacao = await _transacaoRepository.GetByIdAsync(id);
                    if (transacao == null)
                    {
                        _notificationService.AddNotification(ChavesNotificacao.Transacao, MensagensErro.TransacaoNaoEncontrada);
                        return false;
                    }
                }

                // Atualiza os dados da transação usando Notification Pattern
                if (!AtualizarDadosTransacao(transacao, transacaoDto, usuarioId))
                    return false;

                // Persistência
                await _transacaoRepository.UpdateAsync(transacao);
                return true;
            }
            catch (Exception ex)
            {
                _notificationService.AddNotification(ChavesNotificacao.Erro, $"Erro ao atualizar transação: {ex.Message}");
                return false;
            }
        }

        private bool AtualizarDadosTransacao(Transacao transacao, UpdateTransacaoDTO transacaoDto, Guid? usuarioId = null)
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
                
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid? usuarioId = null)
        {
            // Limpa notificações anteriores
            _notificationService.Clear();
            
            try
            {
                // Verifica se a transação existe e pertence ao usuário
                if (usuarioId.HasValue)
                {
                    var transacao = await _transacaoRepository.GetByIdAndUsuarioAsync(id, usuarioId.Value);
                    if (transacao == null)
                    {
                        _notificationService.AddNotification(ChavesNotificacao.Transacao, MensagensErro.TransacaoNaoPertenceUsuario);
                        return false;
                    }
                }
                else
                {
                    bool exists = await _transacaoRepository.ExistsAsync(id);
                    if (!exists)
                    {
                        _notificationService.AddNotification(ChavesNotificacao.Transacao, MensagensErro.TransacaoNaoEncontrada);
                        return false;
                    }
                }

                // Persistência (utilizando soft delete)
                await _transacaoRepository.DeleteAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _notificationService.AddNotification(ChavesNotificacao.Erro, $"Erro ao excluir transação: {ex.Message}");
                return false;
            }
        }

        // Método auxiliar para mapear entidade para DTO usando AutoMapper
        private TransacaoDTO MapToDto(Transacao transacao)
        {
            return _mapper.Map<TransacaoDTO>(transacao);
        }
    }
} 