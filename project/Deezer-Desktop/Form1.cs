using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deezer_Desktop
{
    public partial class Form1 : Form
    {
        public ChromiumWebBrowser chromeBrowser;
        public string title;

        public Form1()
        {
            CefSettings settings = new CefSettings();
            settings.CefCommandLineArgs["enable-system-flash"] = "1";
            settings.CachePath = Path.Combine(Path.GetTempPath(), "Deezer-Desktop");
            Cef.Initialize(settings);
            chromeBrowser = new ChromiumWebBrowser("https://deezer.com/");
            //chromeBrowser = new ChromiumWebBrowser("chrome://flags");
            this.Controls.Add(chromeBrowser);
            chromeBrowser.Dock = DockStyle.Fill;

            chromeBrowser.LoadingStateChanged += ChromeBrowser_LoadingStateChanged;
            chromeBrowser.TitleChanged += ChromeBrowser_TitleChanged;

            InitializeComponent();
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
    }
}
