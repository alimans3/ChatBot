using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EDMChatBot.NAVClient.Dtos;
using Newtonsoft.Json;

namespace EDMChatBot.NAVClient
{
    public class NavHttpClient : INavClient
    {
        private readonly HttpClient Client;
        private readonly string CompanyId;
        
        public NavHttpClient(HttpClient client, string companyId)
        {
            Client = client;
            CompanyId = companyId;
        }

        public async Task<CustomerDto> GetCustomer(string customerName)
        {
            var response = await Client.GetAsync(
                $"api/beta/companies({CompanyId})/customers?$filter=displayName eq '{customerName}'");
            if (!response.IsSuccessStatusCode)
            {
                throw new NavClientException(response.ReasonPhrase);
            }
            var stringResponse = await response.Content.ReadAsStringAsync();
            var context = JsonConvert.DeserializeObject<CustomerContextDto>(stringResponse);
            if (context.Value.Count != 1)
            {
                throw new NavClientException(response.ReasonPhrase);
            }
            return context.Value[0];
        }

        public async Task DeleteSalesOrder(string salesOrderNumber)
        {
            var order = await GetSalesOrderByNumber(salesOrderNumber);
            var response = await Client.DeleteAsync(
                $"api/beta/companies({CompanyId})/salesOrders({order.Id})");
            if (!response.IsSuccessStatusCode)
            {
                throw new NavClientException(response.ReasonPhrase);
            }
        }


        public async Task<GetSalesOrderDto> GetSalesOrderByNumber(string Number)
        {
            var response = await Client.GetAsync($"api/beta/companies({CompanyId})/salesOrders?$filter=number eq '{Number}'");
            if (!response.IsSuccessStatusCode)
            {
                throw new NavClientException(response.ReasonPhrase);
            }

            return JsonConvert.DeserializeObject<GetSalesOrderContextDto>(await response.Content.ReadAsStringAsync())
                .Value[0];
        }
        
        public async Task<List<ItemCategoryDto>> GetItemCategories()
        {
            var response = await Client.GetAsync(
                $"api/beta/companies({CompanyId})/itemCategories");
            if (!response.IsSuccessStatusCode)
            {
                throw new NavClientException(response.ReasonPhrase);
            }
            var stringResponse = await response.Content.ReadAsStringAsync();
            var context = JsonConvert.DeserializeObject<ItemCategoryContextDto>(stringResponse);
            return context.Value;
        }

        public async Task<List<ItemDto>> GetItems(ItemCategoryDto categoryDto)
        {
            var response = await Client.GetAsync(
                $"api/beta/companies({CompanyId})/items?$filter=itemCategoryCode eq '{categoryDto.Code}'");
            if (!response.IsSuccessStatusCode)
            {
                throw new NavClientException(response.ReasonPhrase);
            }

            var stringResponse = await response.Content.ReadAsStringAsync();
            var context = JsonConvert.DeserializeObject<ItemContextDto>(stringResponse);
            return context.Value;

        }

        public async Task<GetSalesOrderDto> PostSalesOrder(PostSalesOrderDto salesOrderDto)
        {
            var response = await Client.PostAsync($"api/beta/companies({CompanyId})/salesOrders",
                new StringContent(JsonConvert.SerializeObject(salesOrderDto), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new NavClientException(response.ReasonPhrase);
            }

            return JsonConvert.DeserializeObject<GetSalesOrderDto>(await response.Content.ReadAsStringAsync());
        }

        public async Task<GetSalesOrderDto> GetSalesOrder(string id)
        {
            var response = await Client.GetAsync($"api/beta/companies({CompanyId})/salesOrders({id})");
            if (!response.IsSuccessStatusCode)
            {
                throw new NavClientException(response.ReasonPhrase);
            }
            return JsonConvert.DeserializeObject<GetSalesOrderDto>(await response.Content.ReadAsStringAsync());
        }

        public async Task<IDictionary<GetSalesOrderDto, List<GetSalesOrderLineDto>>> GetSalesOrdersOfCustomer(string customerNumber)
        {
            var dict = new Dictionary<GetSalesOrderDto, List<GetSalesOrderLineDto>>();
            var response =
                await Client.GetAsync(
                    $"api/beta/companies({CompanyId})/salesOrders?$filter=customerNumber eq '{customerNumber}'");
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var salesOrders = JsonConvert.DeserializeObject<GetSalesOrderContextDto>(jsonResponse);
            foreach (var order in salesOrders.Value)
            {
                var lines = await GetSalesOrderLines(order.Id);
                dict.Add(order,lines);
            }

            return dict;
        }
        
        public async Task<List<GetSalesOrderLineDto>> GetSalesOrderLines(string salesOrderId)
        {
            var response =
                await Client.GetAsync($"api/beta/companies({CompanyId})/salesOrders({salesOrderId})/salesOrderLines");
            if (!response.IsSuccessStatusCode)
            {
                throw new NavClientException(response.ReasonPhrase);
            }

            var content = await response.Content.ReadAsStringAsync();
            var deserializeObject = JsonConvert.DeserializeObject<GetSalesOrderLinesContextDto>(content);
            return deserializeObject.Value;
        }

        public async Task<GetSalesOrderLineDto> PostSalesOrderLine(string salesOrderId,PostSalesOrderLineDto salesOrderLineDto)
        {
            var response = await Client.PostAsync(
                $"api/beta/companies({CompanyId})/salesOrders({salesOrderId})/salesOrderLines",
                new StringContent(JsonConvert.SerializeObject(salesOrderLineDto), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new NavClientException(response.ReasonPhrase);
            }

            return JsonConvert.DeserializeObject<GetSalesOrderLineDto>(await response.Content.ReadAsStringAsync());
        }
    }
}