namespace Messaging.MessageContracts;

/// <summary>
/// Event published when the analysis has started.
/// Produced by Analysis Orchestrator, consumed by AI Service.
/// </summary>
/// <param name="ObjectKey">Identifier of the analyzed image.</param>
public record AnalysisStarted(string ObjectKey);