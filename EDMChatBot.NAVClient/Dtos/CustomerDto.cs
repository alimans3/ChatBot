using System;

namespace EDMChatBot.NAVClient.Dtos
{
    public class CustomerDto
    {
        public CustomerDto()
        {
            
        }
        
        public string Id { get; set; }
        public string Number { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
        public AddressDto Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public bool TaxLiable { get; set; }
        public string TaxAreaId { get; set; }
        public string TaxAreaDisplayName { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public string CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string PaymentTermsId { get; set; }
        public CodedNavDto PaymentTerms { get; set; }
        public string ShipmentMethodId { get; set; }
        public CodedNavDto ShipmentMethod { get; set; }
        public string PaymentMethodId { get; set; }
        public CodedNavDto PaymentMethod { get; set; }
        public string Blocked { get; set; }
        public double Balance { get; set; }
        public double OverdueAmount { get; set; }
        public double TotalSalesExcludingTax { get; set; }
        public DateTime LastModifiedDateTime { get; set; }

        public override string ToString()
        {
            return $"Id: {Id} \n" +
                   $"Number: {Number} \n" +
                   $"DisplayName: {DisplayName} \n" +
                   $"PhoneNumber: {PhoneNumber} \n";
        }
    }
}