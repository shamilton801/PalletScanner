using PalletScanner.CustomerInterface;
using PalletScanner.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletScanner.Customers.Tyson
{
    public class Tyson : ICustomer
    {
        public string Name => "Tyson";
        public IValidator CreateValidationSession() => new TysonValidator();
    }

    public class TysonValidator : IValidator
    {
        public IList<Status> Status => [];

        public void AddBarcodeRead(BarcodeRead raw)
        {
            // TODO
        }
    }
}