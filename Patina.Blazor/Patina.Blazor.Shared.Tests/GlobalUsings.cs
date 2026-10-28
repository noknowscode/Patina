global using FluentAssertions;
global using NUnit.Framework;
global using Patina.Blazor.Shared.Services;
global using Patina.Core;
global using Patina.Core.Storage;
global using Bunit;
global using Microsoft.Extensions.DependencyInjection;
global using Patina.Blazor.Shared.Pages;
global using Patina.Core.Model;
global using Patina.Core.Queries;
global using Patina.Core.Rules;

// One fixture instance per test: TestHost owns a bUnit context and a data folder, neither of which may be shared.
[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
