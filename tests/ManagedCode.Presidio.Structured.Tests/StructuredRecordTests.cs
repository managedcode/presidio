using ManagedCode.Presidio.Core;
using Xunit;

namespace ManagedCode.Presidio.Structured.Tests;

public sealed class StructuredRecordTests
{
    [Fact]
    public void TryGetFieldRetrievesStoredValue()
    {
        var record = new StructuredRecord(new Dictionary<string, string>
        {
            ["customer.name"] = "Alice",
        });

        var found = record.TryGetField("customer.name", out var field);

        Assert.True(found);
        Assert.Equal("Alice", field.Value);
    }

    [Fact]
    public void UpdateFieldThrowsWhenPathMissing()
    {
        var record = new StructuredRecord(Array.Empty<KeyValuePair<string, string>>());

        Assert.Throws<KeyNotFoundException>(() => record.UpdateField("missing", "value"));
    }

    [Fact]
    public void UpdateFieldReturnsNewInstanceWithUpdatedValue()
    {
        var record = new StructuredRecord(new Dictionary<string, string>
        {
            ["customer.name"] = "Alice",
            ["customer.email"] = "alice@example.com",
        });

        var updated = record.UpdateField("customer.name", "Bob");

        Assert.Equal("Alice", record.Fields.First(f => f.Path == "customer.name").Value);
        Assert.Equal("Bob", updated.Fields.First(f => f.Path == "customer.name").Value);
    }

    [Fact]
    public void WithDetectionsAssignsRecognizerResults()
    {
        var record = new StructuredRecord(new Dictionary<string, string>
        {
            ["customer.phone"] = "555-0100",
        });

        var detection = new RecognizerResult("PHONE_NUMBER", new TextSpan(0, 8), 0.99,
            metadata: new Dictionary<string, object?> { ["path"] = "customer.phone" });

        var enriched = record.WithDetections(new[] { detection }, r => (string)r.Metadata["path"]!);

        Assert.NotNull(enriched.Fields[0].Detection);
        Assert.Equal("PHONE_NUMBER", enriched.Fields[0].Detection!.EntityType);
    }

    [Fact]
    public void WithDetectionsIgnoresMissingPaths()
    {
        var record = new StructuredRecord(new Dictionary<string, string>
        {
            ["customer.phone"] = "555-0100",
        });

        var detection = new RecognizerResult("PHONE_NUMBER", new TextSpan(0, 8), 0.99,
            metadata: new Dictionary<string, object?> { ["path"] = "customer.email" });

        var enriched = record.WithDetections(new[] { detection }, r => (string)r.Metadata["path"]!);

        Assert.Null(enriched.Fields[0].Detection);
    }

    [Fact]
    public void StructuredAnonymizationPlanBuildsMap()
    {
        var plan = StructuredAnonymizationPlan
            .CreateBuilder()
            .Add("customer.email", "mask")
            .Add("customer.phone", "hash")
            .Build();

        Assert.True(plan.TryGetOperator("customer.email", out var op));
        Assert.Equal("mask", op);
    }

    [Fact]
    public void StructuredAnonymizationPlanValidatesInput()
    {
        var builder = StructuredAnonymizationPlan.CreateBuilder();

        Assert.Throws<ArgumentException>(() => builder.Add("", "mask"));
        Assert.Throws<ArgumentException>(() => builder.Add("customer.email", ""));
    }
}
