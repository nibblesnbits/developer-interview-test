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

    private decimal GetRebateAmount(CalculateRebateRequest request, Rebate rebate, Product product)
    {
        if (!product.SupportedIncentives.HasFlag(rebate.Incentive))
        {
            return 0;
        }
        return rebate switch
        {
            { Incentive: var i, Amount: var a } when i == IncentiveType.FixedCashAmount && rebate.Amount != 0 => rebate.Amount,
            { Incentive: var i, Percentage: var p } when i == IncentiveType.FixedRateRebate && (p != 0 || product.Price != 0 || request.Volume != 0) => product.Price * p * request.Volume,
            { Incentive: var i, Amount: var a } when i == IncentiveType.AmountPerUom && (a != 0 || request.Volume != 0) => a * request.Volume,
            _ => 0
        };
    }

    public Maybe<decimal> Calculate(CalculateRebateRequest request)
    {
        var rebateAmount = _rebateDataStore.GetRebate(request.RebateIdentifier)
            .SelectMany((rebate) => _productDataStore.GetProduct(request.ProductIdentifier),
                (r, p) => GetRebateAmount(request, r, p));

        return rebateAmount;
    }
}
