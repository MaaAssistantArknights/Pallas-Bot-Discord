using System.Text.Json;
using System.Text.Json.Serialization;

namespace PallasBot.Application.Common.Models.GitHub;

public enum OAuthDeviceCodeFlowError
{
    AuthorizationPending,
    SlowDown,
    ExpiredToken,
    UnsupportedGrantType,
    IncorrectClientCredentials,
    IncorrectDeviceCode,
    AccessDenied,
    InvalidScope,

    Unknown
}

public class OAuthDeviceCodeFlowErrorJsonConverter : JsonConverter<OAuthDeviceCodeFlowError>
{
    public override OAuthDeviceCodeFlowError Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str switch
        {
            "authorization_pending" => OAuthDeviceCodeFlowError.AuthorizationPending,
            "slow_down" => OAuthDeviceCodeFlowError.SlowDown,
            "expired_token" => OAuthDeviceCodeFlowError.ExpiredToken,
            "unsupported_grant_type" => OAuthDeviceCodeFlowError.UnsupportedGrantType,
            "incorrect_client_credentials" => OAuthDeviceCodeFlowError.IncorrectClientCredentials,
            "incorrect_device_code" => OAuthDeviceCodeFlowError.IncorrectDeviceCode,
            "access_denied" => OAuthDeviceCodeFlowError.AccessDenied,
            "invalid_scope" => OAuthDeviceCodeFlowError.InvalidScope,
            _ => OAuthDeviceCodeFlowError.Unknown
        };
    }

    public override void Write(Utf8JsonWriter writer, OAuthDeviceCodeFlowError value, JsonSerializerOptions options)
    {
        var str = value switch
        {
            OAuthDeviceCodeFlowError.AuthorizationPending => "authorization_pending",
            OAuthDeviceCodeFlowError.SlowDown => "slow_down",
            OAuthDeviceCodeFlowError.ExpiredToken => "expired_token",
            OAuthDeviceCodeFlowError.UnsupportedGrantType => "unsupported_grant_type",
            OAuthDeviceCodeFlowError.IncorrectClientCredentials => "incorrect_client_credentials",
            OAuthDeviceCodeFlowError.IncorrectDeviceCode => "incorrect_device_code",
            OAuthDeviceCodeFlowError.AccessDenied => "access_denied",
            OAuthDeviceCodeFlowError.InvalidScope => "invalid_scope",
            _ => "unknown"
        };

        writer.WriteStringValue(str);
    }
}
