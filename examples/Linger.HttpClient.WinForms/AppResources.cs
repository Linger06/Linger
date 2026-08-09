using System.Globalization;
using System.Resources;

namespace Linger.HttpClient.WinForms;

internal static class AppResources
{
    private static readonly ResourceManager s_resourceManager = new(
        "Linger.HttpClient.WinForms.Resources",
        typeof(AppResources).Assembly);

    public static string FormTitle => GetString(nameof(FormTitle));
    public static string LanguageLabel => GetString(nameof(LanguageLabel));
    public static string AccessTokenLabel => GetString(nameof(AccessTokenLabel));
    public static string RefreshTokenLabel => GetString(nameof(RefreshTokenLabel));
    public static string RefreshEndpointLabel => GetString(nameof(RefreshEndpointLabel));
    public static string RequestUrlLabel => GetString(nameof(RequestUrlLabel));
    public static string ShowTokens => GetString(nameof(ShowTokens));
    public static string ApplyTokensButton => GetString(nameof(ApplyTokensButton));
    public static string SendGetButton => GetString(nameof(SendGetButton));
    public static string ClearButton => GetString(nameof(ClearButton));
    public static string ResponseLabel => GetString(nameof(ResponseLabel));
    public static string ReadyStatus => GetString(nameof(ReadyStatus));
    public static string TokenSavedStatus => GetString(nameof(TokenSavedStatus));
    public static string RequestingStatus => GetString(nameof(RequestingStatus));
    public static string RequestSucceededStatus => GetString(nameof(RequestSucceededStatus));
    public static string ValidationErrorTitle => GetString(nameof(ValidationErrorTitle));
    public static string AccessTokenRequired => GetString(nameof(AccessTokenRequired));
    public static string RefreshTokenRequired => GetString(nameof(RefreshTokenRequired));
    public static string InvalidRefreshUrl => GetString(nameof(InvalidRefreshUrl));
    public static string InvalidRequestUrl => GetString(nameof(InvalidRequestUrl));
    public static string TokenSessionRequired => GetString(nameof(TokenSessionRequired));
    public static string RefreshFailed => GetString(nameof(RefreshFailed));
    public static string RequestFailed => GetString(nameof(RequestFailed));
    public static string NoStatusCode => GetString(nameof(NoStatusCode));
    public static string RequestCanceledStatus => GetString(nameof(RequestCanceledStatus));

    public static string FormatRequestFailed(string statusCode, string message)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            GetString(nameof(FormatRequestFailed)),
            statusCode,
            message);
    }

    private static string GetString(string name)
    {
        return s_resourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
    }
}
