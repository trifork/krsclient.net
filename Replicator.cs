using System;
using System.Xml;
using dk.nsi.batchcopy;

namespace krsclient.net
{
    class Replicator
    {
        private readonly SosiUtil _sosiUtil;
        private readonly StamdataReplicationClient _replicationClient;

        public Replicator()
        {
            // TODO Get from settings
            _sosiUtil = new SosiUtil("Resources/FMK-KRS-TEST.p12", "Test1234");
            _replicationClient = new StamdataReplicationClient("StamdataReplicationTEST2");
        }

        public void Replicate()
        {
            Security sec = _sosiUtil.MakeSecurity();
            Header header = _sosiUtil.MakeHeader();
            XmlElement response = _replicationClient.replicate(ref sec, ref header, MakeReplicationRequest());
            Console.WriteLine("{0}", response);
        }

        private static ReplicationRequestType MakeReplicationRequest()
        {
            return new ReplicationRequestType
            {
                register = "ddv",
                datatype = "diseases",
                maxRecords = 1,
                maxRecordsSpecified = true,
                offset = "",
                version = 1
            };
        }
    }
}
