using Npgsql;

namespace Infrastructure.Persistence.Database
{
    public class DatabaseConnections
    {
        public NpgsqlConnection MetadataConnection { get; }
        public NpgsqlConnection DataConnection { get; }
        public DatabaseConnections(NpgsqlConnection metadataConnection, NpgsqlConnection dataConnection)
        {
            this.MetadataConnection = metadataConnection;
            this.DataConnection = dataConnection;
        }
    }
}