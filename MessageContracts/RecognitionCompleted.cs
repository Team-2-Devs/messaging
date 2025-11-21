namespace Messaging.MessageContracts;

/// <summary>
/// Event published when image recognition has completed for an uploaded image.
/// Produced by svc-ai-vision-adapter, consumed by SvcAnalysisOrchestrator.
/// </summary>
public sealed record RecognitionCompleted(
    AIProviderDto Provider,
    MachineAggregateDto Aggregate
);

public sealed record AIProviderDto(
    string Name,
    string? ApiVersion,
    IReadOnlyList<string> Featureset,
    int? MaxResults
);

public sealed record MachineAggregateDto(
    string? Brand,
    string? Type,
    string? Model,
    double Confidence,
    bool IsConfident,
    double? TypeConfidence,
    string? TypeSource,
    string? Name
);