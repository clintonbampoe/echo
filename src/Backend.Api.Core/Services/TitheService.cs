using AutoMapper;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories;
using Backend.Api.Core.Services.Base;

namespace Backend.Api.Core.Services;

public class TitheService(TitheRepository repository, IMapper mapper)
    : ServiceBase<Tithe>(repository, mapper) { }
