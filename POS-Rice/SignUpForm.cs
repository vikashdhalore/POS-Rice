using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class SignUpForm : Form
    {
       private int userID;
        
        // Database connection string
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

        // For rounded corners
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public SignUpForm()
        {
            InitializeComponents();
            ApplyModernStyling();
            AutoGenerateUserID();
        }

        private void AutoGenerateUserID()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                
                conn.Open();
                SqlCommand cmd = new SqlCommand("Select ISNULL(MAX(UserID), 0) + 1 from UserAccount", conn);
                int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                userID = nextID;
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error generating Product ID: "+ ex.Message);
            }
            finally 
            {
                conn.Close();
                //textBox2.Focus();
            }
            
        }

        private void InitializeComponents()
        {
            SuspendLayout();

            // Form Properties
            this.Text = "POS Rice - Add New User";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(25, 25, 35);

            // Main Container Panel
            var mainPanel = new Panel
            {
                Size = new Size(480, 580),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(35, 35, 45),
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 480, 580, 20, 20))
            };

            // Close Button
            var btnClose = new Button
            {
                Text = "×",
                Size = new Size(30, 30),
                Location = new Point(440, 10),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            // Title Label
            var lblTitle = new Label
            {
                Text = "Add New User",
                Location = new Point(0, 40),
                Size = new Size(480, 40),
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Subtitle Label
            var lblSubtitle = new Label
            {
                Text = "Create new user account for POS Rice System",
                Location = new Point(0, 90),
                Size = new Size(480, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username Field
            var lblUsername = new Label
            {
                Text = "Username *",
                Location = new Point(50, 140),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            var txtUsername = new TextBox
            {
                Location = new Point(50, 165),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                Name = "txtUsername"
            };

            // Password Field
            var lblPassword = new Label
            {
                Text = "Password *",
                Location = new Point(50, 220),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            var txtPassword = new TextBox
            {
                Location = new Point(50, 245),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                Name = "txtPassword"
            };

            // Confirm Password Field
            var lblConfirmPassword = new Label
            {
                Text = "Confirm Password *",
                Location = new Point(50, 300),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            var txtConfirmPassword = new TextBox
            {
                Location = new Point(50, 325),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                UseSystemPasswordChar = true,
                Name = "txtConfirmPassword"
            };

            // Phone Number Field
            var lblPhone = new Label
            {
                Text = "Phone Number",
                Location = new Point(50, 380),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            var txtPhone = new TextBox
            {
                Location = new Point(50, 405),
                Size = new Size(380, 35),
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                Name = "txtPhone"
            };

            // Buttons
            var btnAddUser = new Button
            {
                Text = "➕ ADD USER",
                Size = new Size(180, 45),
                Location = new Point(50, 470),
                BackColor = Color.FromArgb(76, 175, 80), // Green
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Name = "btnAddUser"
            };
            btnAddUser.FlatAppearance.BorderSize = 0;
            btnAddUser.Click += BtnAddUser_Click;

            var btnClear = new Button
            {
                Text = "🗑️ CLEAR",
                Size = new Size(180, 45),
                Location = new Point(250, 470),
                BackColor = Color.FromArgb(244, 67, 54), // Red
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += BtnClear_Click;

            var btnViewUsers = new Button
            {
                Text = "👥 VIEW USERS",
                Size = new Size(380, 40),
                Location = new Point(50, 530),
                BackColor = Color.FromArgb(33, 150, 243), // Blue
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnViewUsers.FlatAppearance.BorderSize = 0;
            btnViewUsers.Click += BtnViewUsers_Click;

            // Add controls to main panel
            mainPanel.Controls.Add(btnClose);
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(lblSubtitle);
            mainPanel.Controls.Add(lblUsername);
            mainPanel.Controls.Add(txtUsername);
            mainPanel.Controls.Add(lblPassword);
            mainPanel.Controls.Add(txtPassword);
            mainPanel.Controls.Add(lblConfirmPassword);
            mainPanel.Controls.Add(txtConfirmPassword);
            mainPanel.Controls.Add(lblPhone);
            mainPanel.Controls.Add(txtPhone);
            mainPanel.Controls.Add(btnAddUser);
            mainPanel.Controls.Add(btnClear);
            mainPanel.Controls.Add(btnViewUsers);

            // Add main panel to form
            this.Controls.Add(mainPanel);

            // Enable drag movement
            EnableFormDrag(mainPanel);

            ResumeLayout();
        }

        private void ApplyModernStyling()
        {
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }

        private void EnableFormDrag(Control control)
        {
            control.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
        }

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            AddNewUser();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void BtnViewUsers_Click(object sender, EventArgs e)
        {
            ViewAllUsers();
        }


        private async void AddNewUser()
        {
            try
            {
                // Get form values
                
                string username = GetControlText("txtUsername");
                string password = GetControlText("txtPassword");
                string confirmPassword = GetControlText("txtConfirmPassword");
                string phone = GetControlText("txtPhone");

                // Validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowMessage("Please enter username and password!", MessageType.Error);
                    return;
                }

                if (password != confirmPassword)
                {
                    ShowMessage("Passwords do not match!", MessageType.Error);
                    return;
                }

                if (username.Length < 3)
                {
                    ShowMessage("Username must be at least 3 characters long!", MessageType.Error);
                    return;
                }

                if (password.Length < 6)
                {
                    ShowMessage("Password must be at least 6 characters long!", MessageType.Error);
                    return;
                }

                // Check if username already exists
                if (await IsUsernameExists(username))
                {
                    ShowMessage("Username already exists! Please choose a different one.", MessageType.Error);
                    return;
                }


                // Show loading
                var btnAdd = (Button)GetControl("btnAddUser");
                btnAdd.Text = "ADDING...";
                btnAdd.Enabled = false;

                // Add user to database
                bool success = await InsertUserToDatabase(userID,username, password, phone);

                
                if (success)
                {
                    ShowMessage("User added successfully!", MessageType.Success);
                    ClearForm();
                }
                else
                {
                    ShowMessage("Failed to add user. Please try again.", MessageType.Error);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error: {ex.Message}", MessageType.Error);
            }
            finally
            {
                // Restore button
                var btnAdd = (Button)GetControl("btnAddUser");
                btnAdd.Text = "➕ ADD USER";
                btnAdd.Enabled = true;
            }
        }

        private async Task<bool> IsUsernameExists(string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT COUNT(1) FROM UserAccount WHERE Username = @Username";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    int count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        private async Task<bool> InsertUserToDatabase(int userid,string username, string password, string phone)
        {
            userID = userid;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"INSERT INTO UserAccount (UserID,Username, Password, PhoneNo) 
                             VALUES (@UserID,@Username, @Password, @PhoneNo)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userid);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password); // In real app, hash this!
                    command.Parameters.AddWithValue("@PhoneNo", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        private void ClearForm()
        {
            SetControlText("txtUsername", "");
            SetControlText("txtPassword", "");
            SetControlText("txtConfirmPassword", "");
            SetControlText("txtPhone", "");

            // Focus on username field
            GetControl("txtUsername")?.Focus();
        }

        private void ViewAllUsers()
        {
            var usersForm = new UsersListForm(connectionString);
            usersForm.ShowDialog();
        }

        // Helper methods to find controls
        private Control GetControl(string controlName)
        {
            var mainPanel = this.Controls[0];
            return FindControl(mainPanel, controlName);
        }

        private Control FindControl(Control parent, string controlName)
        {
            foreach (Control control in parent.Controls)
            {
                if (control.Name == controlName)
                    return control;

                if (control.HasChildren)
                {
                    var found = FindControl(control, controlName);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        private string GetControlText(string controlName)
        {
            var control = GetControl(controlName);
            return control?.Text ?? "";
        }

        private void SetControlText(string controlName, string text)
        {
            var control = GetControl(controlName);
            if (control != null)
                control.Text = text;
        }

        private void ShowMessage(string message, MessageType type)
        {
            Color color = type == MessageType.Success ? Color.FromArgb(76, 175, 80) :
                         type == MessageType.Error ? Color.FromArgb(244, 67, 54) :
                         Color.FromArgb(33, 150, 243);

            using (var msgForm = new MessageForm(message, type, color))
            {
                msgForm.ShowDialog();
            }
        }

        private void SignUpForm_Load(object sender, EventArgs e)
        {
            AutoGenerateUserID();
        }
    }

    public enum MessageType
    {
        Success,
        Error,
        Info
    }
}
