using MediatR;
using Neba.Application.Caching;
using Neba.Application.Messaging;

namespace Neba.Application.UnitTests.Behaviors;

internal sealed class TestRequest : IRequest;

internal sealed class TestCommandRequest : ICommand<TestResponse>
{
    public int RequestValue1 { get; set; }

    public int RequestValue2 { get; set; }
}

internal sealed class TestCacheRequest : ICacheQuery<TestResponse>
{
    public string Key
        => "testKey";

    public TimeSpan? Expiration
        => TimeSpan.FromMinutes(5);
}