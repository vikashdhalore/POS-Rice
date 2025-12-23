using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class ChangePasswordForm : Form
    {
        // For rounded corners
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        // For form dragging
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        // Database connection string
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

        // UI Controls
        private TabControl tabControl;
        private TextBox txtUsernameVerify;
        private TextBox txtOldPasswordVerify;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtUsernameByOldPass;
        private TextBox txtOldPassword;
        private TextBox txtNewPasswordByOldPass;
        private TextBox txtConfirmPasswordByOldPass;
        private Button btnChangePassword;
        private Button btnClose;
        private Label lblStatus;
        private bool isNewPasswordVisible = false;
        private bool isConfirmPasswordVisible = false;

        public ChangePasswordForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Form setup
            this.Text = "Change Password";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.Padding = new Padding(20);

            // Apply rounded corners
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            CreateControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Close button
            btnClose = new Button
            {
                Text = "✕",
                Size = new Size(40, 40),
                Location = new Point(440, 10),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 50, 50);

            // Title
            Label lblTitle = new Label
            {
                Text = "CHANGE PASSWORD",
                Location = new Point(0, 20),
                Size = new Size(460, 40),
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Subtitle
            Label lblSubtitle = new Label
            {
                Text = "Choose your verification method",
                Location = new Point(0, 70),
                Size = new Size(460, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Create Tab Control
            tabControl = new TabControl
            {
                Location = new Point(20, 110),
                Size = new Size(440, 380),
                Font = new Font("Segoe UI", 10),
                ItemSize = new Size(200, 30),
                Appearance = TabAppearance.FlatButtons
            };

            // Remove borders and styling
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += TabControl_DrawItem;

            // Tab 1: Verify by Username
            TabPage tabUsername = new TabPage
            {
                Text = "Verify by Username",
                BackColor = Color.FromArgb(35, 35, 50)
            };
            CreateUsernameTabContent(tabUsername);

            // Tab 2: Verify by Old Password
            TabPage tabOldPassword = new TabPage
            {
                Text = "Verify by Old Password",
                BackColor = Color.FromArgb(35, 35, 50)
            };
            CreateOldPasswordTabContent(tabOldPassword);

            tabControl.Controls.Add(tabUsername);
            tabControl.Controls.Add(tabOldPassword);

            // Status label
            lblStatus = new Label
            {
                Location = new Point(20, 500),
                Size = new Size(440, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // Change Password Button
            btnChangePassword = new Button
            {
                Text = "CHANGE PASSWORD",
                Location = new Point(20, 535),
                Size = new Size(440, 45),
                BackColor = Color.FromArgb(86, 156, 214),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChangePassword.FlatAppearance.BorderSize = 0;
            btnChangePassword.FlatAppearance.MouseOverBackColor = Color.FromArgb(66, 136, 194);
            btnChangePassword.FlatAppearance.MouseDownBackColor = Color.FromArgb(46, 116, 174);

            // Add controls to form
            this.Controls.Add(btnClose);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(tabControl);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnChangePassword);

            // Enable form dragging
            EnableFormDrag(this);
            EnableFormDrag(lblTitle);
            EnableFormDrag(lblSubtitle);
        }

        private void CreateUsernameTabContent(TabPage tabPage)
        {
            int yPos = 20;

            // Username Label
            Label lblUsername = new Label
            {
                Text = "Username",
                Location = new Point(20, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            yPos += 30;

            // Username TextBox
            txtUsernameVerify = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(380, 35),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            yPos += 50;

            // Verification Password Label
            Label lblVerifyPass = new Label
            {
                Text = "Verification Password",
                Location = new Point(20, yPos),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            yPos += 30;

            // Verification Password Panel
            Panel verifyPassPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(45, 45, 65),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtOldPasswordVerify = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(330, 25),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                UseSystemPasswordChar = true,
               // PlaceholderText = "Enter current password"
            };

            Button btnToggleVerifyPass = new Button
            {
                Location = new Point(340, 5),
                Size = new Size(30, 25),
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand,
                Tag = "verify"
            };
            btnToggleVerifyPass.FlatAppearance.BorderSize = 0;
            btnToggleVerifyPass.Click += TogglePasswordVisibility;
            yPos += 50;

            verifyPassPanel.Controls.Add(txtOldPasswordVerify);
            verifyPassPanel.Controls.Add(btnToggleVerifyPass);

            // New Password Label
            Label lblNewPassword = new Label
            {
                Text = "New Password",
                Location = new Point(20, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            yPos += 30;

            // New Password Panel
            Panel newPassPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(45, 45, 65),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtNewPassword = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(330, 25),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                UseSystemPasswordChar = true,
                //PlaceholderText = "Enter new password"
            };

            Button btnToggleNewPass = new Button
            {
                Location = new Point(340, 5),
                Size = new Size(30, 25),
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand,
                Tag = "new"
            };
            btnToggleNewPass.FlatAppearance.BorderSize = 0;
            btnToggleNewPass.Click += TogglePasswordVisibility;
            yPos += 50;

            newPassPanel.Controls.Add(txtNewPassword);
            newPassPanel.Controls.Add(btnToggleNewPass);

            // Confirm Password Label
            Label lblConfirmPassword = new Label
            {
                Text = "Confirm New Password",
                Location = new Point(20, yPos),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            yPos += 30;

            // Confirm Password Panel
            Panel confirmPassPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(45, 45, 65),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtConfirmPassword = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(330, 25),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                UseSystemPasswordChar = true,
                //PlaceholderText = "Re-enter new password"
            };

            Button btnToggleConfirmPass = new Button
            {
                Location = new Point(340, 5),
                Size = new Size(30, 25),
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand,
                Tag = "confirm"
            };
            btnToggleConfirmPass.FlatAppearance.BorderSize = 0;
            btnToggleConfirmPass.Click += TogglePasswordVisibility;

            confirmPassPanel.Controls.Add(txtConfirmPassword);
            confirmPassPanel.Controls.Add(btnToggleConfirmPass);

            // Add controls to tab page
            tabPage.Controls.Add(lblUsername);
            tabPage.Controls.Add(txtUsernameVerify);
            tabPage.Controls.Add(lblVerifyPass);
            tabPage.Controls.Add(verifyPassPanel);
            tabPage.Controls.Add(lblNewPassword);
            tabPage.Controls.Add(newPassPanel);
            tabPage.Controls.Add(lblConfirmPassword);
            tabPage.Controls.Add(confirmPassPanel);
        }

        private void CreateOldPasswordTabContent(TabPage tabPage)
        {
            int yPos = 20;

            // Username Label
            Label lblUsername = new Label
            {
                Text = "Username",
                Location = new Point(20, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            yPos += 30;

            // Username TextBox
            txtUsernameByOldPass = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(380, 35),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            yPos += 50;

            // Old Password Label
            Label lblOldPassword = new Label
            {
                Text = "Old Password",
                Location = new Point(20, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            yPos += 30;

            // Old Password Panel
            Panel oldPassPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(45, 45, 65),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtOldPassword = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(330, 25),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                UseSystemPasswordChar = true,
                //PlaceholderText = "Enter current password"
            };

            Button btnToggleOldPass = new Button
            {
                Location = new Point(340, 5),
                Size = new Size(30, 25),
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand,
                Tag = "old"
            };
            btnToggleOldPass.FlatAppearance.BorderSize = 0;
            btnToggleOldPass.Click += TogglePasswordVisibility;
            yPos += 50;

            oldPassPanel.Controls.Add(txtOldPassword);
            oldPassPanel.Controls.Add(btnToggleOldPass);

            // New Password Label
            Label lblNewPassword = new Label
            {
                Text = "New Password",
                Location = new Point(20, yPos),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            yPos += 30;

            // New Password Panel
            Panel newPassPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(45, 45, 65),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtNewPasswordByOldPass = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(330, 25),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                UseSystemPasswordChar = true,
                //PlaceholderText = "Enter new password"
            };

            Button btnToggleNewPassByOld = new Button
            {
                Location = new Point(340, 5),
                Size = new Size(30, 25),
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand,
                Tag = "new_old"
            };
            btnToggleNewPassByOld.FlatAppearance.BorderSize = 0;
            btnToggleNewPassByOld.Click += TogglePasswordVisibility;
            yPos += 50;

            newPassPanel.Controls.Add(txtNewPasswordByOldPass);
            newPassPanel.Controls.Add(btnToggleNewPassByOld);

            // Confirm Password Label
            Label lblConfirmPassword = new Label
            {
                Text = "Confirm New Password",
                Location = new Point(20, yPos),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };
            yPos += 30;

            // Confirm Password Panel
            Panel confirmPassPanel = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(45, 45, 65),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtConfirmPasswordByOldPass = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(330, 25),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                UseSystemPasswordChar = true,
                //PlaceholderText = "Re-enter new password"
            };

            Button btnToggleConfirmPassByOld = new Button
            {
                Location = new Point(340, 5),
                Size = new Size(30, 25),
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand,
                Tag = "confirm_old"
            };
            btnToggleConfirmPassByOld.FlatAppearance.BorderSize = 0;
            btnToggleConfirmPassByOld.Click += TogglePasswordVisibility;

            confirmPassPanel.Controls.Add(txtConfirmPasswordByOldPass);
            confirmPassPanel.Controls.Add(btnToggleConfirmPassByOld);

            // Add controls to tab page
            tabPage.Controls.Add(lblUsername);
            tabPage.Controls.Add(txtUsernameByOldPass);
            tabPage.Controls.Add(lblOldPassword);
            tabPage.Controls.Add(oldPassPanel);
            tabPage.Controls.Add(lblNewPassword);
            tabPage.Controls.Add(newPassPanel);
            tabPage.Controls.Add(lblConfirmPassword);
            tabPage.Controls.Add(confirmPassPanel);
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            TabPage tabPage = tabControl.TabPages[e.Index];

            // Draw background
            if (e.Index == tabControl.SelectedIndex)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 70)), e.Bounds);
                e.Graphics.DrawString(tabPage.Text, e.Font, Brushes.White, e.Bounds.X + 5, e.Bounds.Y + 5);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(40, 40, 60)), e.Bounds);
                e.Graphics.DrawString(tabPage.Text, e.Font, Brushes.Gray, e.Bounds.X + 5, e.Bounds.Y + 5);
            }
        }

        private void TogglePasswordVisibility(object sender, EventArgs e)
        {
            Button button = sender as Button;
            string tag = button.Tag.ToString();
            TextBox passwordBox = null;

            // Find the corresponding textbox based on tag
            switch (tag)
            {
                case "verify":
                    passwordBox = txtOldPasswordVerify;
                    break;
                case "new":
                    passwordBox = txtNewPassword;
                    break;
                case "confirm":
                    passwordBox = txtConfirmPassword;
                    break;
                case "old":
                    passwordBox = txtOldPassword;
                    break;
                case "new_old":
                    passwordBox = txtNewPasswordByOldPass;
                    break;
                case "confirm_old":
                    passwordBox = txtConfirmPasswordByOldPass;
                    break;
            }

            if (passwordBox != null)
            {
                passwordBox.UseSystemPasswordChar = !passwordBox.UseSystemPasswordChar;
                button.Text = passwordBox.UseSystemPasswordChar ? "👁" : "🙈";
                passwordBox.Focus();
            }
        }

        private void SetupEventHandlers()
        {
            btnClose.Click += (s, e) => this.Close();
            btnChangePassword.Click += BtnChangePassword_Click;

            // Enter key to submit
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnChangePassword_Click(null, null);
                }
            };
        }

        private void EnableFormDrag(Control control)
        {
            control.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            };
        }

        private async void BtnChangePassword_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == 0)
            {
                // Verify by Username tab
                await ProcessUsernameVerification();
            }
            else
            {
                // Verify by Old Password tab
                await ProcessOldPasswordVerification();
            }
        }

        private async Task ProcessUsernameVerification()
        {
            string username = txtUsernameVerify.Text.Trim();
            string oldPassword = txtOldPasswordVerify.Text;
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // Validation
            if (string.IsNullOrEmpty(username))
            {
                ShowStatus("Please enter username", Color.Red);
                return;
            }

            if (string.IsNullOrEmpty(oldPassword))
            {
                ShowStatus("Please enter verification password", Color.Red);
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ShowStatus("Please enter new password", Color.Red);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowStatus("New passwords do not match", Color.Red);
                txtNewPassword.Focus();
                txtNewPassword.SelectAll();
                return;
            }

            if (newPassword.Length < 6)
            {
                ShowStatus("Password must be at least 6 characters", Color.Red);
                return;
            }

            // Disable UI
            SetUIState(false);
            ShowStatus("Verifying and updating password...", Color.Yellow);

            try
            {
                bool success = await Task.Run(() => ChangePasswordByUsername(username, oldPassword, newPassword));

                if (success)
                {
                    ShowStatus("Password changed successfully!", Color.Green);
                    await Task.Delay(2000);
                    ClearUsernameTabFields();
                    this.Close();
                }
                else
                {
                    ShowStatus("Invalid username or verification password", Color.Red);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", Color.Red);
            }
            finally
            {
                SetUIState(true);
            }
        }

        private async Task ProcessOldPasswordVerification()
        {
            string username = txtUsernameByOldPass.Text.Trim();
            string oldPassword = txtOldPassword.Text;
            string newPassword = txtNewPasswordByOldPass.Text;
            string confirmPassword = txtConfirmPasswordByOldPass.Text;

            // Validation
            if (string.IsNullOrEmpty(username))
            {
                ShowStatus("Please enter username", Color.Red);
                return;
            }

            if (string.IsNullOrEmpty(oldPassword))
            {
                ShowStatus("Please enter old password", Color.Red);
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ShowStatus("Please enter new password", Color.Red);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowStatus("New passwords do not match", Color.Red);
                txtNewPasswordByOldPass.Focus();
                txtNewPasswordByOldPass.SelectAll();
                return;
            }

            if (newPassword.Length < 6)
            {
                ShowStatus("Password must be at least 6 characters", Color.Red);
                return;
            }

            // Disable UI
            SetUIState(false);
            ShowStatus("Verifying and updating password...", Color.Yellow);

            try
            {
                bool success = await Task.Run(() => ChangePasswordByOldPassword(username, oldPassword, newPassword));

                if (success)
                {
                    ShowStatus("Password changed successfully!", Color.Green);
                    await Task.Delay(2000);
                    ClearOldPasswordTabFields();
                    this.Close();
                }
                else
                {
                    ShowStatus("Invalid username or old password", Color.Red);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", Color.Red);
            }
            finally
            {
                SetUIState(true);
            }
        }

        private bool ChangePasswordByUsername(string username, string verificationPassword, string newPassword)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // First verify the user exists with verification password
                string verifyQuery = @"
                    SELECT COUNT(*) 
                    FROM UserAccount 
                    WHERE Username = @Username 
                    AND VerificationPassword = @VerificationPassword";

                using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, connection))
                {
                    verifyCmd.Parameters.AddWithValue("@Username", username);
                    verifyCmd.Parameters.AddWithValue("@VerificationPassword", verificationPassword);

                    int userCount = Convert.ToInt32(verifyCmd.ExecuteScalar());

                    if (userCount > 0)
                    {
                        // Update password
                        string updateQuery = @"
                            UPDATE UserAccount 
                            SET Password = @NewPassword 
                            WHERE Username = @Username";

                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@Username", username);
                            updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);

                            int rowsAffected = updateCmd.ExecuteNonQuery();
                            return rowsAffected > 0;
                        }
                    }
                }
                return false;
            }
        }

        private bool ChangePasswordByOldPassword(string username, string oldPassword, string newPassword)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // First verify the user exists with old password
                string verifyQuery = @"
                    SELECT COUNT(*) 
                    FROM UserAccount 
                    WHERE Username = @Username 
                    AND Password = @OldPassword";

                using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, connection))
                {
                    verifyCmd.Parameters.AddWithValue("@Username", username);
                    verifyCmd.Parameters.AddWithValue("@OldPassword", oldPassword);

                    int userCount = Convert.ToInt32(verifyCmd.ExecuteScalar());

                    if (userCount > 0)
                    {
                        // Update password
                        string updateQuery = @"
                            UPDATE UserAccount 
                            SET Password = @NewPassword 
                            WHERE Username = @Username";

                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@Username", username);
                            updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);

                            int rowsAffected = updateCmd.ExecuteNonQuery();
                            return rowsAffected > 0;
                        }
                    }
                }
                return false;
            }
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
            lblStatus.Visible = true;
        }

        private void SetUIState(bool enabled)
        {
            btnChangePassword.Enabled = enabled;
            btnClose.Enabled = enabled;
            tabControl.Enabled = enabled;

            if (enabled)
            {
                Cursor = Cursors.Default;
                btnChangePassword.Text = "CHANGE PASSWORD";
            }
            else
            {
                Cursor = Cursors.WaitCursor;
                btnChangePassword.Text = "PROCESSING...";
            }
        }

        private void ClearUsernameTabFields()
        {
            txtUsernameVerify.Clear();
            txtOldPasswordVerify.Clear();
            txtNewPassword.Clear();
            txtConfirmPassword.Clear();
        }

        private void ClearOldPasswordTabFields()
        {
            txtUsernameByOldPass.Clear();
            txtOldPassword.Clear();
            txtNewPasswordByOldPass.Clear();
            txtConfirmPasswordByOldPass.Clear();
        }

        // Method to open this form from your main application
        public static void ShowChangePassword()
        {
            ChangePasswordForm form = new ChangePasswordForm();
            form.ShowDialog();
        }
    }
}