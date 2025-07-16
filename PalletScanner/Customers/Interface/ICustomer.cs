namespace PalletScanner.Customers.Interface
{
    public interface ICustomer
    {
        string Name { get; }
        IValidator CreateValidationSession();
    }
}