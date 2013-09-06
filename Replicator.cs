using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;
using krsclient.net.dk.nsi.batchcopy;
using krsclient.net.Exception;

namespace krsclient.net
{
    /// <summary>
    /// Håndterer replikering fra kopi register servicen
    /// </summary>
    class Replicator
    {
        private readonly SosiUtil _sosiUtil;
        private readonly StamdataReplicationClient _replicationClient;
        /// <summary>
        /// Max antal rækker der hentes i hver kald (servicen har et max på 2000 værdien skal være der eller derunder)
        /// </summary>
        private const uint MaxRecords = 500;

        public Replicator()
        {
            // TODO Get from settings
            String certPath = ConfigurationManager.AppSettings["CERTPath"];
            String certPass = ConfigurationManager.AppSettings["CERTPass"];
            _sosiUtil = new SosiUtil(certPath, certPass);
            _replicationClient = new StamdataReplicationClient("StamdataReplication");
        }

        /// <summary>
        /// Foretag et replikerings kald til kopi register servicen
        /// </summary>
        /// <param name="register">Register navn</param>
        /// <param name="dataType">DataType navn</param>
        /// <param name="offset">Offset/token at starte med</param>
        /// <returns>Hentede rækker</returns>
        public List<Record> Replicate(String register, String dataType, String offset = "")
        {
            Security sec = _sosiUtil.MakeSecurity();
            Header header = _sosiUtil.MakeHeader();
            ReplicationRequestType request = MakeReplicationRequest(register, dataType, offset);
            XmlElement response = _replicationClient.replicate(ref sec, ref header, request);
            
            return ParseServiceResponse(response);
        }

        private static ReplicationRequestType MakeReplicationRequest(String register, String dataType, String offset)
        {
            if (offset == null)
                offset = "00000000000000000000";
            return new ReplicationRequestType
            {
                register = register,
                datatype = dataType,
                maxRecords = MaxRecords,
                maxRecordsSpecified = true,
                offset = offset,
                version = 1
            };
        }

        private static List<Record> ParseServiceResponse(XmlElement response)
        {
            var atomEntries = response.GetElementsByTagName("entry", "http://www.w3.org/2005/Atom");
            var records = new List<Record>();
            foreach (XmlElement atomEntry in atomEntries)
            {
                String offsetToken = ExtractOffsetToken(atomEntry);
                SortedList<string, string> fieldAndValues = 
                    ExtractRecordFieldFromContent(ExtractRecordContent(atomEntry));
                records.Add(new Record(offsetToken, fieldAndValues));
            }
            return records;
        }

        private static SortedList<String, String> ExtractRecordFieldFromContent(XmlElement recordContent)
        {
            SortedList<String, String> fieldAndValues = new SortedList<string, string>();
            XmlNodeList childNodes = recordContent.ChildNodes;
            foreach (var childNode in childNodes)
            {
                if (childNode is XmlElement)
                {
                    XmlElement field = (XmlElement) childNode;
                    fieldAndValues.Add(field.LocalName, field.IsEmpty ? null : field.InnerText);
                }
            }
            return fieldAndValues;
        }

        private static XmlElement ExtractRecordContent(XmlElement atomEntry)
        {
            // The actual record content is placed directly below thenatom:content element
            XmlNodeList contentElementList = atomEntry.GetElementsByTagName("content", "http://www.w3.org/2005/Atom");
            if (contentElementList.Count != 1) throw new ParseErrorException("Failed to extract content from atom:content");

            return contentElementList.Item(0).FirstChild as XmlElement;
        }

        private static String ExtractOffsetToken(XmlElement atomEntry)
        {
            var idElementList = atomEntry.GetElementsByTagName("id", "http://www.w3.org/2005/Atom");
            if (idElementList.Count != 1) throw new ParseErrorException("Failed to extract offset token from atom:id");

            // atom:id is formatted like this:
            // tag:nsi.dk,2011:ddv/diseases/v1/13766544330000000001
            // the last part is the offset token
            XmlNode idNode = idElementList.Item(0);
            string idText = idNode.InnerText;
            int lastSlash = idText.LastIndexOf('/');
            return idText.Substring(lastSlash + 1);
        }
    }
}
