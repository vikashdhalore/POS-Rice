using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class RemoveCustCash : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
        public RemoveCustCash()
        {
            InitializeComponent();
        }

        private void LoadCashBookData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                            CashID,
                            Cash_Entry,
                            Date,
                            CustID
                        FROM CashBook 
                        ORDER BY CashID DESC";

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridViewCustCash.DataSource = dt;

                        // Format columns
                        dataGridViewCustCash.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridViewCustCash.Columns["CashID"].HeaderText = "Cash ID";
                        dataGridViewCustCash.Columns["Cash_Entry"].HeaderText = "Cash Amount";
                        dataGridViewCustCash.Columns["Date"].HeaderText = "Cash Date";
                        dataGridViewCustCash.Columns["CustID"].HeaderText = "Customer ID";

                        // Format numeric column
                        if (dataGridViewCustCash.Columns["Cash_Entry"] != null)
                            dataGridViewCustCash.Columns["Cash_Entry"].DefaultCellStyle.Format = "N2";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading cash book data: {ex.Message}");
            }
        }

        private void AddCheckBoxColumn()
        {
            // Remove existing checkbox column to avoid duplicates
            if (dataGridViewCustCash.Columns.Contains("CheckBoxColumn"))
            {
                dataGridViewCustCash.Columns.Remove("CheckBoxColumn");
            }

            // Create checkbox column
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Select";
            checkBoxColumn.Name = "CheckBoxColumn";
            checkBoxColumn.Width = 50;
            checkBoxColumn.ReadOnly = false; // Allow checking/unchecking

            // Add checkbox column as first column
            dataGridViewCustCash.Columns.Insert(0, checkBoxColumn);
        }
        private void RemoveCustCash_Load(object sender, EventArgs e)
        {
            // Configure DataGridView properties
            dataGridViewCustCash.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewCustCash.MultiSelect = false; // Single row selection
            dataGridViewCustCash.ReadOnly = false; // Make data cells read-only
            dataGridViewCustCash.RowHeadersVisible = false; // Show row headers for better selection

            // Make only checkbox column editable
            if (dataGridViewCustCash.Columns.Contains("CheckBoxColumn"))
            {
                dataGridViewCustCash.Columns["CheckBoxColumn"].ReadOnly = false;
            }
            AddCheckBoxColumn();
            LoadCashBookData();
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkSelectAll.Checked;

            foreach (DataGridViewRow row in dataGridViewCustCash.Rows)
            {
                row.Cells["CheckBoxColumn"].Value = isChecked;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected rows
                var selectedRows = dataGridViewCustCash.Rows
                    .Cast<DataGridViewRow>()
                    .Where(row => Convert.ToBoolean(row.Cells["CheckBoxColumn"].Value) == true)
                    .ToList();

                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("Please select at least one cash entry to delete", "Selection Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion
                DialogResult result = MessageBox.Show($"Are you sure you want to delete {selectedRows.Count} cash entry?",
                                                    "Confirm Delete",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Create connection string
                    //string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        foreach (DataGridViewRow row in selectedRows)
                        {
                            int cashID = Convert.ToInt32(row.Cells["CashID"].Value);

                            string query = "DELETE FROM Cashbook WHERE CashID = @CashID";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@CashID", cashID);
                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show($"{selectedRows.Count} Cash entry(s) deleted successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView
                        AddCheckBoxColumn();
                        LoadCashBookData();

                        // Clear form fields

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting cash entry: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
