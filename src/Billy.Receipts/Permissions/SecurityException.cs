using System;

namespace Billy.Receipts.Permissions
{
    public class SecurityException : Exception
    {
        public string Code { get; }

        public SecurityException(string message, string code) : base(message)
        {
            Code = code;
        }
    }
}