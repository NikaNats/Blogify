using System.Text.Json;
using System.Text.Json.Serialization;
using Blogify.Domain.Abstractions;

namespace Blogify.Infrastructure.Caching;

/// <summary>
/// Serializes <see cref="Result"/> and <see cref="Result{TValue}"/> as a plain envelope
/// (<c>{ isSuccess, value, error }</c>). The domain types have non-public constructors and
/// validated invariants, so System.Text.Json cannot round-trip them on its own.
/// </summary>
internal sealed class ResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert == typeof(Result) ||
        (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>));

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
        typeToConvert == typeof(Result)
            ? new ResultConverter()
            : (JsonConverter)Activator.CreateInstance(
                typeof(ResultConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;

    private sealed record Envelope(bool IsSuccess, Error? Error);

    private sealed record Envelope<TValue>(bool IsSuccess, TValue? Value, Error? Error);

    private sealed class ResultConverter : JsonConverter<Result>
    {
        public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var envelope = JsonSerializer.Deserialize<Envelope>(ref reader, options)
                           ?? throw new JsonException("Cached result envelope was null.");

            return envelope.IsSuccess
                ? Result.Success()
                : Result.Failure(envelope.Error ?? throw new JsonException("Failed result is missing its error."));
        }

        public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, new Envelope(value.IsSuccess, value.IsFailure ? value.Error : null), options);
    }

    private sealed class ResultConverter<TValue> : JsonConverter<Result<TValue>>
    {
        public override Result<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var envelope = JsonSerializer.Deserialize<Envelope<TValue>>(ref reader, options)
                           ?? throw new JsonException("Cached result envelope was null.");

            if (!envelope.IsSuccess)
                return Result.Failure<TValue>(envelope.Error ?? throw new JsonException("Failed result is missing its error."));

            return Result.Success(envelope.Value ?? throw new JsonException("Successful result is missing its value."));
        }

        public override void Write(Utf8JsonWriter writer, Result<TValue> value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(
                writer,
                value.IsSuccess
                    ? new Envelope<TValue>(true, value.Value, null)
                    : new Envelope<TValue>(false, default, value.Error),
                options);
    }
}
