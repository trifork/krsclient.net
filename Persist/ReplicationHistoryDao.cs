
using System;
using System.Configuration;
using System.Data.SqlServerCe;

namespace krsclient.net.Persist
{
    /// <summary>
    /// Klasse der holder styr på opdaterings historik
    /// </summary>
    class ReplicationHistoryDao
    {
        private readonly SqlCeConnection _connection;

        public ReplicationHistoryDao()
        {
            string connectionString = ConfigurationManager.
                ConnectionStrings["DemoDatabaseConnectionString"].ConnectionString;
            _connection = new SqlCeConnection(connectionString);
        }

        /// <summary>
        /// Find det nyeste opdatering token/offset fra historikken
        /// </summary>
        /// <param name="register"></param>
        /// <param name="dataType"></param>
        /// <returns>Token eller null</returns>
        public String FindLatestProgressFor(string register, string dataType)
        {
            _connection.Open();
            try
            {
                SqlCeCommand command = _connection.CreateCommand();
                command.CommandText = "SELECT TOP(1) LastToken FROM HouseKeeping WHERE " + 
                    "Register=@Register AND DataType=@DataType "+
                    "ORDER BY FinishedAt DESC";
                command.Parameters.AddWithValue("@Register", register);
                command.Parameters.AddWithValue("@DataType", dataType);
                return (String) command.ExecuteScalar();
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// Gem fremdrift i databasen
        /// </summary>
        /// <param name="register"></param>
        /// <param name="dataType"></param>
        /// <param name="lastToken">Seneste token</param>
        /// <param name="updatedRecords">Antal opdaterede rækker</param>
        public void SaveProgress(string register, string dataType, 
            string lastToken, uint updatedRecords)
        {
            _connection.Open();
            SqlCeTransaction transaction = _connection.BeginTransaction();
            try
            {
                SqlCeCommand command = _connection.CreateCommand();
                command.Connection = _connection;
                command.Transaction = transaction;

                command.CommandText = "INSERT INTO HouseKeeping(RecordsUpdated, LastToken, FinishedAt, Register, DataType) " +
                    " VALUES (@RecordsUpdated, @LastToken, @FinishedAt, @Register, @DataType)";
                command.Parameters.AddWithValue("@RecordsUpdated", updatedRecords);
                command.Parameters.AddWithValue("@LastToken", lastToken);
                command.Parameters.AddWithValue("@FinishedAt", DateTime.Now);
                command.Parameters.AddWithValue("@Register", register);
                command.Parameters.AddWithValue("@DataType", dataType);
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (System.Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();
            }
        }

    }
}
