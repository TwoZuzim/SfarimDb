using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Sfarim.Proxies
{
    public interface IDbProxy
    {
        Task<Davar[]> GetDevarim(string[] devarim);
        Task<Davar[]> CreateDevarim(string[] devarim);
    }

    public class DbProxy : IDbProxy
    {
        private readonly SqlConnection Connection;
        public DbProxy(SqlConnection connection)
        {
            Connection = connection;
        }

        public async Task<Amud> GetAmud(int seferId, string amudName)
        {
            using var command = new SqlCommand($"SELECT [Id], [SeferId], [Name], [Order] FROM [Amudim] WHERE [SeferId] = @p0 AND [Name] = @p1)", Connection);
            command.Parameters.AddRange(new[]
            {
                new SqlParameter("@p0", seferId),
                new SqlParameter("@p1", amudName)
            });
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            await reader.ReadAsync().ConfigureAwait(false);
            return new Amud
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(2),
                Order = reader.GetInt32(3),
                SeferId = reader.GetInt32(1)
            };
        }

        public async Task<Davar[]> GetDevarim(string[] devarim)
        {
            var dvarim = Enumerable.Empty<Davar>();
            using var command = new SqlCommand($"SELECT [Id], [Davar] FROM [Devarim] WHERE [Davar] IN ({string.Join(',', devarim.Select((d, i) => $"@p{i}"))})", Connection);
            command.Parameters.AddRange(devarim.Select((d, i) => new SqlParameter($"@p{i}", d)).ToArray());
            using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                dvarim = dvarim.Append(new Davar
                {
                    Id = reader.GetInt32(0),
                    Word = reader.GetString(1)
                });
            }
            return dvarim.ToArray();
        }

        public async Task<Davar[]> CreateDevarim(string[] devarim)
        {
            var dbDvarim = await GetDevarim(devarim).ConfigureAwait(false);
            devarim = devarim.Except(dbDvarim.Select(d => d.Word)).ToArray();
            if (devarim.Any())
            {
                var dvarim = new List<Davar>();
                using var command = new SqlCommand($@"
                DECLARE @tempTable TABLE (Id INT, Name NVARCHAR(MAX));

                INSERT INTO [Devarim] ([Davar])
                OUTPUT INSERTED.Id, INSERTED.Davar INTO @tempTable
                VALUES ({string.Join("),(", devarim.Select((d, i) => $"@p{i}"))})

                SELECT Id, Name FROM @tempTable;
            ", Connection);
                command.Parameters.AddRange(devarim.Select((d, i) => new SqlParameter($"@p{i}", d)).ToArray());
                using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    dvarim.Add(new Davar
                    {
                        Id = reader.GetInt32(0),
                        Word = reader.GetString(1)
                    });
                }

                return dbDvarim.Concat(dvarim).ToArray();
            }

            return dbDvarim;
        }
    }
}
