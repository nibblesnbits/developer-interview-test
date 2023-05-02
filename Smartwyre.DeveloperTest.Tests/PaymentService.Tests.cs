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
        rebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(new Rebate("test", IncentiveType.FixedCashAmount, 10, 10));
        var sut = new OldRebateService(rebateDataStore.Object, productDataStoreMock.Object);
        var result = sut.Calculate(new CalculateRebateRequest
        {
            ProductIdentifier = "test",
            RebateIdentifier = "test",
            Volume = 0
        });
        Assert.Equal(result.Success, CalculateRebateResult.Succeeded.Success);
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
        var rebate = new Rebate("test", IncentiveType.FixedCashAmount, 10, 10);
        oldRebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(rebate);
        decimal oldRebateAmount = 0m;
        oldRebateDataStore.Setup(x => x.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>())).Callback<Rebate, decimal>((_, d) => oldRebateAmount = d).Returns(rebate);

        decimal newRebateAmount = 0m;
        newRebateDataStore.Setup(x => x.GetRebate(It.IsAny<string>())).Returns(rebate);
        newRebateDataStore.Setup(x => x.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>())).Callback<Rebate, decimal>((_, d) => newRebateAmount = d).Returns(rebate);


        var oldOne = new OldRebateService(newRebateDataStore.Object, productDataStoreMock.Object);
        var sut = new RebateService(newRebateDataStore.Object, productDataStoreMock.Object);
        var request = new Types.CalculateRebateRequest
        {
            ProductIdentifier = "test",
            RebateIdentifier = "test",
            Volume = 0
        };
        var oldResult = oldOne.Calculate(request); // calls StoreCalculationResult() directly

        var newResult = sut.ProcessRebateRequest(request).Match(() => 0, r => r.Amount);
        Assert.Equal(rebate.Amount, newResult);
        Assert.Equal(oldRebateAmount, newRebateAmount);
        newRebateDataStore.Verify(x => x.StoreCalculationResult(rebate, newResult), Times.Once());
        oldRebateDataStore.Verify(x => x.StoreCalculationResult(rebate, newRebateAmount), Times.Once());

    }
}
