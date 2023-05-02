using Monads;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Data;

public class RebateDataStore
{
    public virtual Maybe<Rebate> GetRebate(string rebateIdentifier)
    {
        // Access database to retrieve account, code removed for brevity 
        return new Rebate(rebateIdentifier, IncentiveType.FixedCashAmount, 10, 10);
    }

    public virtual Rebate StoreCalculationResult(Rebate account, decimal rebateAmount)
    {
        return account with { Amount = rebateAmount };
    }
}
