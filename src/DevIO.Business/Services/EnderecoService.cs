using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repository;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Models;
using DevIO.Business.Models.Validations;
using DevIO.Business.Services.Base;

namespace DevIO.Business.Services
{
    public class EnderecoService : BaseService, IEnderecoService
    {
        private readonly IEnderecoRepository _enderecoRepository;

        public EnderecoService(
            IEnderecoRepository enderecoRepository, 
            INotifier notifier) : base(notifier)
        {
            _enderecoRepository = enderecoRepository;
        }

        public async Task<Endereco> GetByIdAsync(Guid id)
        {
            return await _enderecoRepository.GetByIdAsync(id);
        }

        public async Task UpdateAsync(Endereco endereco)
        {
            if (!Validate(new EnderecoValidator(), endereco)) return;

            await _enderecoRepository.UpdateAsync(endereco);
        }

        public void Dispose()
        {
            _enderecoRepository?.Dispose();
        }
    }
}
