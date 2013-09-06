using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using dk.nsi.seal;
using dk.nsi.seal.dgwstypes;
using Attribute = dk.nsi.seal.dgwstypes.Attribute;

namespace krsclient.net
{
    class SosiUtil
    {
        private SealCard _idCard;
        private readonly X509Certificate2 _systemCert;

        private readonly String _issuer;
        private readonly String _sosiCareProviderName;
        private readonly String _sosiCareProviderCvr;
        private readonly String _itSystemName;

        private readonly String _stsUrl;

        public SosiUtil(String certPath, String certPassword)
        {
            _issuer = ConfigurationManager.AppSettings["IDCardIssuer"];
            _sosiCareProviderName = ConfigurationManager.AppSettings["SosiCareProviderName"];
            _sosiCareProviderCvr = ConfigurationManager.AppSettings["SosiCareProviderCvr"];
            _itSystemName = ConfigurationManager.AppSettings["ITSystemName"];
            _stsUrl = ConfigurationManager.AppSettings["STSUrl"];
            _systemCert = new X509Certificate2(certPath, certPassword);
        }

        public SealCard GetIdCard()
        {
            if (!IsIdCardValid(_idCard))
            {
                var rsc = SealCard.Create(MakeAssertionForSts(_systemCert));
                _idCard = SealUtilities.SignIn(rsc, _issuer, _stsUrl);
            }
            return _idCard;
        }

        public dk.nsi.batchcopy.Security MakeSecurity()
        {
            var assertion =
                GetIdCard().GetAssertion<dk.nsi.batchcopy.AssertionType>(typeof(Assertion).Name);
            return new dk.nsi.batchcopy.Security
            {
                id = Guid.NewGuid().ToString("D"),
                Timestamp = new dk.nsi.batchcopy.Timestamp 
                {
                    Created = FiveMinutesAgoUtc().ToLocalTime()
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


        private static bool IsIdCardValid(SealCard sc)
        {
            var fiveMinAgo = FiveMinutesAgoUtc();
            // Check if the card is created and valid for atleast five minutes.
            if (sc != null && (sc.ValidTo.CompareTo(fiveMinAgo) < 0))
                return true;
            return false;
        }

        private static DateTime FiveMinutesAgoUtc()
        {
            TimeSpan secsSpan = TimeSpan.FromSeconds(1);
            DateTime fiveMinAgo = DateTime.Now - TimeSpan.FromMinutes(5);
            long roundTics = fiveMinAgo.Ticks % secsSpan.Ticks;
            return new DateTime(fiveMinAgo.Ticks - roundTics).ToUniversalTime();
        }

        static public string EncodeTo64(X509Certificate2 certificate)
        {
            byte[] toEncodeAsBytes = certificate.GetCertHash();
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;

        }

        private Assertion MakeAssertionForSts(X509Certificate2 certificate)
        {
            var vnow = FiveMinutesAgoUtc();
            var ass = new Assertion
            {
                IssueInstant = FiveMinutesAgoUtc(),
                id = "IDCard",
                Version = 1.0m,
                Issuer = _issuer,
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
                        Value = _sosiCareProviderCvr
                    },
                    SubjectConfirmation = new SubjectConfirmation
                    {
                        SubjectConfirmationData = new SubjectConfirmationData
                        {
                            Item = new KeyInfo
                            {
                                Item = "OCESSignature"
                            }
                        }
                    }
                },
                AttributeStatement = new[]
                {
                    new AttributeStatement
                    {
                        id = AttributeStatementID.IDCardData,
                        Attribute = new[] 
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
                        Attribute = new[] 
                        {
                            new Attribute{ Name = AttributeName.medcomITSystemName, AttributeValue = _itSystemName},
                            new Attribute{ Name = AttributeName.medcomCareProviderID, 
                                AttributeValue = _sosiCareProviderCvr, NameFormatSpecified = true, 
                                NameFormat = SubjectIdentifierType.medcomcvrnumber},
                            new Attribute{ Name = AttributeName.medcomCareProviderName, AttributeValue = _sosiCareProviderName},
                        }
                    }
                }
            };
            return SealUtilities.SignAssertion(ass, certificate);
        }

    }
}
