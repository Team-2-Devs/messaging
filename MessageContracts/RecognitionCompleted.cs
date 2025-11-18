namespace Messaging.MessageContracts;

/// <summary>
/// Event published when image recognition has completed for an uploaded image.
/// Produced by svc-ai-vision-adapter, consumed by SvcAnalysisOrchestrator and others.
/// </summary>
public sealed record RecognitionCompleted(AIProviderDto Provider, MachineAggregateDto Aggregate);

public sealed record AIProviderDto(
    string Name,
    string? ApiVersion,
    IReadOnlyList<string> Featureset,
    int? MaxResults = null);

public sealed record MachineAggregateDto
{
    public string? Brand { get; init; }
    public string? Type { get; init; }
    public string? Model { get; init; }
    public double Confidence { get; init; }
    public bool IsConfident { get; init; }
    public double? TypeConfidence { get; init; }
    public string? TypeSource { get; init; }

    public string Name => string.Join(", ", new[] { Brand, Type, Model }
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Select(s => s!.Trim()));
}