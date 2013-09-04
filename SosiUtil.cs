﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal;
using Microsoft.IdentityModel.Tokens.Saml2;

namespace krsclient.net
{
    using System.Net;
    using dk.nsi.seal.dgwstypes;

    class SosiUtil
    {
        private SealCard idCard;
        private X509Certificate2 systemCert;

        public SosiUtil(String certPath, String certPassword)
        {
            systemCert = new X509Certificate2(certPath, certPassword);
        }

        public SealCard GetIdCard()
        {
            if (!IsIdCardValid(idCard))
            {
                var rsc = SealCard.Create(MakeAssertionForSTS(systemCert));
                // TODO Get URL from properties
                idCard = SealUtilities.SignIn(rsc, "TRIFORK SERVICES A/S", 
                    "http://test1-cnsp.ekstern-test.nspop.dk:8080/sts/services/SecurityTokenService");
            }
            return idCard;
        }

        public dk.nsi.batchcopy.Security MakeSecurity()
        {
            dk.nsi.batchcopy.AssertionType assertion =
                GetIdCard().GetAssertion<dk.nsi.batchcopy.AssertionType>(typeof(Assertion).Name);
            return new dk.nsi.batchcopy.Security
            {
                id = Guid.NewGuid().ToString("D"),
                Timestamp = new dk.nsi.batchcopy.Timestamp 
                {
                    Created = DateTime.UtcNow 
                },
                Assertion = assertion
            };
        }

        public dk.nsi.batchcopy.Header MakeHeader()
        {
            return new dk.nsi.batchcopy.Header
            {
                SecurityLevel = 3,
                SecurityLevelSpecified = true,
                Linking = new dk.nsi.batchcopy.Linking
                {
                    MessageID = Guid.NewGuid().ToString("D")
                },
                RequireNonRepudiationReceipt = dk.nsi.batchcopy.RequireNonRepudiationReceipt.no,
                RequireNonRepudiationReceiptSpecified = true,
            };
        }


        private bool IsIdCardValid(SealCard sc)
        {
            var fiveMinAgo = FiveMinutesAgoUtc();
            // Check if the card is created and valid for atleast five minutes.
            if (sc != null && (sc.ValidTo.CompareTo(fiveMinAgo) < 0))
                return true;
            return false;
        }

        private DateTime FiveMinutesAgoUtc()
        {
            TimeSpan secsSpan = TimeSpan.FromSeconds(1);
            DateTime fiveMinAgo = DateTime.Now - TimeSpan.FromMinutes(5);
            long roundTics = fiveMinAgo.Ticks % secsSpan.Ticks;
            return new DateTime(fiveMinAgo.Ticks - roundTics).ToUniversalTime();
        }

        static public string EncodeTo64(X509Certificate2 certificate)
        {
            byte[] toEncodeAsBytes = certificate.GetCertHash();
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;

        }

        private Assertion MakeAssertionForSTS(X509Certificate2 certificate)
        {
            var vnow = FiveMinutesAgoUtc();
            var ass = new Assertion
            {
                IssueInstant = FiveMinutesAgoUtc(),
                id = "IDCard",
                Version = 1.0m,
                Issuer = "krsclient.net",
                Conditions = new Conditions
                {
                    NotBefore = vnow,
                    NotOnOrAfter = vnow + TimeSpan.FromHours(8)
                },
                Subject = new Subject
                {
                    NameID = new NameID
                    {
                        Format = SubjectIdentifierType.medcomcvrnumber,
                        Value = "25520041"
                    },
                    SubjectConfirmation = new SubjectConfirmation
                    {
                        SubjectConfirmationData = new SubjectConfirmationData
                        {
                            Item = new dk.nsi.seal.dgwstypes.KeyInfo
                            {
                                Item = "OCESSignature"
                            }
                        }
                    }
                },
                AttributeStatement = new dk.nsi.seal.dgwstypes.AttributeStatement[]
                {
                    new dk.nsi.seal.dgwstypes.AttributeStatement
                    {
                        id = AttributeStatementID.IDCardData,
                        Attribute = new Attribute[] 
                        {
                            new Attribute{ Name = AttributeName.sosiIDCardID, AttributeValue = Guid.NewGuid().ToString("D")},
                            new Attribute{ Name = AttributeName.sosiIDCardVersion, AttributeValue = "1.0.1"},
                            new Attribute{ Name = AttributeName.sosiIDCardType, AttributeValue = "system"},
                            new Attribute{ Name = AttributeName.sosiAuthenticationLevel, AttributeValue = "3"},
                            new Attribute{ Name = AttributeName.sosiOCESCertHash, AttributeValue = EncodeTo64(certificate)}
                        }
                    },
                    new AttributeStatement
                    {
                        id = AttributeStatementID.SystemLog,
                        Attribute = new Attribute[] 
                        {
                            new Attribute{ Name = AttributeName.medcomITSystemName, AttributeValue = "krsclient.net"},
                            new Attribute{ Name = AttributeName.medcomCareProviderID, 
                                AttributeValue = "25520041", NameFormatSpecified = true, NameFormat = SubjectIdentifierType.medcomcvrnumber},
                            new Attribute{ Name = AttributeName.medcomCareProviderName, AttributeValue = "TRIFORK SERVICES A/S"},
                        }
                    }
                }
            };
            return SealUtilities.SignAssertion(ass, certificate);
        }

    }
}
