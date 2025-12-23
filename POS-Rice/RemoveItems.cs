using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
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
    public partial class RemoveItems : Form
    {
        private string _orderID;
        public RemoveItems(string OrderID)
        {
            InitializeComponent();
            _orderID = OrderID; // Store the parameter
        }

        private void ItemGridView(string orderId)
        {
            try
            {
                if (dataGridView1.Columns.Contains("CheckBoxColumn"))
                {
                    dataGridView1.Columns.Remove("CheckBoxColumn");
                }

                // Create connection string - update with your database details
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                SqlConnection connection = new SqlConnection(connectionString);
                // SQL select query
                SqlCommand cmd = new SqlCommand("select S.SP_ID, Pt.PartyName,P.ProductName,P.Brand,S.QtyBag,S.TotalQtyBags,S.RemBags,S.Weight,S.Rate,S.Amount from SaleProduct S left join product P on S.ProductID = P.ProductID left join Party Pt on S.PartyID = Pt.PartyID Where OrderID = @OrderID", connection);

                cmd.Parameters.AddWithValue("OrderID", orderId);

                // Create connection and data adapter

                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))

                {
                    // Create DataTable to hold the data
                    DataTable dataTable = new DataTable();

                    // Fill DataTable with data from database
                    dataAdapter.Fill(dataTable);

                    // Set DataGridView data source
                    dataGridView1.DataSource = dataTable;

                    // Optional: Configure column headers
                    dataGridView1.Columns["SP_ID"].HeaderText = "Item No";
                    dataGridView1.Columns["PartyName"].HeaderText = "Party";
                    dataGridView1.Columns["ProductName"].HeaderText = "Product Name";
                    dataGridView1.Columns["Brand"].HeaderText = "Brand";
                    dataGridView1.Columns["QtyBag"].HeaderText = "Bags";
                    dataGridView1.Columns["TotalQtyBags"].HeaderText = "Total Lat";
                    dataGridView1.Columns["RemBags"].HeaderText = "Remaining Bags";
                    dataGridView1.Columns["Weight"].HeaderText = "Weight";
                    dataGridView1.Columns["Rate"].HeaderText = "Rate";
                    dataGridView1.Columns["Amount"].HeaderText = "Amount";



                    // Optional: Auto-size columns
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Add checkbox column after data binding
                    AddCheckBoxColumn();
                    //ItemClear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddCheckBoxColumn()
        {
            // Remove existing checkbox column to avoid duplicates
            if (dataGridView1.Columns.Contains("CheckBoxColumn"))
            {
                dataGridView1.Columns.Remove("CheckBoxColumn");
            }

            // Create checkbox column
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Select";
            checkBoxColumn.Name = "CheckBoxColumn";
            checkBoxColumn.Width = 50;
            checkBoxColumn.ReadOnly = false; // Allow checking/unchecking

            // Add checkbox column as first column
            dataGridView1.Columns.Insert(0, checkBoxColumn);
        }

        private void RemoveItems_Load(object sender, EventArgs e)
        {
            // Use the stored OrderID parameter here
            if (!string.IsNullOrEmpty(_orderID))
            {
                ItemGridView(_orderID);

                // Optional: Display the OrderID in the form title or a label
                this.Text = $"Remove Items - Order # {_orderID}";
            }
            else
            {
                MessageBox.Show("No Order ID provided!", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close(); // Close the form if no OrderID
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected rows
                var selectedRows = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .Where(row => Convert.ToBoolean(row.Cells["CheckBoxColumn"].Value) == true)
                    .ToList();

                if (selectedRows.Count == 0)
                {
                    MessageBox.Show("Please select at least one customer to delete", "Selection Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm deletion
                DialogResult result = MessageBox.Show($"Are you sure you want to delete {selectedRows.Count} Sale Items(s)?",
                                                    "Confirm Delete",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Create connection string
                    string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        foreach (DataGridViewRow row in selectedRows)
                        {
                            int SPID = Convert.ToInt32(row.Cells["SP_ID"].Value);

                            string query = "DELETE FROM SaleProduct WHERE SP_ID = @SP_ID";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@SP_ID", SPID);
                                command.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show($"{selectedRows.Count} Item(s) deleted successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView
                        ItemGridView(_orderID);

                        // Clear form fields

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting products: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
