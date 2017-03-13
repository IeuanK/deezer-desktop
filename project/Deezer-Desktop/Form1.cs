using CefSharp;
using CefSharp.WinForms;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deezer_Desktop
{
    public partial class Form1 : Form
    {
        public ChromiumWebBrowser chromeBrowser;
        public string title;
        private KeyHook keyHook;
        public PlayingState state = PlayingState.NONE;
        public bool initialized = false;
        public string initializedURL = "";

        public Form1()
        {
            CefSettings settings = new CefSettings();
            settings.CefCommandLineArgs["enable-system-flash"] = "1";
            settings.CachePath = Path.Combine(Path.GetTempPath(), "Deezer-Desktop");
            Cef.Initialize(settings);
            chromeBrowser = new ChromiumWebBrowser("https://deezer.com/");
            chromeBrowser.RegisterJsObject("clickCallback", new BoundObject(this));
            //chromeBrowser = new ChromiumWebBrowser("chrome://flags");
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;

            chromeBrowser.LoadingStateChanged += ChromeBrowser_LoadingStateChanged;
            chromeBrowser.TitleChanged += ChromeBrowser_TitleChanged;
            chromeBrowser.FrameLoadEnd += ChromeBrowser_FrameLoadEnd;

            keyHook = new KeyHook();
            keyHook.KeyHook_Mode = KeyHook.KeyHook_Modes.Hooks;
            keyHook.VKCodeUp += KeyHook_VKCodeUp;
            keyHook.Enabled = true;

            MouseDownFilter mouseFilter = new MouseDownFilter(this);
            mouseFilter.FormClicked += MouseFilter_FormClicked;
            Application.AddMessageFilter(mouseFilter);


            InitializeComponent();
        }

        private void KeyHook_VKCodeUp(int vkcode)
        {
            switch(vkcode)
            {
                case 179:
                    musicControl(ControlCommand.PLAY);
                    break;
                case 178:
                    musicControl(ControlCommand.STOP);
                    break;
                case 177:
                    musicControl(ControlCommand.PREV);
                    break;
                case 176:
                    musicControl(ControlCommand.NEXT);
                    break;

            }
        }

        private void callFixState()
        {
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(250);
                this.fixState();
            });
        }

        private void ChromeBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            try
            {
                if(!initialized || initializedURL != chromeBrowser.Address)
                {
                    Console.WriteLine("Frame load end; "+ chromeBrowser.Address);
                    //chromeBrowser.ShowDevTools();
                    //chromeBrowser.ExecuteScriptAsync(@"$('.control-play').on('click', function() { clickCallback.clickCallback(); });");
                    //chromeBrowser.ExecuteScriptAsync(@"$('.icon-play').on('click', function() { clickCallback.clickCallback(); });");
                    //chromeBrowser.ExecuteScriptAsync(@"$('.btn-play').on('click', function() { clickCallback.clickCallback(); });");
                    chromeBrowser.ExecuteScriptAsync(@"$(document).on('click', function() { clickCallback.clickCallback(); });");
                    initialized = true;
                    initializedURL = chromeBrowser.Address;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void MouseFilter_FormClicked(object sender, EventArgs e)
        {
            grabPlayingState();
        }

        public void fixState()
        {
            if(InvokeRequired)
            {
                Invoke(new Action(fixState));
                return;
            }

            this.grabPlayingState();
        }

        private void updateNotifyIcon()
        {
            switch(state)
            {
                case PlayingState.PLAYING:
                    Console.WriteLine("Setting icon to playing");
                    notifyIcon1.Icon = notifyIcon_playing.Icon;
                    break;
                case PlayingState.PAUSED:
                    Console.WriteLine("Setting icon to paused");
                    notifyIcon1.Icon = notifyIcon_paused.Icon;
                    break;
                case PlayingState.NONE:
                    Console.WriteLine("Setting icon to none");
                    notifyIcon1.Icon = notifyIcon_none.Icon;
                    break;
                default:
                    Console.WriteLine("Setting icon to none");
                    notifyIcon1.Icon = notifyIcon_none.Icon;
                    break;
            }
        }

        public void grabPlayingState()
        {
            Task<JavascriptResponse> task = chromeBrowser.EvaluateScriptAsync("dzPlayer.isPlaying()");

            task.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    var jsResult = response.Success ? (response.Result ?? "null") : response.Message;

                    Boolean playing;
                    if(Boolean.TryParse(jsResult.ToString(), out playing))
                    {
                        if(playing)
                        {
                            Console.WriteLine("Setting state to playing");
                            state = PlayingState.PLAYING;
                        } else
                        {
                            Console.WriteLine("Setting state to paused");
                            state = PlayingState.PAUSED;
                        }
                    } else
                    {
                        Console.WriteLine("Setting state to none");
                        state = PlayingState.NONE;
                    }
                    updateNotifyIcon();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void hotkey_HotkeyPressed(object sender, EventArgs e)
        {
            Console.WriteLine("Hotkey triggered");
        }

        private int keyboardHook(int code, IntPtr wParam, IntPtr lParam, ref bool callNext)
        {
            if(code > 0)
            {
                Console.WriteLine("Keyboard hook:");
                Console.WriteLine("code: " + code + ", wParam: " + wParam.ToString() + ", lParam: " + lParam.ToString());
            }
            return code;
        }

        private void ChromeBrowser_TitleChanged(object sender, TitleChangedEventArgs e)
        {
            if(InvokeRequired)
            {
                Invoke(new Action<object, TitleChangedEventArgs>(ChromeBrowser_TitleChanged), sender, e);
                return;
            }
            this.title = e.Title;
            this.Text = title;
        }

        private void ChromeBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.KeyCode.ToString());
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                //notifyIcon1.ShowBalloonTip(500);
                this.Hide();
            }
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            /*var bounds = this.MaximizedBounds;
            this.Size = new Size(bounds.Width, bounds.Height);*/
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.BringToFront();

            this.TopMost = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Program.mutex.ReleaseMutex();
            }
            catch (Exception)
            {
                Console.WriteLine("Mutex release failed");
            }
            finally
            {
                Console.WriteLine("Mutex release worked");
            }
        }

        private void musicControl(ControlCommand command)
        {
            switch(command)
            {
                case ControlCommand.NEXT:
                    chromeBrowser.ExecuteScriptAsync(@"dzPlayer.control.nextSong();");
                    break;
                case ControlCommand.PREV:
                    chromeBrowser.ExecuteScriptAsync(@"dzPlayer.control.prevSong();");
                    break;
                case ControlCommand.PLAY:
                    chromeBrowser.ExecuteScriptAsync(@"dzPlayer.control.togglePause();");
                    break;
                case ControlCommand.STOP:
                    chromeBrowser.ExecuteScriptAsync(@"dzPlayer.control.seek(0); setTimeout(function() { dzPlayer.control.pause(); }, 5);");
                    break;
            }
            callFixState();
        }

        private void skipSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            musicControl(ControlCommand.NEXT);
        }

        private void playPauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            musicControl(ControlCommand.PLAY);
        }

        private void exitProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                musicControl(ControlCommand.PLAY);
            }
        }
    }

    public class KeyboardHandler : IKeyboardHandler
    {
        Form1 form;

        public KeyboardHandler(Form1 form)
        {
            this.form = form;
        }
        public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            Console.WriteLine("browser_keyevent: {\"type\": " + type.ToString() + ",\"windowsKeyCode\": " + windowsKeyCode.ToString() + ",\"nativeKeyCode\": " + nativeKeyCode.ToString() + ",\"isSystemKey\": " + isSystemKey.ToString() + "}");
            return true;
        }

        public bool OnPreKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            Console.WriteLine("browser_prekeyevent: {\"type\": " + type.ToString() + ",\"windowsKeyCode\": " + windowsKeyCode.ToString() + ",\"nativeKeyCode\": " + nativeKeyCode.ToString() + ",\"isSystemKey\": " + isSystemKey.ToString() + "}");
            return true;
        }
    }

    public enum PlayingState
    {
        NONE,
        PLAYING,
        PAUSED
    }

    public class MouseDownFilter : IMessageFilter
    {
        public event EventHandler FormClicked;
        private int WM_LBUTTONDOWN = 0x201;
        private Form form = null;

        [DllImport("user32.dll")]
        public static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        public MouseDownFilter(Form f)
        {
            form = f;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN)
            {
                if (Form.ActiveForm != null && Form.ActiveForm.Equals(form))
                {
                    OnFormClicked();
                }
            }
            return false;
        }

        protected void OnFormClicked()
        {
            if (FormClicked != null)
            {
                FormClicked(form, EventArgs.Empty);
            }
        }
    }

    public enum ControlCommand
    {
        PLAY, NEXT, PREV, STOP
    }

    public class BoundObject
    {
        private Form1 form;
        public BoundObject(Form1 form)
        {
            this.form = form;
        }

        public void clickCallback()
        {
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(250);
                form.fixState();
            });
        }
    }
}
