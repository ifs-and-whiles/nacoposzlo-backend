using System;

namespace Billy.CQRS
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
        {
            
        }

        public NotFoundException(string message): base(message)
        {
            
        }
    }
}