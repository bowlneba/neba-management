using Bunit;
using TestContext = Bunit.TestContext;

namespace Neba.WebTests;

/// <summary>
/// Base class for bUnit tests that handles test context creation and disposal.
/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete - TestContext will be replaced when bUnit updates
public abstract class TestContextWrapper : IDisposable
{
    private readonly Lazy<Bunit.TestContext> _testContext = new Lazy<Bunit.TestContext>(() =>
    {
        TestContext context = new Bunit.TestContext();

        // Setup JS interop to handle module imports
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        return context;
    });
    private bool _disposed;

    protected Bunit.TestContext TestContext => _testContext.Value;

    /// <summary>
    /// Renders a component. This uses the new Render API which wraps the builder action.
    /// </summary>
    protected IRenderedComponent<TComponent> Render<TComponent>(Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null)
        where TComponent : Microsoft.AspNetCore.Components.IComponent
    {
        // The new Render method takes the same Action signature
        return TestContext.Render<TComponent>(parameterBuilder);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _testContext.IsValueCreated)
            {
                _testContext.Value?.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
