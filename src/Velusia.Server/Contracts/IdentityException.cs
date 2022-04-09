namespace Velusia.Server.Contracts
{
    public class IdentityException : ApplicationException
    {
        public IdentityException()
            : base()
        {
        }

        public IdentityException(string message)
            : base(message)
        {
        }

        public IdentityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
