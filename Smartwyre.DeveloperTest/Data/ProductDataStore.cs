using Monads;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Data;

public class ProductDataStore
{
    public Maybe<Product> GetProduct(string productIdentifier)
    {
        // Access database to retrieve account, code removed for brevity 
        return new Product();
    }
}
