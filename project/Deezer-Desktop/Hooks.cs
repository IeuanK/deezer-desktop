using Kennedy.ManagedHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Deezer_Desktop
{
    class Hooks
    {
        private MouseHook m;
        private KeyboardHook k;
        private Form1 form;

        public Hooks(Form1 f)
        {
            this.form = f;
            this.m = new MouseHook();
            this.k = new KeyboardHook();
            m.MouseEvent += mouseHook_MouseEvent;
            k.KeyboardEvent += keyboardHook_KeyboardEvent;
            k.InstallHook();
            m.InstallHook();
        }

        private void keyboardHook_KeyboardEvent(KeyboardEvents kEvent, Keys key)
        {
            Console.WriteLine(string.Format("Keyboard event: {0}", key.ToString()));
        }

        private void mouseHook_MouseEvent(MouseEvents mEvent, Point point)
        {
        }
    }
}
