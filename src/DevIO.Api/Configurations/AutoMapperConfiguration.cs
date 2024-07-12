using AutoMapper;
using DevIO.Api.Dtos;
using DevIO.Business.Models;
using DevIO.Business.Models.Auth;

namespace DevIO.Api.Configurations;

public class AutoMapperConfiguration : Profile
{
    public AutoMapperConfiguration()
    {
        CreateMap<Fornecedor, FornecedorDto>().ReverseMap();
        CreateMap<Endereco, EnderecoDto>().ReverseMap();
        CreateMap<ProdutoDto, Produto>();
        CreateMap<ProdutoImagemDto, Produto>().ReverseMap();
        CreateMap<UserLogin, LoginResponseDto>();
        CreateMap<UserClaim, ClaimDto>();
        CreateMap<UserToken, UserTokenDto>();
        CreateMap<Produto, ProdutoDto>()
            .ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome));
    }
}
