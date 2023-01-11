using AutoMapper;
using DevIO.Api.Controllers.Base;
using DevIO.Api.ViewModels;
using DevIO.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers
{
    [Route("api/[controller]")]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorService _fornecedorService;
        private readonly IMapper _mapper;

        public FornecedoresController(
            IFornecedorService fornecedorService, 
            IMapper mapper)
        {
            _fornecedorService = fornecedorService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FornecedorViewModel>> GetAllAsync()
        {
            return _mapper.Map<IEnumerable<FornecedorViewModel>>(await _fornecedorService.GetAllAsync());
        }
    }
}
