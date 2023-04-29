using Monads;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;


public class RebateService : IRebateService
{
    private readonly RebateDataStore _rebateDataStore;
    private readonly ProductDataStore _productDataStore;
    public RebateService(RebateDataStore rebateDataStore, ProductDataStore productDataStore)
    {
        _rebateDataStore = rebateDataStore;
        _productDataStore = productDataStore;
    }

    public Maybe<decimal> Calculate(CalculateRebateRequest request)
    {
        var rebateAmount = _rebateDataStore.GetRebate(request.RebateIdentifier)
            .SelectMany((rebate) => _productDataStore.GetProduct(request.ProductIdentifier),
                (r, p) => p.SupportedIncentives.HasFlag(r.Incentive) ? 0 : r switch
                {
                    { Incentive: var i, Amount: var a } when i == IncentiveType.FixedCashAmount && r.Amount != 0 => r.Amount,
                    { Incentive: var i, Percentage: var pc } when i == IncentiveType.FixedRateRebate && (pc != 0 || p.Price != 0 || request.Volume != 0) => p.Price * pc * request.Volume,
                    { Incentive: var i, Amount: var a } when i == IncentiveType.AmountPerUom && (a != 0 || request.Volume != 0) => a * request.Volume,
                    _ => 0
                });

        return rebateAmount;
    }
}
