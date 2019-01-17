using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EDMChatBot.NAVClient.Dtos;

namespace EDMChatBot.NAVClient
{
    public interface INavClient
    {
        Task<CustomerDto> GetCustomer(string customerName);

        Task<List<ItemCategoryDto>> GetItemCategories();

        Task DeleteSalesOrder(string salesOrderNumber);

        Task<List<ItemDto>> GetItems(ItemCategoryDto categoryDto);

        Task<GetSalesOrderDto> PostSalesOrder(PostSalesOrderDto salesOrderDto);
        
        Task<GetSalesOrderDto> GetSalesOrder(string id);
        
        Task<GetSalesOrderDto> GetSalesOrderByNumber(string Number);

        Task<IDictionary<GetSalesOrderDto, List<GetSalesOrderLineDto>>> GetSalesOrdersOfCustomer(string customerId);

        Task<GetSalesOrderLineDto> PostSalesOrderLine(string salesOrderId,PostSalesOrderLineDto salesOrderLineDto);
    }
}