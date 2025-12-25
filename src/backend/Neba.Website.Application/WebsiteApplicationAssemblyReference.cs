using System.Reflection;

namespace Neba.Website.Application;

/// <summary>
/// Serves as a marker for the Website Application assembly, enabling the framework
/// or other application layers to identify and access its components. This static reference
/// is commonly used for service registration and dependency injection configuration.
/// </summary>
public static class WebsiteApplicationAssemblyReference
{
    /// <summary>
    /// Provides a reference to the assembly containing the Website Application layer.
    /// This variable can be used to identify and load types, resources, and other
    /// elements defined within the assembly. It is often used for purposes such as
    /// dependency injection, service registration, and runtime type discovery.
    /// </summary>
    public static readonly Assembly Assembly = typeof(WebsiteApplicationAssemblyReference).Assembly;
}

