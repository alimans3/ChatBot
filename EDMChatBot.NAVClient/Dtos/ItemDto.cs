using System;

namespace EDMChatBot.NAVClient.Dtos
{
    public class ItemDto
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public string ItemCategoryId { get; set; }
        public string ItemCategoryCode { get; set; }
        public bool Blocked  { get; set; } 
        public string BaseUnitOfMeasureId { get; set; }
        public string Gtin { get; set; }
        public double Inventory { get; set; }
        public double UnitPrice { get; set; }
        public bool PriceIncludesTax  { get; set; } 
        public double UnitCost { get; set; }
        public string TaxGroupId { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public UnitOfMeasureDto BaseUnitOfMeasure { get; set; }
    }
}