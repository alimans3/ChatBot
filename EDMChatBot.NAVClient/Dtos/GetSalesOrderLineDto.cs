using System;

namespace EDMChatBot.NAVClient.Dtos
{
    public class GetSalesOrderLineDto
    {
        public string DocumentId { get; set; }
        public double Sequence { get; set; }
        public string ItemId { get; set; }
        public string AccountId { get; set; }
        public string LineType { get; set; }
        public string Description { get; set; }
        public UnitOfMeasureDto UnitOfMeasure { get; set; }
        public double UnitPrice { get; set; }
        public double Quantity { get; set; }
        public double DiscountAmount { get; set; }
        public double DiscountPercent { get; set; }
        public bool DiscountAppliedBeforeTax { get; set; }
        public double AmountExcludingTax { get; set; }
        public string TaxCode { get; set; }
        public double TaxPercent { get; set; }
        public double TotalTaxAmount { get; set; }
        public double AmountIncludingTax { get; set; }
        public double InvoiceDiscountAllocation { get; set; }
        public double NetAmount { get; set; }
        public double NetTaxAmount { get; set; }
        public double NetAmountIncludingTax { get; set; }
        public DateTime ShipmentDate { get; set; }
        public double ShippedQuantity { get; set; }
        public double InvoicedQuantity { get; set; }
        public double InvoiceQuantity { get; set; }
        public double ShipQuantity { get; set; }
    }
}