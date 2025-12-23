using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class ForgotPasswordForm : Form
    {
        public ForgotPasswordForm()
        {
            InitializeComponents();
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void InitializeComponents()
        {
            this.Size = new Size(350, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(35, 35, 45);

            var mainPanel = new Panel
            {
                Size = new Size(330, 230),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(45, 45, 55),
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 330, 230, 15, 15))
            };

            var lblTitle = new Label
            {
                Text = "Reset Password",
                Location = new Point(0, 20),
                Size = new Size(330, 30),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblEmail = new Label
            {
                Text = "Enter your email address:",
                Location = new Point(30, 70),
                Size = new Size(270, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray
            };

            var txtEmail = new TextBox
            {
                Location = new Point(30, 100),
                Size = new Size(270, 30),
                BackColor = Color.FromArgb(60, 60, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10)
            };

            var btnSubmit = new Button
            {
                Text = "SEND RESET LINK",
                Location = new Point(30, 150),
                Size = new Size(270, 35),
                BackColor = Color.FromArgb(128, 255, 128),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s, e) =>
            {
                MessageBox.Show("Reset link has been sent to your email!", "Success",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            };

            var btnClose = new Button
            {
                Text = "×",
                Location = new Point(300, 5),
                Size = new Size(25, 25),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(lblEmail);
            mainPanel.Controls.Add(txtEmail);
            mainPanel.Controls.Add(btnSubmit);
            mainPanel.Controls.Add(btnClose);

            this.Controls.Add(mainPanel);

            // Enable dragging
            mainPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
    }
}
