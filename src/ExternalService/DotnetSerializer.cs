using System.Text.Json;
using Polly.Caching;

namespace WeatherForecastApiWithPollyDistributedCaching.ExternalService;

public class DotnetSerializer<TResult> : ICacheItemSerializer<TResult, byte[]>
{
    private readonly JsonSerializerOptions _serializerSettings;

    /// <summary>
    /// Constructs a new <see cref="T:Polly.Caching.Serialization.Json.JsonSerializer`1" /> using the given <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
    /// </summary>
    /// <param name="serializerSettings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> to use for serialization and deserialization.</param>
    public DotnetSerializer(JsonSerializerOptions serializerSettings) => this._serializerSettings =
        serializerSettings ?? throw new ArgumentNullException(nameof(serializerSettings));

    /// <summary>
    /// Deserializes the passed json-serialization of an object, to type <typeparamref name="TResult" />, using the <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> passed when the <see cref="T:Polly.Caching.Serialization.Json.JsonSerializer`1" /> was constructed.
    /// </summary>
    /// <param name="objectToDeserialize">The object to deserialize</param>
    /// <returns>The deserialized object</returns>
    public TResult Deserialize(byte[] objectToDeserialize) =>
        JsonSerializer.Deserialize<TResult>(objectToDeserialize, this._serializerSettings);

    /// <summary>
    /// Serializes the specified object to JSON, using the <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> passed when the <see cref="T:Polly.Caching.Serialization.Json.JsonSerializer`1" /> was constructed.
    /// </summary>
    /// <param name="objectToSerialize">The object to serialize</param>
    /// <returns>The serialized object</returns>
    public byte[] Serialize(TResult objectToSerialize) =>
        JsonSerializer.SerializeToUtf8Bytes(objectToSerialize, _serializerSettings);
}