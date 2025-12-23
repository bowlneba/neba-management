using Ardalis.SmartEnum;

namespace Neba.Application.Documents;
/// <summary>
/// Represents the state of a document refresh operation.
/// Uses <see cref="Ardalis.SmartEnum.SmartEnum{T}"/> to provide named, strongly-typed values.
/// </summary>
public sealed class DocumentRefreshStatus : SmartEnum<DocumentRefreshStatus>
{
    /// <summary>
    /// The refresh is currently retrieving source data.
    /// </summary>
    public static readonly DocumentRefreshStatus Retrieving = new(nameof(Retrieving), 1);

    /// <summary>
    /// The refresh is uploading processed or transformed content.
    /// </summary>
    public static readonly DocumentRefreshStatus Uploading = new(nameof(Uploading), 2);

    /// <summary>
    /// The refresh completed successfully.
    /// </summary>
    public static readonly DocumentRefreshStatus Completed = new(nameof(Completed), 3);

    /// <summary>
    /// The refresh failed due to an error.
    /// </summary>
    public static readonly DocumentRefreshStatus Failed = new(nameof(Failed), 4);

    private DocumentRefreshStatus(string name, int value)
        : base(name, value)
    { }
}
