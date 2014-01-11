
using System;
using System.Configuration;
using System.Data.Common;

namespace krsclient.net.Persist
{
    /// <summary>
    /// Klasse der holder styr på opdaterings historik
    /// </summary>
    class ReplicationHistoryDao
    {
        private readonly DbConnection _connection;

        public ReplicationHistoryDao(DbConnection connection)
        {
            _connection = connection;
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
                DbCommand command = _connection.CreateCommand();
                command.CommandText = "SELECT TOP(1) LastToken FROM HouseKeeping WHERE " + 
                    "Register=@Register AND DataType=@DataType "+
                    "ORDER BY FinishedAt DESC";
                AddParamWithValue(command, "@Register", register);
                AddParamWithValue(command, "@DataType", dataType);
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
            DbTransaction transaction = _connection.BeginTransaction();
            try
            {
                DbCommand command = _connection.CreateCommand();
                command.Connection = _connection;
                command.Transaction = transaction;

                command.CommandText = "INSERT INTO HouseKeeping(RecordsUpdated, LastToken, FinishedAt, Register, DataType) " +
                    " VALUES (@RecordsUpdated, @LastToken, @FinishedAt, @Register, @DataType)";
                AddParamWithValue(command, "@RecordsUpdated", updatedRecords);
                AddParamWithValue(command, "@LastToken", lastToken);
                AddParamWithValue(command, "@FinishedAt", DateTime.Now);
                AddParamWithValue(command, "@Register", register);
                AddParamWithValue(command, "@DataType", dataType);
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

        private void AddParamWithValue(DbCommand command, string name, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }

    }
}
