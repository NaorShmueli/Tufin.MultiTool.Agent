using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tufin.MultiAgentTool.Persistence.Entities;

namespace Tufin.MultiAgentTool.Persistence.Configurations;

internal sealed class AgentTraceEventEntityConfiguration
    : IEntityTypeConfiguration<AgentTraceEventEntity>
{
    public void Configure(
        EntityTypeBuilder<AgentTraceEventEntity> builder)
    {
        builder.ToTable("AgentTraceEvents");

        builder.HasKey(trace => trace.Id);

        builder.Property(trace => trace.EventType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(trace => trace.DecisionSummary)
            .HasMaxLength(2_000);

        builder.Property(trace => trace.ToolName)
            .HasMaxLength(200);

        builder.Property(trace => trace.ArgumentsJson)
            .HasColumnType("TEXT");

        builder.Property(trace => trace.ResultJson)
            .HasColumnType("TEXT");

        builder.Property(trace => trace.Error)
            .HasColumnType("TEXT");

        builder.HasIndex(trace => new
            {
                trace.TaskId,
                trace.Sequence
            })
            .IsUnique();

        builder.HasIndex(trace => trace.EventType);

        builder.HasIndex(trace => trace.ToolName);
    }
}