namespace Billy.Domain
{
    public class InvalidValueException : DomainException
    {
        public InvalidValueException(string message, string errorCode)
            : base(message, errorCode)
        {

        }
    }
}