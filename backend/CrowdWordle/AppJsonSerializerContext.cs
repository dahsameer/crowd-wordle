using System.Text.Json.Serialization;

namespace CrowdWordle;

[JsonSerializable(typeof(Status))]
[JsonSerializable(typeof(VerifyRequest))]
public partial class AppJsonSerializerContext : JsonSerializerContext
{
}
