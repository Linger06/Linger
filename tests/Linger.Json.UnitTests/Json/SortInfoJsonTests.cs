using Linger;

namespace Linger.Json.UnitTests.Json;

public class SortInfoJsonTests
{
    [Fact]
    public void DeserializeWithoutProperty_ShouldThrowJsonException()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SortInfo>("{}"));
    }
}
