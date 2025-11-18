namespace Messaging.MessageContracts;

/// <summary>
/// Event published when the image upload has finished.
/// Produced by tu-ingestion-service, consumed by svc-ai-vision-adapter and SvcAnalysisOrchestrator.
/// </summary>
public record ImageUploaded(string UploadId, string ObjectKey, string ContentType, long Bytes, string Checksum, DateTimeOffset OccurredAt);