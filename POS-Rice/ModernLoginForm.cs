using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class ModernLoginForm : Form
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
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnClose;
        private Label lblStatus;
        private bool isPasswordVisible = false;

        public ModernLoginForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Form setup
            this.Text = "Project Rice - Login";
            this.Size = new Size(350, 450);
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
                Location = new Point(290, 10),
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
                Text = "LOGIN",
                Location = new Point(0, 60),
                Size = new Size(310, 40),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Subtitle
            Label lblSubtitle = new Label
            {
                Text = "Project Rice Management System",
                Location = new Point(0, 110),
                Size = new Size(310, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username label
            Label lblUsername = new Label
            {
                Text = "Username",
                Location = new Point(20, 160),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White
            };

            // Username panel
            Panel usernamePanel = new Panel
            {
                Location = new Point(20, 185),
                Size = new Size(290, 35),
                BackColor = Color.FromArgb(45, 45, 65),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Username textbox
            txtUsername = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(280, 25),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // Password label
            Label lblPassword = new Label
            {
                Text = "Password",
                Location = new Point(20, 230),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White
            };

            // Password panel
            Panel passwordPanel = new Panel
            {
                Location = new Point(20, 255),
                Size = new Size(290, 35),
                BackColor = Color.FromArgb(45, 45, 65),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password textbox
            txtPassword = new TextBox
            {
                Location = new Point(5, 5),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                UseSystemPasswordChar = true
            };

            // Show/Hide password button
            Button btnTogglePassword = new Button
            {
                Location = new Point(260, 5),
                Size = new Size(25, 25),
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 60, 80),
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand
            };
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.FlatAppearance.MouseOverBackColor = Color.FromArgb(80, 80, 100);

            btnTogglePassword.Click += (s, e) =>
            {
                isPasswordVisible = !isPasswordVisible;
                txtPassword.UseSystemPasswordChar = !isPasswordVisible;
                btnTogglePassword.Text = isPasswordVisible ? "🙈" : "👁";
                txtPassword.Focus(); // Keep focus on password field
            };

            

            usernamePanel.Controls.Add(txtUsername);
            passwordPanel.Controls.Add(txtPassword);
            passwordPanel.Controls.Add(btnTogglePassword);

            // Status label
            lblStatus = new Label
            {
                Location = new Point(20, 300),
                Size = new Size(290, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false
            };

            // Login button
            btnLogin = new Button
            {
                Text = "SIGN IN",
                Location = new Point(20, 330),
                Size = new Size(290, 45),
                BackColor = Color.FromArgb(86, 156, 214),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(66, 136, 194);
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(46, 116, 174);

            LinkLabel linkLbl = new LinkLabel
            {
                Text = "Forget Password",
                Location = new Point(20, 390),
                Size = new Size(290, 20),
                //BackColor = Color.FromArgb(86, 156, 214),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };


            // Add controls to form
            this.Controls.Add(btnClose);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(lblUsername);
            this.Controls.Add(usernamePanel);
            this.Controls.Add(lblPassword);
            this.Controls.Add(passwordPanel);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnLogin);
            this.Controls.Add(linkLbl);

            // Enable form dragging
            EnableFormDrag(this);
            EnableFormDrag(lblTitle);
            EnableFormDrag(lblSubtitle);
        }

        private void SetupEventHandlers()
        {
            btnClose.Click += (s, e) => Application.Exit();
            btnLogin.Click += BtnLogin_Click;

            // Enter key to login
            txtUsername.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    txtPassword.Focus();
                    e.Handled = true;
                }
            };

            txtPassword.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    BtnLogin_Click(null, null);
                    e.Handled = true;
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

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // Basic validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowStatus("Please enter both username and password", Color.Red);
                return;
            }

            // Disable UI during login attempt
            SetLoginUIState(false);
            lblStatus.Text = "Authenticating...";
            lblStatus.ForeColor = Color.Yellow;
            lblStatus.Visible = true;

            try
            {
                bool isAuthenticated = await Task.Run(() => AuthenticateUser(username, password));

                if (isAuthenticated)
                {
                    ShowStatus("Login successful! Redirecting...", Color.Green);
                    await Task.Delay(1000);

                    // Open main application form
                    OpenMainApplication();
                }
                else
                {
                    ShowStatus("Invalid username or password", Color.Red);
                    txtPassword.Focus();
                    txtPassword.SelectAll();
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", Color.Red);
            }
            finally
            {
                SetLoginUIState(true);
            }
        }

        private void linkLbl_Click(object sender, EventArgs e) 
        {
            ShowChangePasswordForm();
        }

        private void ShowChangePasswordForm()
        {
            ChangePasswordForm.ShowChangePassword();
        }
        
        private bool AuthenticateUser(string username, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to check if user exists with given credentials
                    string query = @"
                        SELECT COUNT(*) 
                        FROM UserAccount 
                        WHERE Username = @Username 
                        AND Password = @Password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Use parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        int userCount = Convert.ToInt32(command.ExecuteScalar());
                        return userCount > 0;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Handle specific SQL exceptions
                if (sqlEx.Number == -2) // Timeout
                {
                    throw new Exception("Database connection timeout");
                }
                else if (sqlEx.Number == 4060) // Cannot open database
                {
                    throw new Exception("Cannot connect to database. Please check the database name.");
                }
                else if (sqlEx.Number == 18456) // Login failed
                {
                    throw new Exception("Database login failed");
                }
                else
                {
                    throw new Exception($"Database error: {sqlEx.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Authentication error: {ex.Message}");
            }
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
            lblStatus.Visible = true;
        }

        private void SetLoginUIState(bool enabled)
        {
            btnLogin.Enabled = enabled;
            txtUsername.Enabled = enabled;
            txtPassword.Enabled = enabled;
            btnClose.Enabled = enabled;

            if (enabled)
            {
                Cursor = Cursors.Default;
                btnLogin.Text = "SIGN IN";
            }
            else
            {
                Cursor = Cursors.WaitCursor;
                btnLogin.Text = "PLEASE WAIT...";
            }
        }

        private void OpenMainApplication()
        {
            // Open your main application form here
            Main mainForm = new Main();
            mainForm.Show();
            this.Hide();
        }

        // Alternative: Custom TextBox with Transparent Background
        private class TransparentTextBox : TextBox
        {
            public TransparentTextBox()
            {
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                SetStyle(ControlStyles.Opaque, false);
                BackColor = Color.Transparent;
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // Don't paint background
            }
        }
    }

    // Placeholder for your main form
    public class MainForm : Form
    {
        public MainForm()
        {
            this.Text = "Project Rice - Main";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblWelcome = new Label
            {
                Text = "Welcome to Project Rice!",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 24, FontStyle.Bold)
            };

            this.Controls.Add(lblWelcome);
        }
    }
}