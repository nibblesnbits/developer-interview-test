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

    private Maybe<decimal> GetRebateAmount(CalculateRebateRequest request, Rebate rebate, Product product)
    {
        if (!product.SupportedIncentives.HasFlag(rebate.Incentive))
        {
            return Maybe<decimal>.Empty;
        }
        return rebate switch
        {
            { Incentive: var i, Amount: var a } when i == IncentiveType.FixedCashAmount && rebate.Amount != 0 => rebate.Amount,
            { Incentive: var i, Percentage: var p } when i == IncentiveType.FixedRateRebate && (p != 0 || product.Price != 0 || request.Volume != 0) => product.Price * p * request.Volume,
            { Incentive: var i, Amount: var a } when i == IncentiveType.AmountPerUom && (a != 0 || request.Volume != 0) => a * request.Volume,
            _ => Maybe<decimal>.Empty
        };
    }

    public Maybe<Rebate> ProcessRebateRequest(CalculateRebateRequest request)
    {
        // get the rebate by identifier
        var rebateAmount = _rebateDataStore.GetRebate(request.RebateIdentifier)
            // if the rebate exists, get the product
            .SelectMany((rebate) => _productDataStore.GetProduct(request.ProductIdentifier),
            // if the product exists, get the rebate amount
            (rebate, product) => GetRebateAmount(request, rebate, product).Match(rebate,
            // if there is a rebate, store the calculation result and return the new rebate record
            (amount) => _rebateDataStore.StoreCalculationResult(rebate, amount)));

        return rebateAmount;
    }
}
