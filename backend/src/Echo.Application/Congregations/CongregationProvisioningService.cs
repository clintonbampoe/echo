using Echo.Application.Assets;
using Echo.Application.Attendances;
using Echo.Application.Projects;
using Echo.Application.Transactions;
using Echo.Domain.Assets;
using Echo.Domain.Attendances;
using Echo.Domain.Congregations;
using Echo.Domain.Projects;
using Echo.Domain.Transactions;
using Microsoft.Extensions.Logging;

namespace Echo.Application.Congregations;

/// <summary>
/// Populates a new congregation with its default lookup rows.
/// <br/><br/>
/// Call this inside the same transaction that creates the congregation.
/// If this fails, the whole registration should roll back — a
/// congregation with no session labels or transaction categories is not
/// a usable state.
/// <br/><br/>
/// This method only adds to the change tracker. The caller owns the
/// commit boundary; do not call SaveChanges here.
/// </summary>
public class CongregationProvisioningService(
    SessionLabelRepository sessionLabelRepository,
    TransactionCategoryRepository transactionCategoryRepository,
    ProjectCategoryRepository projectCategoryRepository,
    AssetCategoryRepository assetCategoryRepository,
    ILogger<CongregationProvisioningService> logger
)
{
    public void ProvisionDefaults(Guid congregationId)
    {
        var sessionLabels = CongregationDefaults
            .SessionLabels.Select(x => new SessionLabel
            {
                CongregationId = congregationId,
                Name = x.Name,
                Category = x.Category,
            })
            .ToList();
        sessionLabelRepository.BatchCreate(sessionLabels);
        CongregationProvisioningLog.SessionLabelsQueued(
            logger,
            congregationId,
            sessionLabels.Count
        );

        var transactionCategories = CongregationDefaults
            .TransactionCategories.Select(x => new TransactionCategory
            {
                CongregationId = congregationId,
                Name = x.Name,
                CategoryType = x.Type,
            })
            .ToList();
        transactionCategoryRepository.BatchCreate(transactionCategories);
        CongregationProvisioningLog.TransactionCategoriesQueued(
            logger,
            congregationId,
            transactionCategories.Count
        );

        var projectCategories = CongregationDefaults
            .ProjectCategoryNames.Select(name => new ProjectCategory
            {
                CongregationId = congregationId,
                Name = name,
            })
            .ToList();
        projectCategoryRepository.BatchCreate(projectCategories);
        CongregationProvisioningLog.ProjectCategoriesQueued(
            logger,
            congregationId,
            projectCategories.Count
        );

        var assetCategories = CongregationDefaults
            .AssetCategoryNames.Select(name => new AssetCategory
            {
                CongregationId = congregationId,
                Name = name,
            })
            .ToList();
        assetCategoryRepository.BatchCreate(assetCategories);
        CongregationProvisioningLog.AssetCategoriesQueued(
            logger,
            congregationId,
            assetCategories.Count
        );

        var total =
            sessionLabels.Count
            + transactionCategories.Count
            + projectCategories.Count
            + assetCategories.Count;
        CongregationProvisioningLog.Completed(logger, congregationId, total);
    }
}
