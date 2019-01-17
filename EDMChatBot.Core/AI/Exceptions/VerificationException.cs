using System;

namespace EDMChatBot.Core.AI
{
    public class VerificationException : Exception
    {
        public VerificationException(string message) : base(message)
        {
            
        }
    }
}