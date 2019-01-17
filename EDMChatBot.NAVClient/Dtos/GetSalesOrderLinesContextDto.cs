using System.Collections.Generic;

namespace EDMChatBot.NAVClient.Dtos
{
    public class GetSalesOrderLinesContextDto
    {
        public List<GetSalesOrderLineDto> Value { get; set; }
    }
}