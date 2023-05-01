using Monads;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Data;

public class RebateDataStore
{
    public virtual Maybe<Rebate> GetRebate(string rebateIdentifier)
    {
        // Access database to retrieve account, code removed for brevity 
        return new Rebate();
    }

    public virtual Rebate StoreCalculationResult(Rebate account, decimal rebateAmount)
    {
        return new Rebate
        {
            Amount = rebateAmount,
            Identifier = account.Identifier,
            Incentive = account.Incentive,
            Percentage = account.Percentage
        };
    }
}
