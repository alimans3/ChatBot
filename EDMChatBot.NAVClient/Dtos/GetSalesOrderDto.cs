using System;

namespace EDMChatBot.NAVClient.Dtos
{
    public class GetSalesOrderDto
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public  DateTime OrderDate { get; set; }
        public string CustomerId { get; set; }
        public string ContactId { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public AddressDto BillingPostalAddress { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public bool PricesIncludeTax { get; set; }
        public string PaymentTermsId { get; set; }
        public string PaymentTerms { get; set; }
        public string Salesperson { get; set; }
        public bool PartialShipping { get; set; }
        public DateTime RequestedDeliveryDate { get; set; }
        public double DiscountAmount { get; set; }
        public bool DiscountAppliedBeforeTax { get; set; }
        public double TotalAmountExcludingTax { get; set; }
        public double TotalTaxAmount { get; set; }
        public double TotalAmountIncludingTax { get; set; }
        public bool FullyShipped { get; set; }
        public string Status { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
    }
}