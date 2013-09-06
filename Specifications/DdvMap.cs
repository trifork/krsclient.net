
namespace krsclient.net.Specifications
{
    class DdvMap : IReplicationMap
    {
        private static readonly TableSpecification DiseasesMap = 
            TableSpecification.CreateSpecification("ddv", "diseases", "DdvDiseases", 
                new []
                    {
                        TableSpecification.Map("Id", "Id").IdColumn(),
                        TableSpecification.Map("DiseaseIdentifier", "DiseaseIdentifier").AsBigInt(),
                        TableSpecification.Map("versionID", "VersionID").AsBigInt(),
                        TableSpecification.Map("name", "Name"),
                        TableSpecification.Map("name_dk", "NameDK"),
                        TableSpecification.Map("ATCCode", "ATCCode"),
                        TableSpecification.Map("ATCText", "ATCText"),
                        TableSpecification.Map("ddvModifiedDate", "DdvModifiedDate").AsDateTime(),
                        TableSpecification.Map("ddvValidFrom", "DdvValidFrom").AsDateTime(),
                        TableSpecification.Map("ddvValidTo", "DdvValidTo").AsDateTime(),
                        TableSpecification.Map("ValidFrom", "ValidFrom").AsDateTime().ValidFromColumn(),
                        TableSpecification.Map("ValidTo", "ValidTo").AsDateTime()
                    }
            );

        private static readonly TableSpecification VaccinesMap =
            TableSpecification.CreateSpecification("ddv", "vaccines", "DdvVaccines",
                new []
                    {
                        TableSpecification.Map("Id", "Id").IdColumn(),
                        TableSpecification.Map("VaccineIdentifier", "VaccineIdentifier").AsBigInt(),
                        TableSpecification.Map("VersionID", "VersionID").AsBigInt(),
                        TableSpecification.Map("ATCCode", "ATCCode"),
                        TableSpecification.Map("ATCText", "ATCText"),
                        TableSpecification.Map("ShortDescription", "ShortDescription"),
                        TableSpecification.Map("AllowCitizenSelfRegister", "AllowCitizenSelfRegister").AsBoolean(),
                        TableSpecification.Map("AllowBulkRegister", "AllowBulkRegister").AsBoolean(),
                        TableSpecification.Map("Keywords", "Keywords"),
                        TableSpecification.Map("SearchBoost", "SearchBoost").AsDecimal(),
                        TableSpecification.Map("ddvModifiedDate", "DdvModifiedDate").AsDateTime(),
                        TableSpecification.Map("ddvValidFrom", "DdvValidFrom").AsDateTime(),
                        TableSpecification.Map("ddvValidTo", "DdvValidTo").AsDateTime(),
                        TableSpecification.Map("ValidFrom", "ValidFrom").AsDateTime().ValidFromColumn(),
                        TableSpecification.Map("ValidTo", "ValidTo").AsDateTime()
                    }
            );

        public TableSpecification[] GetTableSpecifications()
        {
            return new []
                       {
                           DiseasesMap,
                           VaccinesMap
                       };
        }
    }
}
