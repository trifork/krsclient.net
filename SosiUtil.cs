using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dk.nsi.seal.dgwstypes;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal;


namespace krsclient.net
{
    class SosiUtil
    {
        static dk.nsi.batchcopy.AssertionType MakeAssertionForSTS(X509Certificate2 certificate)
        {
            var ass = new dk.nsi.batchcopy.AssertionType
            {
                IssueInstant = new DateTime(),
                id = "IDCard",
                Version = 1.0m,
                Issuer = "krsclient.net",
                AttributeStatement = new dk.nsi.batchcopy.AttributeStatement[]
                {
                    new dk.nsi.batchcopy.AttributeStatement
                    {
                        id = dk.nsi.batchcopy.AttributeStatementID.IDCardData,
                        Attribute = new dk.nsi.batchcopy.Attribute[] 
                        {
                            new dk.nsi.batchcopy.Attribute{ Name = dk.nsi.batchcopy.AttributeName.sosiIDCardID, AttributeValue = Guid.NewGuid().ToString("D")},
                            new dk.nsi.batchcopy.Attribute{ Name = dk.nsi.batchcopy.AttributeName.sosiIDCardVersion, AttributeValue = "1.0.1"},
                            new dk.nsi.batchcopy.Attribute{ Name = dk.nsi.batchcopy.AttributeName.sosiIDCardType, AttributeValue = "system"},
                            new dk.nsi.batchcopy.Attribute{ Name = dk.nsi.batchcopy.AttributeName.sosiAuthenticationLevel, AttributeValue = "3"},
                        }
                    },
                    new dk.nsi.batchcopy.AttributeStatement
                    {
                        id = dk.nsi.batchcopy.AttributeStatementID.SystemLog,
                        Attribute = new dk.nsi.batchcopy.Attribute[] 
                        {
                            new dk.nsi.batchcopy.Attribute{ Name =  dk.nsi.batchcopy.AttributeName.medcomITSystemName, AttributeValue = "krsclient.net"},
                            new dk.nsi.batchcopy.Attribute{ Name = dk.nsi.batchcopy.AttributeName.medcomCareProviderID, AttributeValue = "25520041", NameFormat=dk.nsi.batchcopy.SubjectIdentifierType.medcomcvrnumber},
                            new dk.nsi.batchcopy.Attribute{ Name = dk.nsi.batchcopy.AttributeName.medcomCareProviderName, AttributeValue = "TRIFORK SERVICES A/S"},
                        }
                    }
                }
            };
            return ass;
        }
    }
}
