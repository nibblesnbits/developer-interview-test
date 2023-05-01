using System;
using Monads;
using Moq;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class PaymentServiceTests
{
    [Fact]
    public void OldRebateService_Returns_A_Result()
    {
        var productDataStoreMock = new Mock<ProductDataStore>();
        var rebateDataStore = new Mock<RebateDataStore>();

        productDataStoreMock.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(new Product
        {
            SupportedIncentives = IncentiveType.FixedCashAmount
        });
        rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate
        {
            Amount = 10,
            Incentive = IncentiveType.FixedCashAmount,
            Percentage = 10
        });
        var sut = new OldRebateService(rebateDataStore.Object, productDataStoreMock.Object);
        var result = sut.Calculate(new CalculateRebateRequest
        {
            ProductIdentifier = "test",
            RebateIdentifier = "test",
            Volume = 0
        });
        Assert.Equal(result.Success, CalculateRebateResult.Failed.Success);
    }
    [Fact]
    public void RebateService_Returns_The_Same_Result_As_OldRebateService()
    {
        var productDataStoreMock = new Mock<ProductDataStore>();
        var newRebateDataStore = new Mock<RebateDataStore>();
        var oldRebateDataStore = new Mock<RebateDataStore>();

        var product = new Product
        {
            Identifier = "test",
            SupportedIncentives = IncentiveType.FixedCashAmount
        };
        productDataStoreMock.Setup(x => x.GetProduct(It.IsAny<string>())).Returns(product);
        var rebate = new Rebate
        {
            Identifier = "test",
            Amount = 10,
            Incentive = IncentiveType.FixedCashAmount,
            Percentage = 10
        };

        newRebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(rebate);
        oldRebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(rebate);

        decimal rebateAmount = 0m;
        newRebateDataStore.Setup(x => x.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>())).Callback<Rebate, decimal>((_, d) => rebateAmount = d);


        var oldOne = new OldRebateService(newRebateDataStore.Object, productDataStoreMock.Object);
        var sut = new RebateService(newRebateDataStore.Object, productDataStoreMock.Object);
        var request = new Types.CalculateRebateRequest
        {
            ProductIdentifier = "test",
            RebateIdentifier = "test",
            Volume = 0
        };
        var oldResult = oldOne.Calculate(request); // calls StoreCalculationResult() directly

        var newResult = sut.Calculate(request).Match(() => Try.Create<Rebate>(() => new Exception()), amount =>
            Try.Create<Rebate>(() => newRebateDataStore.Object.StoreCalculationResult(rebate, amount))).Match(r => r.Amount, e => 0m);

        newRebateDataStore.Verify(x => x.StoreCalculationResult(rebate, newResult), Times.Once());
        oldRebateDataStore.Verify(x => x.StoreCalculationResult(rebate, rebateAmount), Times.Once());

    }
}
