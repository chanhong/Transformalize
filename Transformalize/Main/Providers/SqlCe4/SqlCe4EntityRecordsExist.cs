namespace Transformalize.Main.Providers.SqlCe4
{
    public class SqlCe4EntityRecordsExist : IEntityRecordsExist {
        public IEntityExists EntityExists { get; set; }


        public SqlCe4EntityRecordsExist() {
            EntityExists = new SqlCe4EntityExists();
        }

        public bool RecordsExist(AbstractConnection connection, Entity entity) {

            if (EntityExists.Exists(connection, entity)) {

                using (var cn = connection.GetConnection()) {
                    cn.Open();
                    var sql = string.Format(@"SELECT [{0}] FROM [{1}];", entity.PrimaryKey.First().Alias, entity.OutputName());
                    var cmd = cn.CreateCommand();
                    cmd.CommandText = sql;
                    using (var reader = cmd.ExecuteReader()) {
                        return reader.Read();
                    }
                }
            }
            return false;
        }
    }
}