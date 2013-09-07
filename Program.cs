
using System;
using System.Collections.Generic;
using krsclient.net.Persist;
using krsclient.net.Specifications;

namespace krsclient.net
{
    class Program
    {
        static void Main(string[] args)
        {
            var replicator = new Replicator();

            // Replicate DDV registers
            uint updatedRecords = ReplicateAndPersist(replicator, new DdvMap());
            Console.WriteLine("Successfully updated {0} ddv records", updatedRecords);
        }

        /// <summary>
        /// Repliker alle tabeller angivet i et IReplicationMap
        /// </summary>
        /// <param name="replicator"></param>
        /// <param name="replicationMap"></param>
        /// <returns></returns>
        public static uint ReplicateAndPersist(Replicator replicator, IReplicationMap replicationMap)
        {
            var recordDao = new RecordDao();
            var historyDao = new ReplicationHistoryDao();
            uint totalUpdatedRecords = 0;
            foreach (var registerSpecification in replicationMap.GetTableSpecifications())
            {
                totalUpdatedRecords += ReplicateAndPersistSpecification(
                    replicator, registerSpecification, recordDao, historyDao);
            }
            Console.Write("\r\n");
            return totalUpdatedRecords;
        }

        /// <summary>
        /// Repliker en enkelt tabel
        /// </summary>
        /// <param name="replicator"></param>
        /// <param name="tableSpecification">Tabel specifikationen</param>
        /// <param name="recordDao"></param>
        /// <param name="historyDao"></param>
        /// <returns>Antallet af opdaterede og indsatte rækker</returns>
        private static uint ReplicateAndPersistSpecification(Replicator replicator, 
            TableSpecification tableSpecification, RecordDao recordDao, 
            ReplicationHistoryDao historyDao)
        {
            uint totalUpdatedRecords = 0;
            List<Record> records;
            
            string registerName = tableSpecification.RegisterName;
            string datatypeName = tableSpecification.DatatypeName;

            String lastToken = historyDao.FindLatestProgressFor(registerName, datatypeName);
            do
            {
                records = replicator.Replicate(registerName, datatypeName, lastToken);
                if (records.Count > 0)
                {
                    uint recordsPersisted = recordDao.PersistRecords(records.ToArray(), tableSpecification);
                    totalUpdatedRecords += recordsPersisted;
                    lastToken = records[records.Count - 1].OffsetToken;
                    // Save our progress
                    if (lastToken != null)
                        historyDao.SaveProgress(registerName, datatypeName, lastToken, recordsPersisted);
                }
                Console.Write(".");
            } while (records.Count > 0);
            return totalUpdatedRecords;
        }
    }
}
