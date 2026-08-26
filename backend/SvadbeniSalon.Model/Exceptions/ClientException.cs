namespace SvadbeniSalon.Model.Exceptions
{
    public class ClientException : BusinessException
    {
        public ClientException(string message) : base(message)
        {
        }

        public ClientException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
