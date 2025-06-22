using AutoMapper;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Mappings
{
    /// <summary>
    /// Perfil de mapeamento de DTOs para entidades de domínio
    /// </summary>
    public class DTOToDomainMappingProfile : Profile
    {
        /// <summary>
        /// Configura os mapeamentos entre DTOs e entidades de domínio
        /// </summary>
        public DTOToDomainMappingProfile()
        {
            // Mapeamento de TransacaoDTO para Transacao
            CreateMap<TransacaoDTO, Transacao>()
                .ForMember(dest => dest.Usuario, opt => opt.Ignore());
        }
    }
}
