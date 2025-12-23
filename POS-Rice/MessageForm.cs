using System;
using System.Drawing;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class MessageForm : Form
    {
        public MessageForm(string message, MessageType type, Color color)
        {
            InitializeComponent(message, type, color);
        }

        private void InitializeComponent(string message, MessageType type, Color color)
        {
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = color;

            var mainPanel = new Panel
            {
                Size = new Size(280, 130),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(35, 35, 45),
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 280, 130, 15, 15))
            };

            // Icon based on message type
            string icon = type == MessageType.Success ? "✅" :
                         type == MessageType.Error ? "❌" : "ℹ️";

            var lblIcon = new Label
            {
                Text = icon,
                Location = new Point(20, 20),
                Size = new Size(40, 40),
                Font = new Font("Segoe UI", 20),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblMessage = new Label
            {
                Text = message,
                Location = new Point(70, 20),
                Size = new Size(190, 60),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnOK = new Button
            {
                Text = "OK",
                Location = new Point(100, 90),
                Size = new Size(80, 30),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnOK.FlatAppearance.BorderSize = 0;

            mainPanel.Controls.Add(lblIcon);
            mainPanel.Controls.Add(lblMessage);
            mainPanel.Controls.Add(btnOK);

            this.Controls.Add(mainPanel);
            this.AcceptButton = btnOK;
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
    }
}