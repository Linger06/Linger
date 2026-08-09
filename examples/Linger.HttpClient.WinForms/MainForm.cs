using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Linger.AspNetCore.Jwt.Contracts;
using Linger.HttpClient.Contracts.Extensions;
using Linger.HttpClient.Standard;
using Microsoft.IdentityModel.Tokens;

namespace Linger.HttpClient.WinForms;

internal sealed partial class MainForm : Form
{
    private static readonly CultureInfo[] s_supportedCultures =
    [
        CultureInfo.GetCultureInfo("en-US"),
        CultureInfo.GetCultureInfo("zh-CN")
    ];

    private static readonly JsonSerializerOptions s_displayJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly CancellationTokenSource _lifetimeSource = new();
    private readonly System.Net.Http.HttpClient _refreshHttpClient;
    private readonly System.Net.Http.HttpClient _apiHttpClient;
    private readonly JwtTokenSession _tokenSession;
    private readonly StandardHttpClient _apiClient;

    public MainForm()
    {
        _refreshHttpClient = new System.Net.Http.HttpClient();
        var refreshClient = new StandardHttpClient(_refreshHttpClient);
        _tokenSession = new JwtTokenSession(refreshClient);

        var accessTokenHandler = new AccessTokenHandler(_tokenSession)
        {
            InnerHandler = new HttpClientHandler()
        };
        _apiHttpClient = new System.Net.Http.HttpClient(accessTokenHandler);
        _apiClient = new StandardHttpClient(_apiHttpClient);

        InitializeComponent();
        InitializeCulture();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lifetimeSource.Cancel();
            _apiHttpClient.Dispose();
            _refreshHttpClient.Dispose();
            _lifetimeSource.Dispose();
        }

        base.Dispose(disposing);
    }

    private void ShowTokensCheckedChanged(object? sender, EventArgs e)
    {
        var maskTokens = !_showTokensCheckBox.Checked;
        _accessTokenTextBox.UseSystemPasswordChar = maskTokens;
        _refreshTokenTextBox.UseSystemPasswordChar = maskTokens;
    }

    private void ApplyTokensButtonClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_accessTokenTextBox.Text))
        {
            ShowValidationError(AppResources.AccessTokenRequired);

            return;
        }

        if (string.IsNullOrWhiteSpace(_refreshTokenTextBox.Text))
        {
            ShowValidationError(AppResources.RefreshTokenRequired);

            return;
        }

        if (!TryCreateHttpUri(_refreshEndpointTextBox.Text, out var refreshEndpoint))
        {
            ShowValidationError(AppResources.InvalidRefreshUrl);

            return;
        }

        _tokenSession.SetToken(
            new Token(_accessTokenTextBox.Text.Trim(), _refreshTokenTextBox.Text.Trim()),
            refreshEndpoint);
        _statusLabel.Text = AppResources.TokenSavedStatus;
    }

    private async void SendRequestButtonClickAsync(object? sender, EventArgs e)
    {
        if (!TryCreateHttpUri(_requestUrlTextBox.Text, out var requestUrl))
        {
            ShowValidationError(AppResources.InvalidRequestUrl);

            return;
        }

        SetRequestControlsEnabled(enabled: false);
        _statusLabel.Text = AppResources.RequestingStatus;
        try
        {
            var result = await _apiClient.GetAsync<JsonElement>(
                requestUrl.AbsoluteUri,
                cancellationToken: _lifetimeSource.Token);

            if (result.IsSuccess)
            {
                _responseTextBox.Text = JsonSerializer.Serialize(result.Data, s_displayJsonOptions);
                _statusLabel.Text = AppResources.RequestSucceededStatus;

                return;
            }

            var statusCode = result.StatusCode.HasValue
                ? ((int)result.StatusCode.Value).ToString(CultureInfo.CurrentCulture)
                : AppResources.NoStatusCode;
            _responseTextBox.Text = AppResources.FormatRequestFailed(
                statusCode,
                result.ErrorMsg ?? result.FirstError?.Message ?? AppResources.RequestFailed);
            _statusLabel.Text = _responseTextBox.Text;
        }
        catch (SecurityTokenException ex)
        {
            _tokenSession.Clear();
            _statusLabel.Text = ex.Message;
            _responseTextBox.Text = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            _statusLabel.Text = ex.Message;
            _responseTextBox.Text = ex.Message;
        }
        catch (OperationCanceledException) when (_lifetimeSource.IsCancellationRequested)
        {
            _statusLabel.Text = AppResources.RequestCanceledStatus;
        }
        finally
        {
            SetRequestControlsEnabled(enabled: true);
        }
    }

    private void ClearButtonClick(object? sender, EventArgs e)
    {
        _responseTextBox.Clear();
        _statusLabel.Text = AppResources.ReadyStatus;
    }

    private void CultureSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cultureComboBox.SelectedItem is CultureInfo culture)
        {
            ApplyCulture(culture);
        }
    }

    private void InitializeCulture()
    {
        _cultureComboBox.Items.AddRange(s_supportedCultures);
        _cultureComboBox.SelectedIndex = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh"
            ? 1
            : 0;
        ApplyCulture((CultureInfo)_cultureComboBox.SelectedItem!);
        _cultureComboBox.SelectedIndexChanged += CultureSelectedIndexChanged;
    }

    private void ApplyCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        SetRequestCulture(_refreshHttpClient, culture);
        SetRequestCulture(_apiHttpClient, culture);
        ApplyLocalizedText();
        _statusLabel.Text = AppResources.ReadyStatus;
    }

    private static void SetRequestCulture(
        System.Net.Http.HttpClient httpClient,
        CultureInfo culture)
    {
        httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        httpClient.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(culture.Name));
    }

    private void SetRequestControlsEnabled(bool enabled)
    {
        _cultureComboBox.Enabled = enabled;
        _showTokensCheckBox.Enabled = enabled;
        _applyTokensButton.Enabled = enabled;
        _sendRequestButton.Enabled = enabled;
    }

    private void ShowValidationError(string message)
    {
        _statusLabel.Text = message;
        MessageBox.Show(this, message, AppResources.ValidationErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static bool TryCreateHttpUri(string value, out Uri uri)
    {
        if (Uri.TryCreate(value.Trim(), UriKind.Absolute, out var candidate) &&
            (candidate.Scheme == Uri.UriSchemeHttps || candidate.Scheme == Uri.UriSchemeHttp))
        {
            uri = candidate;

            return true;
        }

        uri = null!;

        return false;
    }

}
