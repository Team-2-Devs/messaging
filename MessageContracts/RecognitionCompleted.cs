namespace Messaging.MessageContracts;

/// <summary>
/// Event published when image recognition has completed for an uploaded image.
/// Produced by svc-ai-vision-adapter, consumed by SvcAnalysisOrchestrator.
/// </summary>
public sealed record RecognitionCompleted(
    string CorrelationId,
    string ObjectKey,
    bool Success,
    RecognitionResultDto? Result
);

public sealed record RecognitionResultDto(
    AIProviderDto Provider,
    InvocationMetricsDto Metrics,
    IReadOnlyList<ProviderResultDto> ProviderResults,
    IReadOnlyList<ShapedResultDto>? ShapedResults,
    MachineAggregateDto? Aggregate
);

public sealed record AIProviderDto(
    string Name,
    string? ApiVersion,
    IReadOnlyList<string> Featureset,
    int? MaxResults
);

public sealed record InvocationMetricsDto(
    int LatencyMs,
    int ImageCount,
    string? ProviderRequestId
);

public sealed record ProviderResultDto(
    ImageRefDto ImageRef,
    object? Raw
);

public sealed record ImageRefDto(
    string ObjectKey,
    string? OriginalFilename,
    string ContentType
);

public sealed record ShapedResultDto(
    ImageRefDto ImageRef,
    MachineSummaryDto Machine,
    EvidenceDto Evidence,
    IReadOnlyList<ObjectHitDto> Objects
);

public sealed record MachineSummaryDto(
    string? Type,
    string? Brand,
    string? Model,
    double Confidence,
    bool IsConfident
);

public sealed record EvidenceDto(
    string? WebBestGuess,
    string? Logo,
    string? OcrSample,
    IReadOnlyList<WebEntityHitDto>? WebEntities,
    IReadOnlyList<ObjectHitDto>? Objects,
    IReadOnlyList<LogoHitDto>? LogoCandidates
);

public sealed record WebEntityHitDto(string Description, double Score);
public sealed record ObjectHitDto(string Name, double Score);
public sealed record LogoHitDto(string Description, double Score);

public sealed record MachineAggregateDto(
    string? Brand,
    string? Type,
    string? Model,
    double Confidence,
    bool IsConfident,
    double? TypeConfidence,
    string? TypeSource
);