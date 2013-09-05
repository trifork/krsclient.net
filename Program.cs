
using System;
using System.Collections.Generic;
using System.IO;
using krsclient.net.Persist;
using krsclient.net.Specifications;

namespace krsclient.net
{
    class Program
    {
        static void Main(string[] args)
        {
            string workDir = Directory.GetCurrentDirectory();
            AppDomain.CurrentDomain.SetData("DataDirectory", workDir + "\\Data");
            var replicator = new Replicator();

            // Replicate DDV registers
            uint updatedRecords = ReplicateAndPersist(replicator, new DdvMap());
            Console.WriteLine("Successfully updated {0} ddv records", updatedRecords);
        }

        public static uint ReplicateAndPersist(Replicator replicator, IReplicationMap replicationMap)
        {
            var recordDao = new RecordDao();
            uint totalUpdatedRecords = 0;
            foreach (var registerSpecification in replicationMap.GetRegisterSpecifications())
            {
                List<Record> records;
                do
                {
                    records = replicator.Replicate(
                        registerSpecification.RegisterName, registerSpecification.DatatypeName);
                    totalUpdatedRecords += recordDao.PersistRecords(records.ToArray(), registerSpecification);
                } while (records.Count > 0);
            }
            return totalUpdatedRecords;
        }
    }
}
