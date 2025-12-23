using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace POS_Rice
{
    public partial class Products : Form
    {
        private int selectedPartyID = 0;
        private string selectedPartyName = "";
        
        public Products()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            // Check if product is selected
            if (selectedPartyID == 0)
            {
                MessageBox.Show("Please select a party!");
                return;
            }

            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                SqlConnection conn = new SqlConnection(connectionString);
                {
                    string query = @"INSERT INTO Product 
                           (ProductID, ProductName, Brand, PartyID) 
                           VALUES 
                           (@ProductID, @ProductName, @Brand, @PartyID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use the stored selectedPartyID variable
                        cmd.Parameters.AddWithValue("@ProductID", Convert.ToInt32(textBox1.Text));
                        cmd.Parameters.AddWithValue("@ProductName", textBox2.Text.Trim());
                        cmd.Parameters.AddWithValue("@Brand", textBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@PartyID", selectedPartyID);

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Product record added successfully!");
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add product record!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding product: " + ex.Message);
            }

            AutoGenerateProductID();
            LoadProductsData();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
       /* private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e) 
        {
            try
            {
                // Ignore clicks on checkbox column and header row
                if (e.RowIndex >= 0 && e.ColumnIndex != 0)
                {
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    if (row.Cells["Product ID"].Value != null) 
                    {
                        // Populate form fields with selected row data
                        textBox1.Text = row.Cells["Product ID"].Value?.ToString() ?? "";
                        textBox2.Text = row.Cells["Product Name"].Value?.ToString() ?? "";
                        textBox3.Text = row.Cells["Brand"].Value?.ToString() ?? "";
                    }

                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting product: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }*/
        
        private void Form1_Load(object sender, EventArgs e)
        {
            
            // Configure DataGridView properties
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false; // Single row selection
            dataGridView1.ReadOnly = true; // Make data cells read-only
            dataGridView1.RowHeadersVisible = true; // Show row headers for better selection

            // Make only checkbox column editable
            if (dataGridView1.Columns.Contains("CheckBoxColumn"))
            {
                dataGridView1.Columns["CheckBoxColumn"].ReadOnly = false;
            }

           // AddCheckBoxColumn();
            LoadProductsData();
            LoadParty();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a row is selected in DataGridView
                if (dataGridView1.CurrentRow == null)
                {
                    MessageBox.Show("Please select a product to update", "Selection Required",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the selected product ID from DataGridView
                int productID = Convert.ToInt32(dataGridView1.CurrentRow.Cells["ProductID"].Value);

                // Validate form fields
                if (string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
                {
                    MessageBox.Show("Please fill in all fields", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if(comboPartyName.SelectedIndex == 0) 
                {
                    MessageBox.Show("Please select any of the to update", "Validation Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create connection string
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                // SQL update query
                string query = "UPDATE Product SET ProductName = @ProductName, Brand = @Brand, PartyID = @PartyID WHERE ProductID = @ProductID";

                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters
                    command.Parameters.AddWithValue("@ProductName", textBox2.Text.Trim());
                    command.Parameters.AddWithValue("@Brand", textBox3.Text.Trim());
                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@PartyID", selectedPartyID);
                    // Open connection and execute query
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Product updated successfully!", "Success",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh DataGridView to show updated data
                        ClearForm();
                        AutoGenerateProductID();
                        LoadProductsData();
                        LoadParty();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update product", "Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating product: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ClearForm()
        {
                 selectedPartyID = 0;
                 selectedPartyName = "";

                //textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
            if(comboPartyName.Items.Count > 0) 
            {
                comboPartyName.SelectedIndex = 0;
                textBox2.Focus();
            }
            //comboPartyName.Items.Clear();
            
        }

        private void LoadParty()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            string query = "SELECT PartyID, PartyName FROM Party ORDER BY PartyName";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        // Clear and add items manually
                        comboPartyName.Items.Clear();
                        comboPartyName.Items.Add("-- Select Party --");

                        while (reader.Read())
                        {
                            comboPartyName.Items.Add(new
                            {
                                Display = reader["PartyName"].ToString(),
                                Value = reader["PartyID"].ToString()
                            });
                        }

                        comboPartyName.DisplayMember = "Display";
                        comboPartyName.ValueMember = "Value";
                        comboPartyName.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
        }

        private bool ValidateInputs()
        {
            if (!int.TryParse(textBox1.Text, out _))
            {
                MessageBox.Show("Please enter valid number for Product ID!");
                return false;
            }

            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please enter Product Name!");
                return false;
            }

            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("Please enter Brand!");
                return false;
            }

            if (comboPartyName.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a party!");
                return false;
            }

            return true;
        }
        private void AutoGenerateProductID()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                
                conn.Open();
                SqlCommand cmd = new SqlCommand("Select ISNULL(MAX(ProductID), 0) + 1 from Product", conn);
                int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                textBox1.Text = nextID.ToString();
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error generating Product ID: "+ ex.Message);
            }
            finally 
            {
                conn.Close();
                textBox2.Focus();
            }
            
        }

        /*private void AddCheckBoxColumn()
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
        */
        
        private void LoadData(string search ="") 
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString); 
            
                try
                {

                conn.Open();
                    

                    // SQL select query
                    string query = "SELECT ProductID, ProductName, Brand FROM Product WHERE ProductName LIKE @search Or Brand LIKE @search";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                // Create connection and data adapter               
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);

                    
                        // Create DataTable to hold the data
                        DataTable dataTable = new DataTable();

                        // Fill DataTable with data from database
                        dataAdapter.Fill(dataTable);

                        // Set DataGridView data source
                        dataGridView1.DataSource = dataTable;

                        // Optional: Configure column headers
                        dataGridView1.Columns["ProductID"].HeaderText = "Product ID";
                        dataGridView1.Columns["ProductName"].HeaderText = "Product Name";
                        dataGridView1.Columns["Brand"].HeaderText = "Brand";

                        // Optional: Auto-size columns
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                        // Add checkbox column after data binding
                        //AddCheckBoxColumn();
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            
        }

        private void LoadProductsData() 
        {
            try
            {
                if (dataGridView1.Columns.Contains("CheckBoxColumn"))
                {
                    dataGridView1.Columns.Remove("CheckBoxColumn");
                }

                // Create connection string - update with your database details
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                // SQL select query
                string query = "select P.ProductID, P.ProductName,P.Brand,PT.PartyName from Product P INNER JOIN Party PT ON P.PartyID = PT.PartyID";

                // Create connection and data adapter
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
                {
                    // Create DataTable to hold the data
                    DataTable dataTable = new DataTable();

                    // Fill DataTable with data from database
                    dataAdapter.Fill(dataTable);

                    // Set DataGridView data source
                    dataGridView1.DataSource = dataTable;

                    // Optional: Configure column headers
                    dataGridView1.Columns["ProductID"].HeaderText = "Product ID";
                    dataGridView1.Columns["ProductName"].HeaderText = "Product Name";
                    dataGridView1.Columns["Brand"].HeaderText = "Brand";
                    dataGridView1.Columns["PartyName"].HeaderText = "Party";

                    // Optional: Auto-size columns
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Add checkbox column after data binding
                    //AddCheckBoxColumn();
                    ClearForm();
                    AutoGenerateProductID();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void deleteProduct_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Refresh data when remove form is closed
            ClearForm();
            AutoGenerateProductID();
            LoadProductsData();
            LoadParty();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            deleteProduct deltProduct = new deleteProduct();
            deltProduct.FormClosed += deleteProduct_FormClosed;
            deltProduct.Show();
        }


        /*private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkSelectAll.Checked;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["CheckBoxColumn"].Value = isChecked;
            }
        }*/

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            LoadData(textSearch.Text.Trim());
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            ClearForm();
            AutoGenerateProductID();
            LoadProductsData();
            LoadParty();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void comboPartyName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboPartyName.SelectedIndex > 0)
                {
                    // Get the selected item
                    dynamic selectedItem = comboPartyName.SelectedItem;

                    // Extract ProductID, ProductName, and Brand
                    selectedPartyID = Convert.ToInt32(selectedItem.Value);
                    selectedPartyName = selectedItem.Display.ToString();
                    //selectedBrand = selectedItem.Brand.ToString();

                    // Display the brand in comboBoxBrand
                    //comboBoxBrand.Text = selectedBrand;

                    //LoadBrands();

                    // Optional: If you want to filter brands based on selected product
                    // FilterBrandsByProduct(selectedProductID);

                    // Display selected information (for debugging)
                    //DisplaySelectedProductInfo();
                }
                else
                {
                    // Reset values when "Select Product" is chosen
                    selectedPartyID = 0;
                    selectedPartyName = "";
                    //selectedBrand = "";
                    //comboBoxBrand.Text = "-- Select Brand --";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting product: " + ex.Message);
            }
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Check if a row is selected and has data
                if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Cells["ProductID"].Value != null)
                {
                    DataGridViewRow row = dataGridView1.CurrentRow;

                    // Populate form fields with selected row data
                    textBox1.Text = row.Cells["ProductID"].Value.ToString();
                    textBox2.Text = row.Cells["ProductName"].Value?.ToString() ?? "";
                    textBox3.Text = row.Cells["Brand"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting product: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}

