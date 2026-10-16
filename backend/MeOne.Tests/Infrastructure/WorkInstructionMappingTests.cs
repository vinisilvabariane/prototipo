using MeOne.Domain.Entities;
using MeOne.Infrastructure.Data.Context;
using MeOne.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Tests.Infrastructure;

public sealed class WorkInstructionMappingTests
{
    private static MeOneDbContext Context() =>
        new(new DbContextOptionsBuilder<MeOneDbContext>()
            .UseMySql("Server=localhost;Database=meone;User=x;Password=x", new MySqlServerVersion(new Version(8, 0, 0)))
            .Options);

    [Fact]
    public void WorkInstructions_UseSnakeCaseColumns()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(WorkInstruction))!;
        var columns = entity.GetProperties().ToDictionary(p => p.Name, p => p.GetColumnName());

        Assert.Equal("work_instructions", entity.GetTableName());
        Assert.Equal("document_number", columns[nameof(WorkInstruction.DocumentNumber)]);
        Assert.Equal("prepared_by", columns[nameof(WorkInstruction.PreparedBy)]);
        Assert.Equal("approved_by", columns[nameof(WorkInstruction.ApprovedBy)]);
        Assert.Equal("required_ppe", columns[nameof(WorkInstruction.RequiredPpe)]);
        Assert.Equal("process_specifications", columns[nameof(WorkInstruction.ProcessSpecifications)]);
        Assert.Equal("pdf_stored_name", columns[nameof(WorkInstruction.PdfStoredName)]);
        Assert.Equal("fk_created_by", columns[nameof(WorkInstruction.CreatedBy)]);
    }

    [Fact]
    public void Steps_UseSnakeCaseColumnsAndTheCodeIsUnique()
    {
        using var context = Context();
        var steps = context.Model.FindEntityType(typeof(WorkInstructionStep))!;
        var columns = steps.GetProperties().ToDictionary(p => p.Name, p => p.GetColumnName());

        Assert.Equal("work_instruction_steps", steps.GetTableName());
        Assert.Equal("fk_instruction", columns[nameof(WorkInstructionStep.WorkInstructionId)]);
        Assert.Equal("image_stored_name", columns[nameof(WorkInstructionStep.ImageStoredName)]);
        Assert.DoesNotContain(nameof(WorkInstructionStep.HasImage), columns.Keys);

        var instruction = context.Model.FindEntityType(typeof(WorkInstruction))!;
        Assert.True(instruction.GetIndexes().Single(index => index.Properties.Single().Name == nameof(WorkInstruction.Code)).IsUnique);
    }

    [Fact]
    public void SearchQuery_TranslatesToSql()
    {
        using var context = Context();

        var sql = WorkInstructionRepository.ApplySearch(context.Set<WorkInstruction>(), "solda").ToQueryString();

        Assert.Contains("LIKE", sql);
        Assert.Contains("work_instructions", sql);
    }
}
