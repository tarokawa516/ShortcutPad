using System.Runtime.InteropServices;

namespace ShortcutPad;

internal sealed class ShortcutPadForm : Form
{
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;
    private const int WmNchittest = 0x0084;
    private const int HtCaption = 2;

    private readonly Panel _actionPanel;
    private readonly Button _collapseButton;
    private bool _collapsed;
    private int _expandedHeight;

    public ShortcutPadForm()
    {
        Text = "Shortcut Pad";
        Name = "ShortcutPad";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(31, 36, 44);
        Padding = new Padding(2);
        ClientSize = new Size(330, 458);
        MinimumSize = new Size(250, 48);

        var header = CreateHeader();
        _collapseButton = (Button)header.Controls[1];
        _actionPanel = CreateActionPanel();

        Controls.Add(_actionPanel);
        Controls.Add(header);

        _expandedHeight = Height;
        Shown += (_, _) => PlaceNearRightEdge();
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var parameters = base.CreateParams;
            parameters.ExStyle |= WsExNoActivate | WsExToolWindow;
            return parameters;
        }
    }

    protected override void WndProc(ref Message message)
    {
        base.WndProc(ref message);

        if (message.Msg != WmNchittest || (int)message.Result != 1)
        {
            return;
        }

        var screenPoint = PointToClient(Cursor.Position);
        if (screenPoint.Y < 48 && screenPoint.X < ClientSize.Width - 92)
        {
            message.Result = (IntPtr)HtCaption;
        }
    }

    private Panel CreateHeader()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Color.FromArgb(45, 52, 62)
        };

        var title = new Label
        {
            Text = "  ショートカット",
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Yu Gothic UI", 11F, FontStyle.Bold),
            UseMnemonic = false
        };

        var collapse = CreateHeaderButton("−", ToggleCollapsed);
        collapse.Dock = DockStyle.Right;

        var close = CreateHeaderButton("×", Close);
        close.Dock = DockStyle.Right;

        header.Controls.Add(title);
        header.Controls.Add(collapse);
        header.Controls.Add(close);
        return header;
    }

    private Panel CreateActionPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 8, 8, 10),
            BackColor = BackColor
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            BackColor = BackColor,
            Margin = Padding.Empty
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var row = 0; row < 5; row++)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        }

        AddWideButton(grid, "全選択 → コピー\r\nCtrl+A, Ctrl+C", 0, SelectAllAndCopyAsync);
        AddButton(grid, "全選択\r\nCtrl+A", 0, 1, () => Send('A', ShortcutSender.Control));
        AddButton(grid, "コピー\r\nCtrl+C", 1, 1, () => Send('C', ShortcutSender.Control));
        AddButton(grid, "切り取り\r\nCtrl+X", 0, 2, () => Send('X', ShortcutSender.Control));
        AddButton(grid, "貼り付け\r\nCtrl+V", 1, 2, () => Send('V', ShortcutSender.Control));
        AddWideButton(grid, "Ctrl+Shift+V\r\nアプリ固有の貼り付け", 3,
            () => Send('V', ShortcutSender.Control, ShortcutSender.Shift));
        AddButton(grid, "元に戻す\r\nCtrl+Z", 0, 4, () => Send('Z', ShortcutSender.Control));
        AddButton(grid, "検索\r\nCtrl+F", 1, 4, () => Send('F', ShortcutSender.Control));

        panel.Controls.Add(grid);
        return panel;
    }

    private static Button CreateHeaderButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = 46,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(45, 52, 62),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F),
            TabStop = false,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) => action();
        return button;
    }

    private static Button CreateActionButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(4),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(61, 71, 85),
            ForeColor = Color.White,
            Font = new Font("Yu Gothic UI", 10.5F, FontStyle.Bold),
            TabStop = false,
            UseMnemonic = false,
            Cursor = Cursors.Hand
        };
        button.FlatAppearance.BorderColor = Color.FromArgb(91, 107, 127);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(73, 91, 112);
        button.Click += (_, _) => action();
        return button;
    }

    private static void AddButton(TableLayoutPanel grid, string text, int column, int row, Action action)
    {
        grid.Controls.Add(CreateActionButton(text, action), column, row);
    }

    private static void AddWideButton(TableLayoutPanel grid, string text, int row, Action action)
    {
        var button = CreateActionButton(text, action);
        grid.Controls.Add(button, 0, row);
        grid.SetColumnSpan(button, 2);
    }

    private async void SelectAllAndCopyAsync()
    {
        try
        {
            ShortcutSender.SendChord('A', ShortcutSender.Control);
            await Task.Delay(80);
            ShortcutSender.SendChord('C', ShortcutSender.Control);
        }
        catch (Exception exception)
        {
            ShowSendError(exception);
        }
    }

    private void Send(char key, params ushort[] modifiers)
    {
        try
        {
            ShortcutSender.SendChord(key, modifiers);
        }
        catch (Exception exception)
        {
            ShowSendError(exception);
        }
    }

    private void ToggleCollapsed()
    {
        _collapsed = !_collapsed;
        if (_collapsed)
        {
            _expandedHeight = Height;
            _actionPanel.Visible = false;
            Height = 48;
            _collapseButton.Text = "+";
        }
        else
        {
            Height = _expandedHeight;
            _actionPanel.Visible = true;
            _collapseButton.Text = "−";
        }
    }

    private void PlaceNearRightEdge()
    {
        var workingArea = Screen.PrimaryScreen?.WorkingArea ?? Screen.GetWorkingArea(this);
        Location = new Point(
            workingArea.Right - Width - 16,
            workingArea.Top + Math.Max(16, (workingArea.Height - Height) / 2));
    }

    private void ShowSendError(Exception exception)
    {
        MessageBox.Show(
            this,
            exception.Message + "\r\n\r\n管理者として動作中のアプリには、同じ権限で起動してください。",
            "Shortcut Pad",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
}
