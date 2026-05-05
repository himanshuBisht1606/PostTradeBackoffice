namespace PostTrade.Application.Common.Exceptions;

public class DuplicateImportException : Exception
{
    public Guid ExistingBatchId { get; }

    public DuplicateImportException(Guid existingBatchId)
        : base($"File already imported. BatchId: {existingBatchId}. Delete the batch to reimport.")
    {
        ExistingBatchId = existingBatchId;
    }
}
