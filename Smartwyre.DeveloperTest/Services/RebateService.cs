using System;
using System.Collections.Generic;
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
        var rebate = _rebateDataStore.GetRebate(request.RebateIdentifier);

        if (rebate is null)
        {
            return Maybe<decimal>.Empty;
        }

        var product = _productDataStore.GetProduct(request.ProductIdentifier);

        if (product is null)
        {
            return Maybe<decimal>.Empty;
        }

        if (product.SupportedIncentives.HasFlag(rebate.Incentive))
        {
            return Maybe<decimal>.Empty;
        }

        var rebateAmount = rebate switch
        {
            { Incentive: var i, Amount: var a } when i == IncentiveType.FixedCashAmount && rebate.Amount != 0 => rebate.Amount,
            { Incentive: var i, Percentage: var p } when i == IncentiveType.FixedRateRebate && (p != 0 || product.Price != 0 || request.Volume != 0) => product.Price * p * request.Volume,
            { Incentive: var i, Amount: var a } when i == IncentiveType.AmountPerUom && (a != 0 || request.Volume != 0) => a * request.Volume,
            _ => 0
        };

        if (rebateAmount == 0)
        {
            return Maybe<decimal>.Empty;
        }

        return rebateAmount;
    }
}
