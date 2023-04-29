namespace Smartwyre.DeveloperTest.Types;

public class CalculateRebateResult
{
    public bool Success { get; set; }

    public static readonly CalculateRebateResult Failed = new CalculateRebateResult { Success = false };
    public static readonly CalculateRebateResult Succeeded = new CalculateRebateResult { Success = true };
}
