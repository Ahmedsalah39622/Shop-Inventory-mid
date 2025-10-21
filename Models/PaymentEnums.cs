using System;

namespace ShopInventory.Models
{
    public enum PaymentMethod
    {
        Cash = 0,
        Card = 1,
        BankTransfer = 2,
        Installment = 3
    }

    public enum InvoiceType
    {
        Retail = 0,
        TaxInvoice = 1,
        Credit = 2
    }
}
