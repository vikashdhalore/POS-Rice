using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace POS_Rice
{
    public partial class UsersListForm : Form
    {
        private string connectionString;

        public UsersListForm(string connString)
        {
            connectionString = connString;
            InitializeComponents();
            LoadUsers();
        }

        private void InitializeComponents()
        {
            this.Text = "POS Rice - All Users";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(25, 25, 35);

            var mainPanel = new Panel
            {
                Size = new Size(580, 380),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(35, 35, 45)
            };

            var lblTitle = new Label
            {
                Text = "All Users",
                Location = new Point(0, 20),
                Size = new Size(580, 30),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var dataGrid = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(540, 250),
                BackgroundColor = Color.FromArgb(45, 45, 55),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Name = "dgvUsers",
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Style the datagrid
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 60);
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGrid.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 55);
            dataGrid.DefaultCellStyle.ForeColor = Color.White;
            dataGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dataGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 50);

            var btnClose = new Button
            {
                Text = "CLOSE",
                Location = new Point(250, 330),
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(dataGrid);
            mainPanel.Controls.Add(btnClose);

            this.Controls.Add(mainPanel);
        }

        private void LoadUsers()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT UserID, Username, PhoneNo FROM UserAccount ORDER BY UserID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        var dataGrid = (DataGridView)GetControl("dgvUsers");
                        dataGrid.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
    }
}