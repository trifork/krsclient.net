using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using dk.nsi.batchcopy;

namespace krsclient.net
{
    class Replicator
    {
        private SosiUtil sosiUtil;

        private StamdataReplicationClient replicationClient;

        public Replicator()
        {
            // TODO Get from settings
            sosiUtil = new SosiUtil("Resources/FMK-KRS-TEST.p12", "Test1234");
            replicationClient = new StamdataReplicationClient("StamdataReplicationTEST2");
        }

        public void Replicate()
        {
            Security sec = sosiUtil.MakeSecurity();
            Header header = sosiUtil.MakeHeader();
            XmlElement response = replicationClient.replicate(ref sec, ref header, MakeReplicationRequest());
            Console.WriteLine("{0}", response);
        }

        private ReplicationRequestType MakeReplicationRequest()
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
