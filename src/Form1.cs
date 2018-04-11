// Adapted from: http://csharphelper.com/blog/2016/09/display-battery-status-using-notify-icon-c/

using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NumberDisplayer.WindowsApplication {
    public partial class Form1 : Form {
        private static readonly int IconTooltipMaxLength = 64;
        private static readonly string DefaultText = "?";
        private readonly ITextDrawer _textBitmapCreator;
        private readonly IWebServer _webServer;
        private static readonly string HttpListenerPrefix = "http://+:8468/NumberDisplayer/"; // https://stackoverflow.com/a/7007987/116895
        private DateTime _numberLastUpdated;
        private int _numberLastValue;
        private string _displayTextLastValue;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        public Form1() {
            InitializeComponent();
            _textBitmapCreator = new TextBitmapCreator();
            _webServer = new WebServer(request => {
                var number = Convert.ToInt32(request.Url.AbsolutePath.Split('/').Last());
                _numberLastUpdated = DateTime.Now;
                _numberLastValue = number;
                return $"Received: {_numberLastValue} at {_numberLastUpdated}";
            }, exception => { }, HttpListenerPrefix);
            _webServer.Run();
        }

        private void Form1_Load(object sender, EventArgs e) {
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(0, 0);
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Hide();
            ThreadPool.QueueUserWorkItem(state => {
                while (true) {
                    UpdateNumberDisplay();
                    Thread.Sleep(1000);
                }
                // ReSharper disable once FunctionNeverReturns
            });
        }

        // https://stackoverflow.com/a/12026812/116895
        private void UpdateNumberDisplay() {
            try {
                var displayText = _numberLastValue == default(int) || (DateTime.Now - _numberLastUpdated).TotalSeconds >= 300 ? DefaultText : _numberLastValue <= 99 ? $"{_numberLastValue}" : ":)";
                if (!displayText.Equals(_displayTextLastValue)) {
                    var bitmap = _textBitmapCreator.Create(displayText, DefaultFont, ForeColor, BackColor);
                    var bitmapHandle = bitmap.GetHicon();
                    DestroyIcon(notifyIcon.Icon.Handle);
                    notifyIcon.Icon = Icon.FromHandle(bitmapHandle);
                    // DestroyIcon(bitmapHandle);
                    _displayTextLastValue = displayText;
                }
                notifyIcon.Text = displayText == DefaultText ? "Waiting for number..." : $@"{_numberLastUpdated}";
            }
            catch (Exception e) {
                notifyIcon.Text = e.Message.Substring(0, e.Message.Length < IconTooltipMaxLength ? e.Message.Length : IconTooltipMaxLength - 1);
            }
        }

        private void OnExitClick(object sender, EventArgs e) {
            _webServer?.Stop();
            Close();
        }
    }
}