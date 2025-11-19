namespace Messaging.MessageContracts;

/// <summary>
/// Event published when the analysis has finished (success or failure).
/// Produced by Analysis Orchestrator, consumed by the Graph Gateway,
/// where GraphQL subscriptions broadcast updates to clients
/// </summary>
public record AnalysisCompleted(bool Success, RecognitionCompletedPayload? RecognitionPayload);