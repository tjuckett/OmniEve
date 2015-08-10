using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Controls;

namespace OmniEveModules.Logging
{
    public class TextBoxWriter : TextWriter
    {
        private MetroTextBox textbox;
        public TextBoxWriter(MetroTextBox textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(string value)
        {
            textbox.Invoke((MethodInvoker)delegate 
            {
                textbox.AppendText(value);
            });
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
