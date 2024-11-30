using System;
using System.Drawing;
using System.Windows.Forms;

namespace FlybyScript
{
    public class Logger
    {
        private RichTextBox rtbDescription;

        public Logger(RichTextBox rtbDescription)
        {
            this.rtbDescription = rtbDescription;
        }

        public void Log(string message, Color color)
        {
            if (rtbDescription.InvokeRequired)
            {
                rtbDescription.Invoke(new Action(() => LogMessage(message, color)));
            }
            else
            {
                LogMessage(message, color);
            }
        }

        private void LogMessage(string message, Color color)
        {
            rtbDescription.SelectionColor = color;
            rtbDescription.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            rtbDescription.ScrollToCaret();
        }
    }
}