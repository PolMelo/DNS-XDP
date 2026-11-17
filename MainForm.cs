using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DnsSwitcher
{
    public class MainForm : Form
    {
        static readonly string[] Servers = { "85.208.114.51", "51.170.52.10" };

        readonly Button toggle = new Button();
        readonly Label status = new Label();
        bool active;

        public MainForm()
        {
            Text = "DNS Switcher";
            ClientSize = new Size(300, 120);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            toggle.SetBounds(20, 20, 260, 45);
            toggle.Click += OnToggle;

            status.SetBounds(20, 75, 260, 25);
            status.TextAlign = ContentAlignment.MiddleCenter;

            Controls.AddRange(new Control[] { toggle, status });

            // Si la app se cerró a la fuerza la última vez, restaura la DNS original
            try { DnsManager.Restore(); } catch { }

            SystemEvents.SessionEnding += delegate { SafeRestore(); };
            UpdateUi();
        }

        void OnToggle(object sender, EventArgs e)
        {
            try
            {
                if (active) DnsManager.Restore();
                else DnsManager.Enable(Servers);
                active = !active;
                UpdateUi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void UpdateUi()
        {
            toggle.Text = active ? "Desactivar" : "Activar";
            status.Text = active ? "DNS cambiada" : "DNS original";
            status.ForeColor = active ? Color.Green : Color.Gray;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SafeRestore();
            base.OnFormClosing(e);
        }

        static void SafeRestore()
        {
            try { DnsManager.Restore(); } catch { }
        }
    }
}