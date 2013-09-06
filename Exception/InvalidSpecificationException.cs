
namespace krsclient.net.Exception
{
    class InvalidSpecificationException : System.Exception
    {
        public InvalidSpecificationException(TableSpecification tableSpecification, string message) : 
            base("Invalid register specification: " + 
            tableSpecification.RegisterName + ":" + tableSpecification.DatatypeName + ", message=" + message) {}
        public InvalidSpecificationException(string message) : base(message) {}
    }
}
