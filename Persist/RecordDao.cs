
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Xml;
using krsclient.net.Exception;

namespace krsclient.net.Persist
{
    class RecordDao
    {
        private readonly SqlConnection _connection;

        public RecordDao()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnectionString"].ConnectionString;
            _connection = new SqlConnection(connectionString);
        }

        public uint PersistRecords(Record[] records, RegisterSpecification registerSpecification)
        {
            uint updated = 0;
            _connection.Open();
            SqlTransaction transaction = _connection.BeginTransaction();
            try
            {
                SqlCommand command = _connection.CreateCommand();
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
            switch (fieldSpecification.Type)
            {
                case RegisterSpecification.FieldSpecification.FieldDataType.String:
                    return fieldValue != null ? new SqlChars(fieldValue) : SqlChars.Null;
                case RegisterSpecification.FieldSpecification.FieldDataType.Int:
                    return fieldValue != null ? SqlInt32.Parse(fieldValue) : SqlInt32.Null;
                case RegisterSpecification.FieldSpecification.FieldDataType.Float:
                    return fieldValue != null ? SqlDouble.Parse(fieldValue) : SqlDouble.Null;
                case RegisterSpecification.FieldSpecification.FieldDataType.BigInt:
                    return fieldValue != null ? SqlInt64.Parse(fieldValue) : SqlInt64.Null;
                case RegisterSpecification.FieldSpecification.FieldDataType.Boolean:
                    return fieldValue != null ? SqlBoolean.Parse(fieldValue) : SqlBoolean.Null;
                case RegisterSpecification.FieldSpecification.FieldDataType.Decimal:
                    return fieldValue != null ? SqlDecimal.Parse(fieldValue) : SqlDecimal.Null;
                case RegisterSpecification.FieldSpecification.FieldDataType.Date:
                case RegisterSpecification.FieldSpecification.FieldDataType.Datetime:
                    if (fieldValue == null) return SqlDateTime.Null;
                    {
                        DateTime dateTime = XmlConvert.ToDateTime(fieldValue, XmlDateTimeSerializationMode.RoundtripKind);
                        return new SqlDateTime(dateTime);
                    }
            }
            throw new InvalidSpecificationException("Field specification: "+ fieldSpecification.SourceName +" maps to unknown type");
        }

        private static bool PersistRecordIfNeeded(Record record, RegisterSpecification registerSpecification,
            SqlCommand command)
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
            Int32 count = (Int32) command.ExecuteScalar();
            if (count > 0)
            {
                // TODO insert new record
            }
            else
            {
                // TODO update existing record
            }
            return true;
        }
    }
}
