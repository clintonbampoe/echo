using AutoMapper;
using Backend.Api.Core.Data;
using Backend.Api.Core.Entities;
using Backend.Api.Core.Repositories.Base;
using Backend.Api.Core.Repositories.Engines.Interfaces;

namespace Backend.Api.Core.Repositories;

public class EventAttendanceRepository : RelationshipRepositoryBase<EventAttendance>
{
    public EventAttendanceRepository(AppDbContext context, IMapper mapper, IDatabaseEngine<EventAttendance> databaseEngine) : base(context, mapper, databaseEngine)
    {
    }
}
