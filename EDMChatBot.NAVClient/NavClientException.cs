using System;
using System.Reflection.Metadata;

namespace EDMChatBot.NAVClient
{
    public class NavClientException : Exception
    {
        public NavClientException(string message) : base(message)
        {
            
        }
    }
}