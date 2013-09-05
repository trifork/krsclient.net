using System;
using System.Collections.Generic;
using System.Xml;
using krsclient.net.dk.nsi.batchcopy;

namespace krsclient.net
{
    class Replicator
    {
        private readonly SosiUtil _sosiUtil;
        private readonly StamdataReplicationClient _replicationClient;
        private const uint MaxRecords = 5;

        public Replicator()
        {
            // TODO Get from settings
            _sosiUtil = new SosiUtil("Resources/FMK-KRS-TEST.p12", "Test1234");
            _replicationClient = new StamdataReplicationClient("StamdataReplicationTEST2");
        }

        public void Replicate(String register, String dataType, String offset = "")
        {
            Security sec = _sosiUtil.MakeSecurity();
            Header header = _sosiUtil.MakeHeader();
            ReplicationRequestType request = MakeReplicationRequest(register, dataType, offset);
            XmlElement response = _replicationClient.replicate(ref sec, ref header, request);
            
            // Parse the response xml
            List<Record> records = ParseServiceResponse(response);
            foreach (var record in records)
            {
                Console.WriteLine("{0}", record);
            }
        }

        private static ReplicationRequestType MakeReplicationRequest(String register, String dataType, String offset)
        {
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
