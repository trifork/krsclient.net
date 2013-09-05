
namespace krsclient.net.Exception
{
    class InvalidSpecificationException : System.Exception
    {
        public InvalidSpecificationException(RegisterSpecification registerSpecification, string message) : 
            base("Invalid register specification: " + 
            registerSpecification.RegisterName + ":" + registerSpecification.DatatypeName + ", message=" + message) {}
        public InvalidSpecificationException(string message) : base(message) {}
    }
}
