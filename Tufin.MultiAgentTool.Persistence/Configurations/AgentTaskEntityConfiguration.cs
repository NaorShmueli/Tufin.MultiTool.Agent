using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tufin.MultiAgentTool.Persistence.Entities;

namespace Tufin.MultiAgentTool.Persistence.Configurations;

internal sealed class AgentTaskEntityConfiguration
    : IEntityTypeConfiguration<AgentTaskEntity>
{
    public void Configure(
        EntityTypeBuilder<AgentTaskEntity> builder)
    {
        builder.ToTable("AgentTasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Id)
            .ValueGeneratedNever();

        builder.Property(task => task.Input)
            .IsRequired()
            .HasMaxLength(10_000);

        builder.Property(task => task.Model)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(task => task.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(task => task.FinalAnswer)
            .HasColumnType("TEXT");

        builder.Property(task => task.Error)
            .HasColumnType("TEXT");

        builder.HasIndex(task => task.CreatedAtUnixMs);

        builder.HasIndex(task => task.Status);

        builder
            .HasMany(task => task.TraceEvents)
            .WithOne()
            .HasForeignKey(trace => trace.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}