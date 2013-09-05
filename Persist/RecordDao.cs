
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Data.SqlTypes;
using System.Text;
using System.Xml;
using krsclient.net.Exception;

namespace krsclient.net.Persist
{
    class RecordDao
    {
        private readonly SqlCeConnection _connection;

        public RecordDao()
        {
            string connectionString = ConfigurationManager.
                ConnectionStrings["DemoDatabaseConnectionString"].ConnectionString;
            _connection = new SqlCeConnection(connectionString);
        }

        public uint PersistRecords(Record[] records, RegisterSpecification registerSpecification)
        {
            uint updated = 0;
            _connection.Open();
            SqlCeTransaction transaction = _connection.BeginTransaction();
            try
            {
                SqlCeCommand command = _connection.CreateCommand();
                command.Connection = _connection;
                command.Transaction = transaction;

                foreach (var record in records)
                {
                    if (PersistRecordIfNeeded(record, registerSpecification, command))
                        updated++;
                }
            }
            catch (System.Exception ex)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();
            }
            return updated;
        }

        private static object ConvertToProperType(RegisterSpecification.FieldSpecification fieldSpecification, 
            String fieldValue)
        {
            if (fieldValue == null) return null;
            switch (fieldSpecification.Type)
            {
                case RegisterSpecification.FieldSpecification.FieldDataType.String:
                    return fieldValue;
                case RegisterSpecification.FieldSpecification.FieldDataType.Int:
                    return Int32.Parse(fieldValue);
                case RegisterSpecification.FieldSpecification.FieldDataType.Float:
                    return Double.Parse(fieldValue);
                case RegisterSpecification.FieldSpecification.FieldDataType.BigInt:
                    return Int64.Parse(fieldValue);
                case RegisterSpecification.FieldSpecification.FieldDataType.Boolean:
                    return "1".Equals(fieldValue) ? true : false;
                case RegisterSpecification.FieldSpecification.FieldDataType.Decimal:
                    return Decimal.Parse(fieldValue);
                case RegisterSpecification.FieldSpecification.FieldDataType.Date:
                case RegisterSpecification.FieldSpecification.FieldDataType.Datetime:
                    return XmlConvert.ToDateTime(fieldValue, XmlDateTimeSerializationMode.RoundtripKind);
            }
            throw new InvalidSpecificationException("Field specification: "+ fieldSpecification.SourceName +" maps to unknown type");
        }

        private static bool PersistRecordIfNeeded(Record record, RegisterSpecification registerSpecification,
            SqlCeCommand command)
        {
            bool exists = RecordExists(registerSpecification, record, command);
            StringBuilder sqlBuilder = new StringBuilder();
            if (exists)
            {
                sqlBuilder.Append("UPDATE " + registerSpecification.TargetTableName + " SET ");
                foreach (var fieldAndValue in record.FieldValues)
                {
                    //sqlBuilder.Append(registerSpecification.)
                }
                // TODO WHERE
            }
            else
            {
                // TODO insert record
            }
            return true;
        }

        private static bool RecordExists(RegisterSpecification registerSpecification, 
            Record record, SqlCeCommand command)
        {

            var identifierField = registerSpecification.GetIdentifierField();
            var validFromField = registerSpecification.GetValidFromField();
            if (identifierField == null || validFromField == null)
                throw new InvalidSpecificationException(registerSpecification, "ValidFrom or Identifier field is not set");

            object validFromValue = ConvertToProperType(validFromField, record.ValueOfFieldNamed(validFromField.SourceName));
            object identifier = ConvertToProperType(identifierField, record.ValueOfFieldNamed(identifierField.SourceName));
            
            command.CommandText = "SELECT COUNT(1) FROM "+registerSpecification.TargetTableName + 
                                  " WHERE " + validFromField.TargetName + "=@ValidFrom AND " + 
                                  identifierField.TargetName + "=@Identifier";
            command.Parameters.AddWithValue("@ValidFrom", validFromValue);
            command.Parameters.AddWithValue("@Identifier", identifier);
            var count = (Int32) command.ExecuteScalar();
            command.Parameters.Clear();
            return count > 0;
        }
    }
}
