using Monads;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;

public interface IRebateService
{
    Maybe<Rebate> ProcessRebateRequest(CalculateRebateRequest request);
}
