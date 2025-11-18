namespace Messaging.Kafka;

/// <summary>
/// Centralized topic names. Topics define where the message goes.
/// </summary>
public static class Topics
{
    /// <summary>
    /// Published when an image has been uploaded.
    /// </summary>
    public const string ImageUploaded = "tu.images.uploaded";

    /// <summary>
    /// Published when an image recognition has been completed.
    /// </summary>
    public const string RecognitionCompleted = "tu.recognition.completed";
}