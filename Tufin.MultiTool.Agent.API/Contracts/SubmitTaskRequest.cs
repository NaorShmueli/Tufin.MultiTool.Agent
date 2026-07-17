using System.ComponentModel.DataAnnotations;

namespace Tufin.MultiTool.Agent.API.Contracts;

public sealed class SubmitTaskRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(10_000)]
    public string Task { get; init; } = string.Empty;
}