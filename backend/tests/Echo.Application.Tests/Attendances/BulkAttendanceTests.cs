using System.ComponentModel.DataAnnotations;
using Echo.Application.Attendances;
using Echo.Application.Tests.Members;
using Echo.Domain.Members;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Echo.Application.Tests.Attendances;

[Trait("Category", "Unit")]
public class BulkAttendanceTests
{
    private static Person NewPerson(PersonKind kind = PersonKind.Member)
    {
        var p = PersonFactory.NewMemberPerson();
        p.Id = Guid.NewGuid();
        p.Kind = kind;
        return p;
    }

    private static Dictionary<Guid, Person> Directory(params Person[] people) =>
        people.ToDictionary(p => p.Id);

    [Fact]
    public void PlanBulk_CreatesForKnownPeopleNotYetPresent_InRequestOrder()
    {
        var ama = NewPerson();
        var kofi = NewPerson(PersonKind.Visitor);

        var plan = AttendanceService.PlanBulk([kofi.Id, ama.Id], Directory(ama, kofi), new HashSet<Guid>());

        Assert.Equal([kofi, ama], plan.ToCreate);
        Assert.Empty(plan.Skipped);
    }

    [Fact]
    public void PlanBulk_SkipsPeopleAlreadyPresent_WithAReason()
    {
        var ama = NewPerson();
        var kofi = NewPerson();

        var plan = AttendanceService.PlanBulk([ama.Id, kofi.Id], Directory(ama, kofi), new HashSet<Guid> { ama.Id });

        Assert.Equal([kofi], plan.ToCreate);
        var skipped = Assert.Single(plan.Skipped);
        Assert.Equal((ama.Id, BulkSkipReason.AlreadyPresent), (skipped.PersonId, skipped.Reason));
    }

    [Fact]
    public void PlanBulk_SkipsUnknownPeople_InsteadOfFailingTheWholeBatch()
    {
        var ama = NewPerson();
        var stranger = Guid.NewGuid();

        var plan = AttendanceService.PlanBulk([stranger, ama.Id], Directory(ama), new HashSet<Guid>());

        Assert.Equal([ama], plan.ToCreate);
        var skipped = Assert.Single(plan.Skipped);
        Assert.Equal((stranger, BulkSkipReason.PersonNotFound), (skipped.PersonId, skipped.Reason));
    }

    [Fact]
    public void PlanBulk_AnIdSentTwice_IsCountedOnce_AndNotReportedAsSkipped()
    {
        var ama = NewPerson();

        var plan = AttendanceService.PlanBulk([ama.Id, ama.Id, ama.Id], Directory(ama), new HashSet<Guid>());

        Assert.Equal([ama], plan.ToCreate);
        Assert.Empty(plan.Skipped);
    }

    private static bool IsValid(BulkAttendanceCreateDto dto) =>
        Validator.TryValidateObject(dto, new ValidationContext(dto), [], validateAllProperties: true);

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void TheRequest_MustNameBetweenOneAndFiveHundredPeople(int count, bool expected)
    {
        var dto = new BulkAttendanceCreateDto
        {
            SessionId = Guid.NewGuid(),
            PersonIds = Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList(),
        };

        Assert.Equal(expected, IsValid(dto));
    }

    [Fact]
    public void BulkCreateAttendance_IsMappedAsAPostOnAttendanceBulk()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddApplicationServices();
        var app = builder.Build();
        using var instrumentation = new ApplicationInstrumentation();
        app.MapAttendanceEndpoints(instrumentation);

        var bulk = ((IEndpointRouteBuilder)app)
            .DataSources.SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .SingleOrDefault(e => e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == "BulkCreateAttendance");

        Assert.NotNull(bulk);
        Assert.Equal("/attendance/bulk", bulk.RoutePattern.RawText);
        Assert.Contains("POST", bulk.Metadata.GetMetadata<IHttpMethodMetadata>()!.HttpMethods);
    }
}
