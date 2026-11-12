namespace SurveillanceAPI.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message){}
        
        public class NaoEncontradoException : DomainException
        {
            public NaoEncontradoException(string message) : base(message) { }
        }

        public class ConflitoException : DomainException
        {
            public ConflitoException(string message) : base(message) { }
        }
    }
}