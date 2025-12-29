using Neba.Domain;

namespace Neba.Tests;

public static class StoredFileFactory
{
    public static StoredFile Create(
        string? container = null,
        string? path = null,
        string? contentType = null,
        long? sizeInBytes = null)
            => new()
            {
                Container = container ?? "container",
                Path = path ?? "file.txt",
                ContentType = contentType ?? "text/plain",
                SizeInBytes = sizeInBytes ?? 1024
            };

    public static StoredFile Bogus(int? seed = null)
        => Bogus(1, seed).Single();

    public static IReadOnlyCollection<StoredFile> Bogus(
        int count,
        int? seed = null)
    {
        Bogus.Faker<StoredFile> faker = new Bogus.Faker<StoredFile>()
            .RuleFor(f => f.Container, f => f.System.DirectoryPath())
            .RuleFor(f => f.Path, f => f.System.FileName())
            .RuleFor(f => f.ContentType, f => f.System.MimeType())
            .RuleFor(f => f.SizeInBytes, f => f.Random.Long(1, 10_000_000));

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return faker.Generate(count);
    }
}
