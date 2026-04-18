using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Infrastructure.Persistence;

namespace MotorCare.Infrastructure.Services;

/// <summary>
/// Generates order numbers in the format SRV-{yyyyMMdd}-{0001}.
/// Uses a PostgreSQL upsert on a per-tenant/day counter row so concurrent requests cannot produce the same sequence.
/// </summary>
public class OrderNumberGenerator : IOrderNumberGenerator
{
    private readonly ApplicationDbContext _context;

    public OrderNumberGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var counterDate = now.UtcDateTime.Date;
        var sequence = await GetNextSequenceAsync(tenantId, counterDate, cancellationToken);
        return $"SRV-{now:yyyyMMdd}-{sequence:D4}";
    }

    private async Task<int> GetNextSequenceAsync(string tenantId, DateTime counterDate, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO "ServiceOrderNumberCounters" ("TenantId", "CounterDate", "LastValue")
            VALUES (@tenantId, @counterDate, 1)
            ON CONFLICT ("TenantId", "CounterDate")
            DO UPDATE SET "LastValue" = "ServiceOrderNumberCounters"."LastValue" + 1
            RETURNING "LastValue";
            """;

        var connection = _context.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var currentTransaction = _context.Database.CurrentTransaction;
            if (currentTransaction is not null)
            {
                command.Transaction = currentTransaction.GetDbTransaction();
            }

            var tenantIdParameter = command.CreateParameter();
            tenantIdParameter.ParameterName = "tenantId";
            tenantIdParameter.Value = tenantId;
            command.Parameters.Add(tenantIdParameter);

            var counterDateParameter = command.CreateParameter();
            counterDateParameter.ParameterName = "counterDate";
            counterDateParameter.Value = counterDate;
            command.Parameters.Add(counterDateParameter);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return Convert.ToInt32(result);
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }
}
