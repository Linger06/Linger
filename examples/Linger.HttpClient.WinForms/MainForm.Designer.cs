using System.Globalization;

namespace Linger.HttpClient.WinForms;

partial class MainForm
{
    private const int FormWidth = 900;
    private const int FormHeight = 640;
    private const int MinimumFormWidth = 720;
    private const int MinimumFormHeight = 520;
    private const int LayoutPadding = 16;
    private const int LayoutSpacing = 8;
    private const int LabelColumnWidth = 150;

    private readonly Label _cultureLabel = CreateLabel();
    private readonly Label _accessTokenLabel = CreateLabel();
    private readonly Label _refreshTokenLabel = CreateLabel();
    private readonly Label _refreshEndpointLabel = CreateLabel();
    private readonly Label _requestUrlLabel = CreateLabel();
    private readonly Label _responseLabel = CreateLabel();
    private readonly ComboBox _cultureComboBox = new()
    {
        DisplayMember = nameof(CultureInfo.NativeName),
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly TextBox _accessTokenTextBox = CreateTokenTextBox();
    private readonly TextBox _refreshTokenTextBox = CreateTokenTextBox();
    private readonly TextBox _refreshEndpointTextBox = CreateSingleLineTextBox();
    private readonly TextBox _requestUrlTextBox = CreateSingleLineTextBox();
    private readonly TextBox _responseTextBox = new()
    {
        Dock = DockStyle.Fill,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = false
    };
    private readonly CheckBox _showTokensCheckBox = new()
    {
        AutoSize = true,
        Margin = new Padding(0, LayoutSpacing, LayoutSpacing, 0)
    };
    private readonly Button _applyTokensButton = new()
    {
        AutoSize = true,
        Margin = new Padding(0, 0, LayoutSpacing, 0)
    };
    private readonly Button _sendRequestButton = new()
    {
        AutoSize = true,
        Margin = new Padding(0, 0, LayoutSpacing, 0)
    };
    private readonly Button _clearButton = new()
    {
        AutoSize = true,
        Margin = Padding.Empty
    };
    private readonly Label _statusLabel = new()
    {
        AutoEllipsis = true,
        BorderStyle = BorderStyle.Fixed3D,
        Dock = DockStyle.Fill,
        Padding = new Padding(LayoutSpacing),
        TextAlign = ContentAlignment.MiddleLeft
    };

    private void InitializeComponent()
    {
        SuspendLayout();

        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(FormWidth, FormHeight);
        MinimumSize = new Size(MinimumFormWidth, MinimumFormHeight);
        AutoScaleMode = AutoScaleMode.Dpi;

        _refreshEndpointTextBox.PlaceholderText = "https://api.example.com/refresh";
        _requestUrlTextBox.PlaceholderText = "https://api.example.com/users/me";
        ApplyLocalizedText();

        _showTokensCheckBox.CheckedChanged += ShowTokensCheckedChanged;
        _applyTokensButton.Click += ApplyTokensButtonClick;
        _sendRequestButton.Click += SendRequestButtonClickAsync;
        _clearButton.Click += ClearButtonClick;

        Controls.Add(CreateLayout());
        ResumeLayout(performLayout: true);
    }

    private Control CreateLayout()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(LayoutPadding),
            RowCount = 8
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LabelColumnWidth));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddRow(layout, _cultureLabel, _cultureComboBox, 0);
        AddRow(layout, _accessTokenLabel, _accessTokenTextBox, 1);
        AddRow(layout, _refreshTokenLabel, _refreshTokenTextBox, 2);
        AddRow(layout, _refreshEndpointLabel, _refreshEndpointTextBox, 3);
        AddRow(layout, _requestUrlLabel, _requestUrlTextBox, 4);

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, LayoutSpacing, 0, LayoutSpacing),
            WrapContents = true
        };
        actions.Controls.Add(_showTokensCheckBox);
        actions.Controls.Add(_applyTokensButton);
        actions.Controls.Add(_sendRequestButton);
        actions.Controls.Add(_clearButton);
        layout.Controls.Add(actions, 1, 5);

        AddRow(layout, _responseLabel, _responseTextBox, 6);
        layout.Controls.Add(_statusLabel, 0, 7);
        layout.SetColumnSpan(_statusLabel, 2);

        return layout;
    }

    private void ApplyLocalizedText()
    {
        Text = AppResources.FormTitle;
        _cultureLabel.Text = AppResources.LanguageLabel;
        _accessTokenLabel.Text = AppResources.AccessTokenLabel;
        _refreshTokenLabel.Text = AppResources.RefreshTokenLabel;
        _refreshEndpointLabel.Text = AppResources.RefreshEndpointLabel;
        _requestUrlLabel.Text = AppResources.RequestUrlLabel;
        _responseLabel.Text = AppResources.ResponseLabel;
        _accessTokenTextBox.PlaceholderText = AppResources.AccessTokenLabel;
        _refreshTokenTextBox.PlaceholderText = AppResources.RefreshTokenLabel;
        _showTokensCheckBox.Text = AppResources.ShowTokens;
        _applyTokensButton.Text = AppResources.ApplyTokensButton;
        _sendRequestButton.Text = AppResources.SendGetButton;
        _clearButton.Text = AppResources.ClearButton;
        _statusLabel.Text = AppResources.ReadyStatus;
    }

    private static void AddRow(TableLayoutPanel layout, Label label, Control control, int row)
    {
        control.Margin = new Padding(0, LayoutSpacing, 0, LayoutSpacing);
        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private static Label CreateLabel()
    {
        return new Label
        {
            AutoSize = true,
            Margin = new Padding(0, LayoutSpacing, LayoutSpacing, LayoutSpacing),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static TextBox CreateTokenTextBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Fill,
            UseSystemPasswordChar = true
        };
    }

    private static TextBox CreateSingleLineTextBox()
    {
        return new TextBox
        {
            Dock = DockStyle.Fill
        };
    }
}
