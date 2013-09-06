
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Text;
using System.Xml;
using krsclient.net.Exception;

namespace krsclient.net.Persist
{
    /// <summary>
    /// Klasse der håndtere gemning af Records
    /// </summary>
    class RecordDao
    {
        private readonly SqlCeConnection _connection;

        public RecordDao()
        {
            string connectionString = ConfigurationManager.
                ConnectionStrings["DemoDatabaseConnectionString"].ConnectionString;
            _connection = new SqlCeConnection(connectionString);
        }

        /// <summary>
        /// Opretter forbindelse til database og persistere alle records.
        /// </summary>
        /// <param name="records">Rækker der ønskes persisteret</param>
        /// <param name="tableSpecification">Specifikation der beskriver hvordan de ønskede rækker skal indsættes</param>
        /// <returns>Antallet af rækker der var opdateret i databasen</returns>
        public uint PersistRecords(Record[] records, TableSpecification tableSpecification)
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
                    if (InsertOrUpdateRecord(record, tableSpecification, command))
                        updated++;
                }
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
            return updated;
        }

        /// <summary>
        /// Konverter en modtagen streng værdi til en type efter regel angivet i felt specifikationen.
        /// </summary>
        /// <param name="fieldSpecification">Felt specifikation</param>
        /// <param name="fieldValue">Felt værdi</param>
        /// <returns>Object af korrekt type</returns>
        private static Object ConvertToProperType(TableSpecification.FieldSpecification fieldSpecification, 
            String fieldValue)
        {
            if (fieldValue == null) return DBNull.Value;
            switch (fieldSpecification.Type)
            {
                case TableSpecification.FieldSpecification.FieldDataType.String:
                    return fieldValue;
                case TableSpecification.FieldSpecification.FieldDataType.Int:
                    return Int32.Parse(fieldValue);
                case TableSpecification.FieldSpecification.FieldDataType.Float:
                    return Double.Parse(fieldValue);
                case TableSpecification.FieldSpecification.FieldDataType.BigInt:
                    return Int64.Parse(fieldValue);
                case TableSpecification.FieldSpecification.FieldDataType.Boolean:
                    return "1".Equals(fieldValue) ? true : false;
                case TableSpecification.FieldSpecification.FieldDataType.Decimal:
                    return Decimal.Parse(fieldValue);
                case TableSpecification.FieldSpecification.FieldDataType.Date:
                case TableSpecification.FieldSpecification.FieldDataType.Datetime:
                    return XmlConvert.ToDateTime(fieldValue, XmlDateTimeSerializationMode.RoundtripKind);
            }
            throw new InvalidSpecificationException("Field specification: "+ fieldSpecification.SourceName +" maps to unknown type");
        }

        /// <summary>
        /// Indsæt eller opdater en enkelt række
        /// </summary>
        /// <param name="record">Række der skal indsættes eller opdateres</param>
        /// <param name="tableSpecification">Specifikation der beskriver hvordan de ønskede rækker skal indsættes</param>
        /// <param name="command">Et command object der er klar til at køre statements med</param>
        /// <returns></returns>
        private static bool InsertOrUpdateRecord(Record record, TableSpecification tableSpecification,
            SqlCeCommand command)
        {
            // Opbyg en streng afhængig af om det drejer sig om en insert eller en update.
            command.CommandText = 
                RecordExists(tableSpecification, record, command) ? 
                    BuildUpdateStatement(tableSpecification, record) : 
                    BuildInsertStatement(tableSpecification, record);

            // Det statement der er opbygget har alle parametre sat som @<SourceNavn>
            // Sæt alle værdier på statement
            foreach (KeyValuePair<string, string> keyValuePair in record.FieldValues)
            {
                TableSpecification.FieldSpecification fieldSpec = 
                    tableSpecification.GetFieldSpecificationForSourceName(keyValuePair.Key);
                // Der behøver ikke findes en fieldspec hvis man ikke ønsker at gemme feltet.
                if (fieldSpec != null)
                {
                    Object properValue = ConvertToProperType(fieldSpec, keyValuePair.Value);
                    command.Parameters.AddWithValue("@" + keyValuePair.Key, properValue);
                }
            }
            int modifiedRows = command.ExecuteNonQuery();
            command.Parameters.Clear();
            if (modifiedRows > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Byg et update statement
        /// </summary>
        /// <param name="tableSpecification"></param>
        /// <param name="record"></param>
        /// <returns>Det byggede update statement</returns>
        private static string BuildUpdateStatement(TableSpecification tableSpecification, Record record)
        {
            string targetTableName = tableSpecification.TargetTableName;

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("UPDATE ").Append(targetTableName).Append(" SET ");
            foreach (var currentField in record.FieldValues)
            {
                TableSpecification.FieldSpecification fieldSpec =
                    tableSpecification.GetFieldSpecificationForSourceName(currentField.Key);
                if (fieldSpec != null)
                {
                    String targetColumnName = fieldSpec.TargetName;
                    sqlBuilder.Append(targetColumnName).Append("=@").Append(currentField.Key);
                    sqlBuilder.Append(",");
                }
            }
            // Fjern det ekstra komma
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1);

            var validFromFieldSpec = tableSpecification.GetValidFromField();
            var idFieldSpec = tableSpecification.GetIdentifierField();
            
            // "Semi" lækker where clause opbygning...

            // TargetName er navn på kolonnen i vores database, SourceName er nøglen i input record
            sqlBuilder.Append(" WHERE ").Append(validFromFieldSpec.TargetName).Append("=@").Append(validFromFieldSpec.SourceName);
            sqlBuilder.Append(" AND ").Append(idFieldSpec.TargetName).Append("=@").Append(idFieldSpec.SourceName);

            return sqlBuilder.ToString();
        }

        /// <summary>
        /// Byg et insert statement
        /// </summary>
        /// <param name="tableSpecification"></param>
        /// <param name="record"></param>
        /// <returns>Strent med det byggede insert statement</returns>
        private static String BuildInsertStatement(TableSpecification tableSpecification, Record record)
        {
            string targetTableName = tableSpecification.TargetTableName;

            var sqlBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder();

            sqlBuilder.Append("INSERT INTO ").Append(targetTableName).Append("(");
            for (int i = 0; i < record.FieldValues.Count; i++)
            {
                String currentKey = record.FieldValues.Keys[i];
                TableSpecification.FieldSpecification fieldSpecificationForSourceName =
                    tableSpecification.GetFieldSpecificationForSourceName(currentKey);
                if (fieldSpecificationForSourceName != null)
                {
                    sqlBuilder.Append(fieldSpecificationForSourceName.TargetName).Append(",");
                    valuesBuilder.Append("@").Append(currentKey).Append(",");
                }
            }
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            valuesBuilder.Remove(valuesBuilder.Length - 1, 1);
            sqlBuilder.Append(") VALUES(").Append(valuesBuilder).Append(")");
            return sqlBuilder.ToString();
        }

        /// <summary>
        /// Undersøg om en række allerede eksistere i databasen.
        /// </summary>
        /// <param name="tableSpecification">Register specifikation</param>
        /// <param name="record">Række der skal tjekkes om findes i forvejen</param>
        /// <param name="command">Et command object der er klar til at køre statements med</param>
        /// <returns>true hvis rækken allerede findes i databasen</returns>
        private static bool RecordExists(TableSpecification tableSpecification, 
            Record record, SqlCeCommand command)
        {

            var identifierField = tableSpecification.GetIdentifierField();
            var validFromField = tableSpecification.GetValidFromField();
            if (identifierField == null || validFromField == null)
                throw new InvalidSpecificationException(tableSpecification, "ValidFrom or Identifier field is not set");

            object validFromValue = ConvertToProperType(validFromField, record.ValueOfFieldNamed(validFromField.SourceName));
            object identifier = ConvertToProperType(identifierField, record.ValueOfFieldNamed(identifierField.SourceName));
            
            command.CommandText = "SELECT COUNT(1) FROM "+tableSpecification.TargetTableName + 
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
