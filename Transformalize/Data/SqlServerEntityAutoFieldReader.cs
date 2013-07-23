﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Transformalize.Model;
using Transformalize.Rhino.Etl.Core;
using Transformalize.Transforms;

namespace Transformalize.Data {
    public class SqlServerEntityAutoFieldReader : WithLoggingMixin, IEntityAutoFieldReader {
        private readonly Entity _entity;
        private readonly int _count;
        private readonly List<Field> _fields = new List<Field>();
        private readonly IDataTypeService _dataTypeService = new SqlServerDataTypeService();

        public SqlServerEntityAutoFieldReader(Entity entity, int count) {
            _entity = entity;
            _count = count;
            using (var cn = new SqlConnection(entity.InputConnection.ConnectionString)) {
                cn.Open();
                var cmd = new SqlCommand(PrepareSql(), cn);
                cmd.Parameters.Add(new SqlParameter("@Name", _entity.Name));
                cmd.Parameters.Add(new SqlParameter("@Schema", _entity.Schema));
                var reader = cmd.ExecuteReader();

                if (!reader.HasRows) return;

                while (reader.Read()) {
                    var name = reader.GetString(0);
                    var type = GetSystemType(reader.GetString(2));
                    var length = reader.GetInt32(3);
                    var fieldType = reader.GetBoolean(7) ? (_count == 0 ? FieldType.MasterKey : FieldType.PrimaryKey) : FieldType.Field;
                    var field = new Field(type, length, fieldType, true, null) {
                        Name = name,
                        Entity = _entity.Name,
                        Index = reader.GetInt32(6),
                        Schema = _entity.Schema,
                        Input = true,
                        Precision = reader.GetByte(4),
                        Scale = reader.GetInt32(5),
                        Transforms = new Transformer[0],
                        Auto = true,
                        Alias = _entity.Prefix + name
                    };
                    _fields.Add(field);
                }
            }
        }

        private string GetSystemType(string dataType) {
            var typeDefined = _dataTypeService.TypesReverse.ContainsKey(dataType);
            if (!typeDefined) {
                Warn("{0} | Transformalize hasn't mapped the SQL data type: {1} to a .NET data type.  It will default to string.", _entity.ProcessName, dataType);
            }
            return typeDefined ? _dataTypeService.TypesReverse[dataType] : "System.String";
        }

        public Dictionary<string, Field> ReadFields() {
            var fields = _fields.Where(f => f.FieldType.Equals(FieldType.Field)).ToDictionary(k => k.Alias, v => v);
            Debug("{0} | Entity auto found {0} field{2}.", _entity.ProcessName, fields.Count, fields.Count == 1 ? string.Empty : "s");
            return fields;
        }

        public Dictionary<string, Field> ReadPrimaryKey() {
            var primaryKey = _fields.Where(f => !f.FieldType.Equals(FieldType.Field)).ToDictionary(k => k.Alias, v => v);
            Debug("{0} | Entity auto found {0} primary key{2}.", _entity.ProcessName, primaryKey.Count, primaryKey.Count == 1 ? string.Empty : "s");
            if (!primaryKey.Any())
                Warn("{0} | Entity auto could not find a primary key.  You will need to define one in <fields><primaryKey> element.");
            return primaryKey;
        }

        public Dictionary<string, Field> ReadAll() {
            return _fields.ToDictionary(k => k.Alias, v => v);
        }

        private static string PrepareSql() {
            return @"
                SELECT
	                c.COLUMN_NAME,  --0
	                CAST(CASE c.IS_NULLABLE WHEN 'YES' THEN 1 ELSE 0 END AS BIT) AS IS_NULLABLE, --1
	                UPPER(c.DATA_TYPE) AS DATA_TYPE, --2
	                ISNULL(c.CHARACTER_MAXIMUM_LENGTH, 0) AS CHARACTER_MAXIMUM_LENGTH, --3
	                ISNULL(c.NUMERIC_PRECISION, 0) AS NUMERIC_PRECISION, --4
	                ISNULL(c.NUMERIC_SCALE, 0) AS NUMERIC_SCALE, --5
	                ISNULL(pk.ORDINAL_POSITION,c.ORDINAL_POSITION) AS ORDINAL_POSITION, --5
	                CAST(CASE WHEN pk.COLUMN_NAME IS NULL THEN 0 ELSE 1 END AS BIT) AS IS_PRIMARY_KEY
                FROM INFORMATION_SCHEMA.COLUMNS c
                LEFT OUTER JOIN (
	                SELECT
		                kcu.TABLE_SCHEMA,
		                kcu.TABLE_NAME,
		                kcu.COLUMN_NAME,
		                kcu.ORDINAL_POSITION
	                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
	                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON (kcu.TABLE_SCHEMA = tc.TABLE_SCHEMA AND kcu.TABLE_NAME = tc.TABLE_NAME AND kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY')
                ) pk ON (c.TABLE_SCHEMA = pk.TABLE_SCHEMA AND c.TABLE_NAME = pk.TABLE_NAME AND c.COLUMN_NAME = pk.COLUMN_NAME)
                WHERE c.TABLE_SCHEMA = @Schema
                AND c.TABLE_NAME = @Name
                ORDER BY IS_PRIMARY_KEY DESC, c.ORDINAL_POSITION
            ";
        }
    }
}