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
        public class NaoAutorizadoException : DomainException
        { public NaoAutorizadoException(string m) : base(m) { } }

        public class ProibidoException : DomainException
        { public ProibidoException(string m) : base(m) { } }
    }
}