using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class TransactionService : ServiceBase<Transaction>
{
    public TransactionService(TransactionRepository repository, IMapper mapper) : base(repository, mapper)
    {
    }
}
