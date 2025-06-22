using AutoMapper;
using ControleFinanceiro.Application.DTOs;
using ControleFinanceiro.Application.DTOs.Auth;
using ControleFinanceiro.Domain.Entities;

namespace ControleFinanceiro.Application.Mappings
{
    /// <summary>
    /// Perfil de mapeamento de entidades de domínio para DTOs
    /// </summary>
    public class DomainToDTOMappingProfile : Profile
    {
        /// <summary>
        /// Configura os mapeamentos entre entidades de domínio e DTOs
        /// </summary>
        public DomainToDTOMappingProfile()
        {
            // Mapeamento de Usuario para UserDTO
            CreateMap<Usuario, UserDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName));

            // Mapeamento de Transacao para TransacaoDTO
            CreateMap<Transacao, TransacaoDTO>();
        }
    }
}
