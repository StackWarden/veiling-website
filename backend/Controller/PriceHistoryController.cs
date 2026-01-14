using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceHistoryController : ControllerBase
{
    private readonly IConfiguration _config;

    public PriceHistoryController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? productId)
    {
        var connStr = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? _config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connStr)) return Problem("No connection string configured", statusCode: 500);

        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        // Ensure useful indexes exist for performance (safe-if-not-exists)
        try
        {
            var checkIdxCmd = new SqlCommand(@"
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_AuctionItems_SoldAtUtc')
    CREATE NONCLUSTERED INDEX IX_AuctionItems_SoldAtUtc ON AuctionItems (SoldAtUtc);
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_AuctionItems_ProductId_SoldAtUtc')
    CREATE NONCLUSTERED INDEX IX_AuctionItems_ProductId_SoldAtUtc ON AuctionItems (ProductId, SoldAtUtc);
IF NOT EXISTS (SELECT name FROM sys.indexes WHERE name = 'IX_Products_SupplierId')
    CREATE NONCLUSTERED INDEX IX_Products_SupplierId ON Products (SupplierId);
", conn);
            await checkIdxCmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // Swallow index creation errors to avoid breaking startup; indices are optimizations only.
        }

        // Helper to read average and last N prices for a supplier (or all)
        async Task<object> QueryForSupplier(Guid? supplierId)
        {
            // Average
            decimal? avg = null;

            if (supplierId.HasValue)
            {
                var avgCmd = new SqlCommand(@"
SELECT AVG(CAST(ai.SoldPrice AS FLOAT))
FROM AuctionItems ai
INNER JOIN Products p ON ai.ProductId = p.Id
WHERE ai.SoldPrice IS NOT NULL AND p.SupplierId = @supplierId;", conn);
                avgCmd.Parameters.Add(new SqlParameter("@supplierId", SqlDbType.UniqueIdentifier) { Value = supplierId.Value });
                var o = await avgCmd.ExecuteScalarAsync();
                if (o != DBNull.Value && o != null) avg = Convert.ToDecimal(o);

                var lastCmd = new SqlCommand(@"
SELECT TOP (10) ai.SoldPrice, ai.SoldAtUtc
FROM AuctionItems ai
INNER JOIN Products p ON ai.ProductId = p.Id
WHERE ai.SoldPrice IS NOT NULL AND p.SupplierId = @supplierId
ORDER BY ai.SoldAtUtc DESC;", conn);
                lastCmd.Parameters.Add(new SqlParameter("@supplierId", SqlDbType.UniqueIdentifier) { Value = supplierId.Value });

                var list = new List<object>();
                using (var r = await lastCmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        list.Add(new { price = r.GetDecimal(0), date = r.GetDateTime(1) });
                    }
                }

                return new { average = avg, last10 = list };
            }
            else
            {
                var avgCmd = new SqlCommand(@"SELECT AVG(CAST(SoldPrice AS FLOAT)) FROM AuctionItems WHERE SoldPrice IS NOT NULL;", conn);
                var o = await avgCmd.ExecuteScalarAsync();
                if (o != DBNull.Value && o != null) avg = Convert.ToDecimal(o);

                var lastCmd = new SqlCommand(@"
SELECT TOP (10) SoldPrice, SoldAtUtc
FROM AuctionItems
WHERE SoldPrice IS NOT NULL
ORDER BY SoldAtUtc DESC;", conn);

                var list = new List<object>();
                using (var r = await lastCmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        list.Add(new { price = r.GetDecimal(0), date = r.GetDateTime(1) });
                    }
                }

                return new { average = avg, last10 = list };
            }
        }

        // Determine supplier for provided productId (if any)
        Guid? supplierIdForProduct = null;
        if (productId.HasValue)
        {
            var prodCmd = new SqlCommand("SELECT SupplierId FROM Products WHERE Id = @id", conn);
            prodCmd.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = productId.Value });
            var v = await prodCmd.ExecuteScalarAsync();
            if (v != DBNull.Value && v != null) supplierIdForProduct = (Guid)v;
        }

        var resultCurrent = await QueryForSupplier(supplierIdForProduct);
        var resultAll = await QueryForSupplier(null);

        return Ok(new { currentSupplier = resultCurrent, allSuppliers = resultAll });
    }
}
