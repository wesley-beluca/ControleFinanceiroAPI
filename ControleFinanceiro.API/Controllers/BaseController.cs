using ControleFinanceiro.Domain.Entities;
using ControleFinanceiro.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ControleFinanceiro.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected readonly INotificationService _notificationService;
        protected readonly UserManager<Usuario> _userManager;

        protected BaseController(INotificationService notificationService, UserManager<Usuario> userManager = null)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtém o ID do usuário logado
        /// </summary>
        /// <returns>ID do usuário logado ou null se não estiver autenticado</returns>
        protected async Task<Guid?> ObterUsuarioIdLogadoAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return null;
                
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
                return null;
                
            return usuario.Id;
        }

        protected ActionResult RespostaPersonalizada(object resultado = null)
        {
            if (_notificationService.HasNotifications)
            {
                return BadRequest(new
                {
                    sucesso = false,
                    erros = _notificationService.Notifications.Select(n => new { n.Key, n.Message })
                });
            }

            return Ok(new
            {
                sucesso = true,
                dados = resultado
            });
        }
        
        protected ActionResult RespostaPersonalizada(object resultado, int statusCode)
        {
            if (_notificationService.HasNotifications)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    sucesso = false,
                    erros = _notificationService.Notifications.Select(n => new { n.Key, n.Message })
                });
            }

            return StatusCode(statusCode, new
            {
                sucesso = true,
                dados = resultado
            });
        }

        protected ActionResult RespostaPersonalizadaCreated(string actionName, object routeValues, object resultado = null, int statusCode = StatusCodes.Status201Created)
        {
            if (_notificationService.HasNotifications)
            {
                return BadRequest(new
                {
                    sucesso = false,
                    erros = _notificationService.Notifications.Select(n => new { n.Key, n.Message })
                });
            }

            return CreatedAtAction(actionName, routeValues, new
            {
                sucesso = true,
                dados = resultado
            });
        }
        
        /// <summary>
        /// Valida o ModelState e adiciona notificações em caso de erros
        /// </summary>
        /// <returns>True se o ModelState é válido, False caso contrário</returns>
        protected bool ValidarModelState()
        {
            if (ModelState.IsValid)
                return true;
                
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _notificationService.AddNotification("ModelState", error.ErrorMessage);
            }
            
            return false;
        }
    }
}
