using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class BackupRestoreForm : Form
    {

        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
        private string backupFolder = @"C:\DatabaseBackups\"; // Default backup folder

        public BackupRestoreForm()
        {
          
            InitializeComponent();
            SetupForm();
            
            this.Text = "Database Backup & Restore";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
        
        }

        private void SetupForm()
        {
            CreateControls();

            // Create backup directory if it doesn't exist
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
        }

        private void CreateControls()
        {
            // Backup Section
            GroupBox backupGroup = new GroupBox
            {
                Text = "Database Backup",
                Location = new Point(20, 20),
                Size = new Size(550, 150),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            Label lblBackupPath = new Label
            {
                Text = "Backup Location:",
                Location = new Point(20, 30),
                Size = new Size(120, 20)
            };

            TextBox txtBackupPath = new TextBox
            {
                Location = new Point(150, 30),
                Size = new Size(300, 20),
                Text = backupFolder,
                ReadOnly = true,
                Name = "txtBackupPath"
            };

            Button btnBrowseBackup = new Button
            {
                Text = "Browse...",
                Location = new Point(460, 28),
                Size = new Size(70, 25),
                Name = "btnBrowseBackup"
            };

            Label lblBackupName = new Label
            {
                Text = "Backup Name:",
                Location = new Point(20, 70),
                Size = new Size(120, 20)
            };

            TextBox txtBackupName = new TextBox
            {
                Location = new Point(150, 70),
                Size = new Size(300, 20),
                Text = $"Rice_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak",
                Name = "txtBackupName"
            };

            Button btnBackup = new Button
            {
                Text = "Backup Now",
                Location = new Point(150, 105),
                Size = new Size(120, 35),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Name = "btnBackup"
            };

            Button btnOpenBackupFolder = new Button
            {
                Text = "Open Folder",
                Location = new Point(280, 105),
                Size = new Size(120, 35),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Name = "btnOpenBackupFolder"
            };

            backupGroup.Controls.AddRange(new Control[]
            {
                lblBackupPath, txtBackupPath, btnBrowseBackup,
                lblBackupName, txtBackupName, btnBackup, btnOpenBackupFolder
            });

            // Restore Section
            GroupBox restoreGroup = new GroupBox
            {
                Text = "Database Restore",
                Location = new Point(20, 190),
                Size = new Size(550, 150),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            Label lblRestoreFile = new Label
            {
                Text = "Backup File:",
                Location = new Point(20, 30),
                Size = new Size(120, 20)
            };

            TextBox txtRestoreFile = new TextBox
            {
                Location = new Point(150, 30),
                Size = new Size(300, 20),
                ReadOnly = true,
                Name = "txtRestoreFile"
            };

            Button btnBrowseRestore = new Button
            {
                Text = "Browse...",
                Location = new Point(460, 28),
                Size = new Size(70, 25),
                Name = "btnBrowseRestore"
            };

            CheckBox chkOverwrite = new CheckBox
            {
                Text = "Overwrite existing database",
                Location = new Point(150, 65),
                Size = new Size(200, 20),
                Checked = true,
                Name = "chkOverwrite"
            };

            Button btnRestore = new Button
            {
                Text = "Restore Now",
                Location = new Point(150, 100),
                Size = new Size(120, 35),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Name = "btnRestore"
            };

            Button btnTestRestore = new Button
            {
                Text = "Test Restore",
                Location = new Point(280, 100),
                Size = new Size(120, 35),
                BackColor = Color.Orange,
                ForeColor = Color.White,
                Name = "btnTestRestore"
            };

            restoreGroup.Controls.AddRange(new Control[]
            {
                lblRestoreFile, txtRestoreFile, btnBrowseRestore,
                chkOverwrite, btnRestore, btnTestRestore
            });

            // Add to form
            this.Controls.Add(backupGroup);
            this.Controls.Add(restoreGroup);

            // Event handlers
            btnBrowseBackup.Click += BtnBrowseBackup_Click;
            btnBackup.Click += BtnBackup_Click;
            btnOpenBackupFolder.Click += BtnOpenBackupFolder_Click;
            btnBrowseRestore.Click += BtnBrowseRestore_Click;
            btnRestore.Click += BtnRestore_Click;
            btnTestRestore.Click += BtnTestRestore_Click;
        }

        private void BtnBrowseBackup_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select backup folder";
            folderDialog.SelectedPath = backupFolder;
            folderDialog.ShowNewFolderButton = true;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                backupFolder = folderDialog.SelectedPath;
                GetTextBox("txtBackupPath").Text = backupFolder;
            }
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                string backupFileName = GetTextBox("txtBackupName").Text.Trim();

                if (string.IsNullOrEmpty(backupFileName))
                {
                    MessageBox.Show("Please enter a backup file name!", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Ensure .bak extension
                if (!backupFileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                {
                    backupFileName += ".bak";
                }

                string backupPath = Path.Combine(backupFolder, backupFileName);

                // Check if file already exists
                if (File.Exists(backupPath))
                {
                    DialogResult result = MessageBox.Show($"Backup file already exists:\n{backupPath}\n\nOverwrite?",
                                                         "File Exists",
                                                         MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No) return;
                }

                // Create backup
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string backupQuery = $"BACKUP DATABASE Rice TO DISK = '{backupPath}'";

                    using (SqlCommand cmd = new SqlCommand(backupQuery, conn))
                    {
                        conn.Open();

                        // Show progress
                        using (var progressForm = new ProgressForm("Creating Database Backup..."))
                        {
                            progressForm.Show();
                            Application.DoEvents();

                            cmd.CommandTimeout = 300; // 5 minutes timeout
                            cmd.ExecuteNonQuery();

                            progressForm.Close();
                        }
                    }
                }

                MessageBox.Show($"Database backup created successfully!\n\nLocation: {backupPath}",
                              "Backup Success",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Log the backup
                LogBackupOperation("BACKUP", backupPath, "Success");

            }
            catch (SqlException sqlEx)
            {
                string errorMessage = $"SQL Error creating backup:\n{sqlEx.Message}";
                if (sqlEx.Number == 18456) // Login failed
                {
                    errorMessage += "\n\nPlease check your SQL Server authentication.";
                }
                MessageBox.Show(errorMessage, "Backup Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogBackupOperation("BACKUP", "", $"Failed: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup:\n{ex.Message}", "Backup Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogBackupOperation("BACKUP", "", $"Failed: {ex.Message}");
            }
        }

        private void BtnOpenBackupFolder_Click(object sender, EventArgs e)
        {
            try
            {
                if (Directory.Exists(backupFolder))
                {
                    Process.Start("explorer.exe", backupFolder);
                }
                else
                {
                    MessageBox.Show("Backup folder does not exist!", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening folder: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBrowseRestore_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select database backup file";
            fileDialog.Filter = "Backup Files (*.bak)|*.bak|All Files (*.*)|*.*";
            fileDialog.InitialDirectory = backupFolder;
            fileDialog.CheckFileExists = true;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                GetTextBox("txtRestoreFile").Text = fileDialog.FileName;
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            string backupFile = GetTextBox("txtRestoreFile").Text.Trim();

            if (string.IsNullOrEmpty(backupFile) || !File.Exists(backupFile))
            {
                MessageBox.Show("Please select a valid backup file!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Warning message
            DialogResult confirmResult = MessageBox.Show(
                "WARNING: Restoring will overwrite the current database.\n" +
                "All current data will be replaced with backup data.\n\n" +
                "Are you sure you want to continue?\n\n" +
                $"Backup File: {Path.GetFileName(backupFile)}",
                "Confirm Restore",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes) return;

            try
            {
                // Get exclusive access to database
                string masterConnectionString = @"Server=.\SQLEXPRESS;Database=master;Integrated Security=true;";

                using (SqlConnection masterConn = new SqlConnection(masterConnectionString))
                {
                    masterConn.Open();

                    // Set database to single user mode
                    using (SqlCommand singleUserCmd = new SqlCommand(
                        "ALTER DATABASE Rice SET SINGLE_USER WITH ROLLBACK IMMEDIATE", masterConn))
                    {
                        singleUserCmd.ExecuteNonQuery();
                    }

                    // Perform restore
                    string restoreQuery = $@"
                    RESTORE DATABASE Rice 
                    FROM DISK = '{backupFile}'
                    WITH REPLACE, RECOVERY, 
                    STATS = 10"; // Show progress every 10%

                    using (SqlCommand restoreCmd = new SqlCommand(restoreQuery, masterConn))
                    {
                        restoreCmd.CommandTimeout = 600; // 10 minutes timeout

                        // Show progress
                        using (var progressForm = new ProgressForm("Restoring Database..."))
                        {
                            progressForm.Show();
                            Application.DoEvents();

                            restoreCmd.ExecuteNonQuery();

                            progressForm.Close();
                        }
                    }

                    // Set back to multi-user mode
                    using (SqlCommand multiUserCmd = new SqlCommand(
                        "ALTER DATABASE Rice SET MULTI_USER", masterConn))
                    {
                        multiUserCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"Database restored successfully!\n\n" +
                              $"Restored from: {Path.GetFileName(backupFile)}\n\n" +
                              "Application will now close. Please restart the application.",
                              "Restore Success",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Log the restore
                LogBackupOperation("RESTORE", backupFile, "Success");

                // Close application
                Application.Exit();

            }
            catch (SqlException sqlEx)
            {
                // Try to set database back to multi-user mode if error occurs
                try
                {
                    using (SqlConnection masterConn = new SqlConnection(
                        @"Server=.\SQLEXPRESS;Database=master;Integrated Security=true;"))
                    {
                        masterConn.Open();
                        using (SqlCommand multiUserCmd = new SqlCommand(
                            "ALTER DATABASE Rice SET MULTI_USER", masterConn))
                        {
                            multiUserCmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }

                MessageBox.Show($"SQL Error during restore:\n{sqlEx.Message}\n\n" +
                              "Database has been set back to multi-user mode.",
                              "Restore Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogBackupOperation("RESTORE", backupFile, $"Failed: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during restore:\n{ex.Message}", "Restore Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogBackupOperation("RESTORE", backupFile, $"Failed: {ex.Message}");
            }
        }

        private void BtnTestRestore_Click(object sender, EventArgs e)
        {
            string backupFile = GetTextBox("txtRestoreFile").Text.Trim();

            if (string.IsNullOrEmpty(backupFile) || !File.Exists(backupFile))
            {
                MessageBox.Show("Please select a valid backup file!", "Validation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Test restore to verify backup file is valid
                string masterConnectionString = @"Server=.\SQLEXPRESS;Database=master;Integrated Security=true;";

                using (SqlConnection masterConn = new SqlConnection(masterConnectionString))
                {
                    masterConn.Open();

                    // Just verify the backup file
                    string verifyQuery = $"RESTORE VERIFYONLY FROM DISK = '{backupFile}'";

                    using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, masterConn))
                    {
                        using (var progressForm = new ProgressForm("Verifying Backup File..."))
                        {
                            progressForm.Show();
                            Application.DoEvents();

                            verifyCmd.ExecuteNonQuery();

                            progressForm.Close();
                        }
                    }
                }

                // Get backup information
                string infoQuery = $"RESTORE HEADERONLY FROM DISK = '{backupFile}'";
                string backupInfo = GetBackupInformation(infoQuery, backupFile);

                MessageBox.Show($"Backup file is valid and ready for restore!\n\n" +
                              $"File: {Path.GetFileName(backupFile)}\n" +
                              $"Size: {new FileInfo(backupFile).Length / 1024 / 1024} MB\n" +
                              backupInfo,
                              "Backup Verification",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup file verification failed!\n\nError: {ex.Message}\n\n" +
                              "The backup file may be corrupted or in wrong format.",
                              "Verification Failed",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetBackupInformation(string query, string backupFile)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(
                    @"Server=.\SQLEXPRESS;Database=master;Integrated Security=true;"))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return $"Database: {reader["DatabaseName"]}\n" +
                                   $"Backup Date: {reader["BackupStartDate"]}\n" +
                                   $"Type: {(reader["BackupType"]?.ToString() == "1" ? "Full" : "Other")}";
                        }
                    }
                }
            }
            catch { }

            return "Additional information not available.";
        }

        private void LogBackupOperation(string operation, string filePath, string status)
        {
            try
            {
                string logFile = Path.Combine(backupFolder, "BackupLog.txt");
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {operation} | " +
                                 $"File: {Path.GetFileName(filePath)} | Status: {status}\n";

                File.AppendAllText(logFile, logEntry);
            }
            catch { }
        }

        // Helper method to find controls
        private TextBox GetTextBox(string name)
        {
            return this.Controls.Find(name, true).FirstOrDefault() as TextBox;
        }

        private CheckBox GetCheckBox(string name)
        {
            return this.Controls.Find(name, true).FirstOrDefault() as CheckBox;
        }
    }

    // Progress Form for long operations
    public class ProgressForm : Form
    {
        public ProgressForm(string message)
        {
            this.Text = "Please Wait";
            this.Size = new Size(300, 100);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            Label lblMessage = new Label
            {
                Text = message,
                Location = new Point(20, 20),
                Size = new Size(260, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10)
            };

            ProgressBar progressBar = new ProgressBar
            {
                Location = new Point(20, 60),
                Size = new Size(260, 20),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30
            };

            this.Controls.Add(lblMessage);
            this.Controls.Add(progressBar);
        }
    }

}