using System.Data;
using Microsoft.Data.SqlClient;
using backend.Dtos;

namespace backend.Services
{
    public class PriceHistoryService
    {
        private readonly string _connectionString;

        public PriceHistoryService(IConfiguration config)
        {
            _connectionString =
                Environment.GetEnvironmentVariable("DB_CONNECTION")
                ?? config["DB_CONNECTION"]
                ?? throw new InvalidOperationException("Missing DB_CONNECTION environment variable");
        }

        public async Task<PriceHistoryDto> GetPriceHistory(Guid productId)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var meta = await GetProductMetaAsync(conn, productId);
            if (meta == null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            var (speciesId, supplierId) = meta.Value;

            var result = new PriceHistoryDto();

            result.AvgSupplier = await ScalarDecimalAsync(conn, @"
                SELECT AVG(CAST(ai.SoldPrice AS decimal(18,2)))
                FROM AuctionItems ai
                INNER JOIN Products p ON p.Id = ai.ProductId
                WHERE p.SpeciesId = @speciesId
                  AND p.SupplierId = @supplierId
                  AND ai.Status = @soldStatus
                  AND ai.SoldPrice IS NOT NULL;",
                new SqlParameter("@speciesId", SqlDbType.UniqueIdentifier) { Value = speciesId },
                new SqlParameter("@supplierId", SqlDbType.UniqueIdentifier) { Value = supplierId },
                new SqlParameter("@soldStatus", SqlDbType.Int) { Value = 2 }); // AuctionItemStatus.Sold = 2

            result.AvgOverall = await ScalarDecimalAsync(conn, @"
                SELECT AVG(CAST(ai.SoldPrice AS decimal(18,2)))
                FROM AuctionItems ai
                INNER JOIN Products p ON p.Id = ai.ProductId
                WHERE p.SpeciesId = @speciesId
                  AND ai.Status = @soldStatus
                  AND ai.SoldPrice IS NOT NULL;",
                new SqlParameter("@speciesId", SqlDbType.UniqueIdentifier) { Value = speciesId },
                new SqlParameter("@soldStatus", SqlDbType.Int) { Value = 2 });

            result.Last10Supplier = await ListAsync(conn, @"
                SELECT TOP 10 ai.SoldPrice, ai.SoldAtUtc
                FROM AuctionItems ai
                INNER JOIN Products p ON p.Id = ai.ProductId
                WHERE p.SpeciesId = @speciesId
                  AND p.SupplierId = @supplierId
                  AND ai.Status = @soldStatus
                  AND ai.SoldPrice IS NOT NULL
                  AND ai.SoldAtUtc IS NOT NULL
                ORDER BY ai.SoldAtUtc DESC;",
                includeSupplierId: false,
                new SqlParameter("@speciesId", SqlDbType.UniqueIdentifier) { Value = speciesId },
                new SqlParameter("@supplierId", SqlDbType.UniqueIdentifier) { Value = supplierId },
                new SqlParameter("@soldStatus", SqlDbType.Int) { Value = 2 });

            result.Last10Overall = await ListAsync(conn, @"
                SELECT TOP 10 ai.SoldPrice, ai.SoldAtUtc, p.SupplierId
                FROM AuctionItems ai
                INNER JOIN Products p ON p.Id = ai.ProductId
                WHERE p.SpeciesId = @speciesId
                  AND ai.Status = @soldStatus
                  AND ai.SoldPrice IS NOT NULL
                  AND ai.SoldAtUtc IS NOT NULL
                ORDER BY ai.SoldAtUtc DESC;",
                includeSupplierId: true,
                new SqlParameter("@speciesId", SqlDbType.UniqueIdentifier) { Value = speciesId },
                new SqlParameter("@soldStatus", SqlDbType.Int) { Value = 2 });

            return result;
        }

        private static async Task<(Guid speciesId, Guid supplierId)?> GetProductMetaAsync(SqlConnection conn, Guid productId)
        {
            await using var cmd = new SqlCommand(@"
                SELECT TOP 1 p.SpeciesId, p.SupplierId
                FROM Products p
                WHERE p.Id = @productId;", conn);

            cmd.Parameters.Add(new SqlParameter("@productId", SqlDbType.UniqueIdentifier) { Value = productId });

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            return (reader.GetGuid(0), reader.GetGuid(1));
        }

        private static async Task<decimal?> ScalarDecimalAsync(SqlConnection conn, string sql, params SqlParameter[] parameters)
        {
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);

            var obj = await cmd.ExecuteScalarAsync();
            if (obj == null || obj == DBNull.Value) return null;
            return Convert.ToDecimal(obj);
        }

        private static async Task<List<PricePointDto>> ListAsync(
            SqlConnection conn,
            string sql,
            bool includeSupplierId,
            params SqlParameter[] parameters)
        {
            var list = new List<PricePointDto>();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var price = reader.GetDecimal(0);
                var date = reader.GetDateTime(1);

                var point = new PricePointDto
                {
                    Price = price,
                    Date = date,
                    SupplierId = includeSupplierId ? reader.GetGuid(2) : null
                };

                list.Add(point);
            }

            return list;
        }
    }
}
