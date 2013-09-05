
namespace krsclient.net.Specifications
{
    class DdvMap : IReplicationMap
    {
        private static readonly RegisterSpecification DiseasesMap = 
            RegisterSpecification.CreateSpecification("ddv", "diseases", "DdvDiseases", 
                new []
                    {
                        RegisterSpecification.Map("Id", "Id").IdColumn(),
                        RegisterSpecification.Map("ValidFrom", "ValidFrom").AsDateTime().ValidFromColumn(),
                        RegisterSpecification.Map("ValidTo", "ValidTo").AsDateTime(),
                    }
            );

        public RegisterSpecification[] GetRegisterSpecifications()
        {
            return new []
                       {
                           DiseasesMap
                       };
        }
    }
}
