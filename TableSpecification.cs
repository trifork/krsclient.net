
using System;
using System.Linq;

namespace krsclient.net
{
    class TableSpecification
    {
        private readonly string _registerName;
        private readonly string _datatypeName;
        private readonly string _targetTableName;

        public string TargetTableName
        {
            get { return _targetTableName; }
        }

        public string DatatypeName
        {
            get { return _datatypeName; }
        }

        public string RegisterName
        {
            get { return _registerName; }
        }

        public FieldSpecification[] FieldSpecifications { get; set; }

        public class FieldSpecification
        {
            public enum FieldDataType
            {
                String, Datetime, Date, Int, Boolean, Float, Decimal, BigInt
            }

            public readonly string SourceName;
            public readonly string TargetName;
            public readonly FieldDataType Type;
            public readonly bool IsId;
            public readonly bool IsValidFrom;

            public FieldSpecification(string sourceName, string targetName, FieldDataType type, bool isId, bool isValidFrom)
            {
                SourceName = sourceName;
                IsId = isId;
                Type = type;
                TargetName = targetName;
                IsValidFrom = isValidFrom;
                if (IsValidFrom && IsId)
                {
                    throw new ArgumentException("Field cannot be both ValidFrom and Id at the same time");
                }
            }

            public FieldSpecification AsDate()
            {
                return new FieldSpecification(SourceName, TargetName, FieldDataType.Date, IsId, IsValidFrom);
            }

            public FieldSpecification AsDateTime()
            {
                return new FieldSpecification(SourceName, TargetName, FieldDataType.Datetime, IsId, IsValidFrom);
            }

            public FieldSpecification AsBoolean()
            {
                return new FieldSpecification(SourceName, TargetName, FieldDataType.Boolean, IsId, IsValidFrom);
            }

            public FieldSpecification AsFloat()
            {
                return new FieldSpecification(SourceName, TargetName, FieldDataType.Float, IsId, IsValidFrom);
            }

            public FieldSpecification AsDecimal()
            {
                return new FieldSpecification(SourceName, TargetName, FieldDataType.Decimal, IsId, IsValidFrom);
            }

            public FieldSpecification AsInt()
            {
                return new FieldSpecification(SourceName, TargetName, FieldDataType.Int, IsId, IsValidFrom);
            }

            public FieldSpecification AsBigInt()
            {
                return new FieldSpecification(SourceName, TargetName, FieldDataType.BigInt, IsId, IsValidFrom);
            }

            public FieldSpecification IdColumn()
            {
                return new FieldSpecification(SourceName, TargetName, Type, true, IsValidFrom);
            }

            public FieldSpecification ValidFromColumn()
            {
                return new FieldSpecification(SourceName, TargetName, Type, IsId, true);
            }
        }

        public TableSpecification(string registerName, string datatypeName, string targetTableName)
        {
            _registerName = registerName;
            _targetTableName = targetTableName;
            _datatypeName = datatypeName;
        }

        public FieldSpecification GetIdentifierField()
        {
            return FieldSpecifications.FirstOrDefault(fieldSpecification => fieldSpecification.IsId);
        }

        public FieldSpecification GetValidFromField()
        {
            return FieldSpecifications.FirstOrDefault(fieldSpecification => fieldSpecification.IsValidFrom);
        }

        public FieldSpecification GetFieldSpecificationForSourceName(String sourceName)
        {
            return
                FieldSpecifications.FirstOrDefault(
                    fieldSpecification => fieldSpecification.SourceName.Equals(sourceName));
        }

        public static TableSpecification CreateSpecification(string registerName, string datatypeName, 
            string targetTableName, FieldSpecification[] fieldSpecifications)
        {
            var spec = new TableSpecification(registerName, datatypeName, targetTableName)
                           {FieldSpecifications = fieldSpecifications};
            return spec;
        }

        public static FieldSpecification Map(string sourceFieldName, string targetFieldName)
        {
            return new FieldSpecification(sourceFieldName, targetFieldName, 
                FieldSpecification.FieldDataType.String, false, false);
        }
    }
}
