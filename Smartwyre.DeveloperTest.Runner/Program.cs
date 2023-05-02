using System;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Runner;

class Program
{
    static void Main(string[] args)
    {
        var productStore = new ProductDataStore();
        var rebateStore = new RebateDataStore();
        var service = new RebateService(rebateStore, productStore);
        var request = new Types.CalculateRebateRequest
        {
            ProductIdentifier = "test",
            RebateIdentifier = "test",
            Volume = 0
        };
        var result = service.ProcessRebateRequest(request);
        var newRebate = result.Match(() => new Rebate("A", IncentiveType.FixedCashAmount, 0, 0), (rebate) => rebate);
        Console.WriteLine($"New Rebate: {newRebate.Amount}");
        var response = result.Match(() => new HttpResult(400), (newRebate) => new HttpResult(200, newRebate.ToString()));
        Console.WriteLine($"Response: {response}");
    }

    private class HttpResult
    {
        private readonly int _statusCode;
        private readonly string _body;
        public HttpResult(int statusCode, string body = null) => (_statusCode, _body) = (statusCode, body);
    }
}
