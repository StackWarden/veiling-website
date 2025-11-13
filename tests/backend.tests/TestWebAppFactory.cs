namespace backend.tests;

using Microsoft.AspNetCore.Mvc.Testing;

// Nodig als fixture-type voor integratie tests
public sealed class TestWebAppFactory : WebApplicationFactory<Program> { }
