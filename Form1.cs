// Adapted from: http://csharphelper.com/blog/2016/09/display-battery-status-using-notify-icon-c/

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NumberDisplayer.WindowsApplication {
    public partial class Form1 : Form {
        private static readonly string DefaultText = "?";
        private readonly ITextDrawer _textDrawer;
        private readonly IWebServer _webServer;
        private static readonly string HttpListenerPrefix = "http://+:8468/NumberDisplayer/"; // https://stackoverflow.com/a/7007987/116895

        public Form1() {
            InitializeComponent();
            _textDrawer = new TextDrawer();
            _webServer = new WebServer(request => {
                var number = Convert.ToInt32(request.Url.AbsolutePath.Split('/').Last());
                UpdateNumberDisplay(number);
                return $"Received: {number} at {DateTime.Now}";
            }, exception => { }, HttpListenerPrefix);
            _webServer.Run();
        }

        private void Form1_Load(object sender, EventArgs e) {
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(0, 0);
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Hide();
            UpdateNumberDisplay();
        }

        private void UpdateNumberDisplay(int number = default(int)) {
            var displayText = number == default(int) ? DefaultText : number < 100 ? $"{number}" : ":)";
            var bitmap = _textDrawer.Draw(displayText, DefaultFont, ForeColor, BackColor);
            var icon = Icon.FromHandle(bitmap.GetHicon());
            notifyIcon.Icon = icon;
            notifyIcon.Text = displayText == DefaultText ? "Waiting for number..." : $@"{DateTime.Now}";
        }

        private void OnExitClick(object sender, EventArgs e) {
            _webServer?.Stop();
            Close();
        }
    }
}