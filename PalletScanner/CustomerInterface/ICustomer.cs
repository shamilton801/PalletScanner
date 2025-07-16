namespace PalletScanner.CustomerInterface
{
    public interface ICustomer
    {
        string Name { get; }
        IValidator CreateValidationSession();
    }
}