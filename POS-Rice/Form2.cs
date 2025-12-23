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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace POS_Rice
{
    public partial class InventoryForm : Form
    {
        string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
        private int selectedProductID = 0;
        private string selectedProductName = "";
        private string selectedBrand = "";
        private int partyID = 0;
       // private double credit = 0.0;
        //private double debit = 0.0;

        public InventoryForm()
        {
            InitializeComponent(); 
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //AutoGenerateInventory();
            
            LoadParty();
            LoadProInventoryDataGridView();

            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void AutoGenerateInventory() 
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("Select ISNULL(MAX(InventoryID), 0) + 1 from ProInventory", conn);
                int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                textInventoryID.Text = nextID.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Inventory ID: " + ex.Message);
            }
            finally
            {
                conn.Close();
                textInventoryID.Focus();
            }
        }

        private void LoadParty()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            string query = "SELECT PartyID, PartyName FROM Party";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        // Clear and add items manually
                        comboParty.Items.Clear();
                        comboParty.Items.Add("-- Select Party --");

                        while (reader.Read())
                        {
                            comboParty.Items.Add(new
                            {
                                Display = reader["PartyName"].ToString(),
                                Value = reader["PartyID"].ToString()
                            });
                        }

                        comboParty.DisplayMember = "Display";
                        comboParty.ValueMember = "Value";
                        comboParty.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading parties: " + ex.Message);
            }
        }
        private void LoadProducts(int partyID)
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            string query = "SELECT ProductID, ProductName, Brand FROM Product Where PartyID =@PartyID ORDER BY ProductName";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                         cmd.Parameters.AddWithValue("@PartyID", partyID);

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        // Clear and add items manually
                        comboBoxProductName.Items.Clear();
                        comboBoxProductName.Items.Add("-- Select Product --");

                        while (reader.Read())
                        {
                            comboBoxProductName.Items.Add(new
                            {
                                Display = reader["ProductName"].ToString(),
                                Value = reader["ProductID"].ToString(),
                                Brand = reader["Brand"].ToString()
                            });
                            //partyID = Convert.ToInt32(comboParty.SelectedIndex);
                        }

                        comboBoxProductName.DisplayMember = "Display";
                        comboBoxProductName.ValueMember = "Value";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
        }

        private void LoadProInventoryDataGridView()
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
                string query = "select I.InventoryID, I.InvDate, I.LaatNo, I.Bags, I.Weight, I.Rate, I.Amount, I.Credit, I.Debit,I.cash,Pt.PartyID, Pt.PartyName, P.ProductID,P.ProductName, P.Brand from ProInventory I left join product P on I.ProductID = P.ProductID left join Party Pt on I.PartyID = Pt.PartyID";

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
                    dataGridView1.Columns["InventoryID"].HeaderText = "Inventory ID";
                    dataGridView1.Columns["PartyName"].HeaderText = "Party";
                    dataGridView1.Columns["ProductName"].HeaderText = "Product Name";
                    dataGridView1.Columns["Brand"].HeaderText = "Brand";
                    dataGridView1.Columns["InvDate"].HeaderText = "Date";
                    dataGridView1.Columns["LaatNo"].HeaderText = "Lat No";
                    dataGridView1.Columns["Bags"].HeaderText = "Bags";
                    dataGridView1.Columns["Weight"].HeaderText = "Weight";
                    dataGridView1.Columns["Rate"].HeaderText = "Rate";
                    dataGridView1.Columns["Amount"].HeaderText = "Amount";
                    dataGridView1.Columns["Credit"].HeaderText = "Credit";
                    dataGridView1.Columns["cash"].HeaderText = "Cash";


                    // Optional: Auto-size columns
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Add checkbox column after data binding
                    //AddCheckBoxColumn();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadBrands()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            string query = "SELECT DISTINCT Brand FROM Product WHERE Brand IS NOT NULL ORDER BY Brand";

            try
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                
                 // Clear and add items manually
                 // comboBoxBrand.Items.Clear();
                 //comboBoxBrand.Items.Add("-- Select Brand --");
                
                while (reader.Read())
                {
                  //  comboBoxBrand.Items.Add(reader["Brand"].ToString());
                }
                
                //comboBoxBrand.SelectedIndex = 0;
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show("Error loading brands: " + ex.Message);
    }

        }

        /*  private int GetSelectedProductID()
          {
              if (comboBoxProductName.SelectedIndex > 0)
              {
                  dynamic selectedItem = comboBoxProductName.SelectedItem;
                  return Convert.ToInt32(selectedItem.ProductID);
              }
              return 0;
          }
        */
        private bool ValidateInputs()
        {
            if (comboBoxProductName.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a product!");
                return false;
            }

            if (Status.SelectedIndex <= 0) 
            {
                MessageBox.Show("Please select a Status of Credit Or Debit!");
                return false;
            }

            if (comboParty.SelectedIndex <= 0) 
            {
                MessageBox.Show("Please select parties!");
                return false;
            }

            if (string.IsNullOrEmpty(textLaatNo.Text))
            {
                MessageBox.Show("Please enter Lat No!");
                return false;
            }

            // Validate numeric fields
            if (!int.TryParse(textBags.Text, out _))
            {
                MessageBox.Show("Please enter valid number for Bags!");
                return false;
            }

            if (!decimal.TryParse(textWeight.Text, out _))
            {
                MessageBox.Show("Please enter valid number for Weight!");
                return false;
            }

            if (!decimal.TryParse(textRate.Text, out _))
            {
                MessageBox.Show("Please enter valid number for Rate!");
                return false;
            }

            if (!decimal.TryParse(textAmount.Text, out _))
            {
                MessageBox.Show("Please enter valid number for Amount!");
                return false;
            }

            if (!decimal.TryParse(textCredit.Text, out _))
            {
                MessageBox.Show("Please enter valid number for Credit!");
                return false;
            }

            if (!decimal.TryParse(textDebit.Text, out _))
            {
                MessageBox.Show("Please enter valid number for Debit!");
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            dateTimePickerDate.Value = DateTime.Now;
            textLaatNo.Clear();
            textBrand.Clear();
            textBags.Text = "0";
            textWPB.Text = "0";
            textWeight.Text = "0";
            textRate.Text = "0";
            textAmount.Text = "0";
            textCredit.Text = "0";
            textDebit.Text = "0";
            textCash.Text = "0";
            comboBoxProductName.SelectedIndex = 0;
            comboParty.SelectedIndex = 0;
            Status.SelectedIndex = 0;
            AutoGenerateInventory();
            textLaatNo.Focus();
        }

        private void LoadData(string search = "")
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // SQL select query for inventory with joins to get related data
                    string query = @"SELECT 
                           pi.InventoryID,
                           pi.InvDate,
                           pi.LaatNo,
                           pi.Bags,
                           pi.Weight,
                           pi.Rate,
                           pi.Amount,
                           pi.Credit,
                           pi.Debit,
                           p.ProductName,
                           p.Brand,
                           pt.PartyName
                       FROM ProInventory pi
                       INNER JOIN Product p ON pi.ProductID = p.ProductID
                       INNER JOIN Party pt ON pi.PartyID = pt.PartyID
                       WHERE pi.LaatNo LIKE @search 
                          OR p.ProductName LIKE @search 
                          OR p.Brand LIKE @search 
                          OR pt.PartyName LIKE @search";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd))
                        {
                            // Create DataTable to hold the data
                            DataTable dataTable = new DataTable();

                            // Fill DataTable with data from database
                            dataAdapter.Fill(dataTable);

                            // Set DataGridView data source
                            dataGridView1.DataSource = dataTable;

                            // Configure column headers
                            dataGridView1.Columns["InventoryID"].HeaderText = "Inventory ID";
                            dataGridView1.Columns["InvDate"].HeaderText = "Date";
                            dataGridView1.Columns["LaatNo"].HeaderText = "Laat No";
                            dataGridView1.Columns["Bags"].HeaderText = "Bags";
                            dataGridView1.Columns["Weight"].HeaderText = "Weight";
                            dataGridView1.Columns["Rate"].HeaderText = "Rate";
                            dataGridView1.Columns["Amount"].HeaderText = "Amount";
                            dataGridView1.Columns["Credit"].HeaderText = "Credit";
                            dataGridView1.Columns["Debit"].HeaderText = "Debit";
                            dataGridView1.Columns["ProductName"].HeaderText = "Product Name";
                            dataGridView1.Columns["Brand"].HeaderText = "Brand";
                            dataGridView1.Columns["PartyName"].HeaderText = "Party Name";

                            // Optional: Auto-size columns
                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                            // Optional: Format columns
                            if (dataGridView1.Columns["InvDate"] != null)
                                dataGridView1.Columns["InvDate"].DefaultCellStyle.Format = "dd/MM/yyyy";

                            if (dataGridView1.Columns["Weight"] != null)
                                dataGridView1.Columns["Weight"].DefaultCellStyle.Format = "N2";

                            if (dataGridView1.Columns["Rate"] != null)
                                dataGridView1.Columns["Rate"].DefaultCellStyle.Format = "N2";

                            if (dataGridView1.Columns["Amount"] != null)
                                dataGridView1.Columns["Amount"].DefaultCellStyle.Format = "N2";

                            if (dataGridView1.Columns["Credit"] != null)
                                dataGridView1.Columns["Credit"].DefaultCellStyle.Format = "N2";

                            if (dataGridView1.Columns["Debit"] != null)
                                dataGridView1.Columns["Debit"].DefaultCellStyle.Format = "N2";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading inventory data: {ex.Message}", "Database Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void RestoreProductSelection(int productID)
        {
            for (int i = 1; i < comboBoxProductName.Items.Count; i++)
            {
                dynamic item = comboBoxProductName.Items[i];
                if (Convert.ToInt32(item.ProductID) == productID)
                {
                    comboBoxProductName.SelectedIndex = i;
                    break;
                }
            }
        }

        private void DisplaySelectedProductInfo()
        {
            // You can use this for debugging or display purposes
            Console.WriteLine($"Selected Product: ID={selectedProductID}, Name={selectedProductName}, Brand={selectedBrand}");

            // Optional: Display in status label or tooltip
            // lblStatus.Text = $"Selected: {selectedProductName} - {selectedBrand} (ID: {selectedProductID})";
        }

        // Method to filter products by brand
        private void FilterProductsByBrand(string brand)
        {
            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                SqlConnection conn = new SqlConnection(connectionString);
                {
                    string query = "SELECT ProductID, ProductName, Brand FROM Product WHERE Brand = @Brand ORDER BY ProductName";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Brand", brand);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        comboBoxProductName.Items.Clear();
                        comboBoxProductName.Items.Add("-- Select Product --");

                        while (reader.Read())
                        {
                            string productInfo = $"{reader["ProductName"]} - {reader["Brand"]}";
                            comboBoxProductName.Items.Add(new
                            {
                                Display = productInfo,
                                ProductID = reader["ProductID"],
                                ProductName = reader["ProductName"],
                                Brand = reader["Brand"]
                            });
                        }

                        comboBoxProductName.DisplayMember = "Display";
                        comboBoxProductName.ValueMember = "ProductID";
                        comboBoxProductName.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering products by brand: " + ex.Message);
            }
        }

        // Method to filter brands by product (if needed)
        private void FilterBrandsByProduct(int productID)
        {
            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                SqlConnection conn = new SqlConnection(connectionString);
                {
                    string query = "SELECT DISTINCT Brand FROM Product WHERE ProductID = @ProductID ORDER BY Brand";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", productID);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        //comboBoxBrand.Items.Clear();
                        //comboBoxBrand.Items.Add("-- Select Brand --");

                        while (reader.Read())
                        {
                          //  comboBoxBrand.Items.Add(reader["Brand"].ToString());
                        }

                        //comboBoxBrand.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering brands by product: " + ex.Message);
            }
        }
        // Method to get all selected product information (if needed)
        private void GetSelectedProductInfo(out int productID, out string productName, out string brand)
        {
            productID = selectedProductID;
            productName = selectedProductName;
            brand = selectedBrand;
        }

        // Updated GetSelectedProductID method (simpler now)
        private int GetSelectedProductID()
        {
            return selectedProductID;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            // Check if product is selected
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product!");
                return;
            }

            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                SqlConnection conn = new SqlConnection(connectionString);
                {
                    string query = @"INSERT INTO ProInventory 
                           (InventoryID, InvDate, LaatNo, Bags, Weight, Rate, Amount, Credit, Debit,cash, ProductID, PartyID) 
                           VALUES 
                           (@InventoryID, @InvDate, @LaatNo, @Bags, @Weight, @Rate, @Amount, @Credit, @Debit,@cash, @ProductID, @PartyID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use the stored selectedProductID variable
                        cmd.Parameters.AddWithValue("@InventoryID", Convert.ToInt32(textInventoryID.Text));
                        cmd.Parameters.AddWithValue("@ProductID", selectedProductID);
                        cmd.Parameters.AddWithValue("@PartyID",partyID);
                        cmd.Parameters.AddWithValue("@InvDate", dateTimePickerDate.Value);
                        cmd.Parameters.AddWithValue("@LaatNo", textLaatNo.Text);
                        cmd.Parameters.AddWithValue("@Bags", Convert.ToInt32(textBags.Text));
                        cmd.Parameters.AddWithValue("@Weight", Convert.ToDecimal(textWeight.Text));
                        cmd.Parameters.AddWithValue("@Rate", Convert.ToDecimal(textRate.Text));
                        cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(textAmount.Text));
                        cmd.Parameters.AddWithValue("@Credit", Convert.ToDecimal(textCredit.Text));
                        cmd.Parameters.AddWithValue("@Debit", Convert.ToDecimal(textDebit.Text));
                        cmd.Parameters.AddWithValue("@cash", Convert.ToDecimal(textCash.Text));

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                           // MessageBox.Show("Inventory record added successfully!");
                            
                            LoadProInventoryDataGridView();
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add inventory record!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding inventory: " + ex.Message);
            }
        }

        private void comboBoxProductName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxProductName.SelectedIndex > 0)
                {
                    // Get the selected item
                    dynamic selectedItem = comboBoxProductName.SelectedItem;

                    // Extract ProductID, ProductName, and Brand
                    selectedProductID = Convert.ToInt32(selectedItem.Value);
                    selectedProductName = selectedItem.Display.ToString();
                    selectedBrand = selectedItem.Brand.ToString();
                    textBrand.Text = selectedBrand;

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
                    selectedProductID = 0;
                    selectedProductName = "";
                    selectedBrand = "";
                    //comboBoxBrand.Text = "-- Select Brand --";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting product: " + ex.Message);
            }
        }

        private void comboBoxBrand_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearForm();
            AutoGenerateInventory();
            LoadProInventoryDataGridView();
        }


        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Refresh data when remove form is closed
            ClearForm();
            AutoGenerateInventory();
            LoadProInventoryDataGridView();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Form4 deleteInv = new Form4();
            deleteInv.FormClosed += Form4_FormClosed;
            deleteInv.Show();
        }

        private void textBags_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBags.Text, out int bags))
            {
                decimal.TryParse(textWPB.Text, out decimal wpb);
                decimal weight = bags * wpb;
                textWeight.Text = weight.ToString();
               // int.TryParse(textTotalBags.Text, out int totalBags);
                //int remBag = totalBags - bags;
                //textRemBags.Text = remBag.ToString();
            }
            else
            {
                textWeight.Text = "0";
                //textRemBags.Text = "0";

            }
            /*if (int.TryParse(textBags.Text, out int bags)) 
            {
                int weight = bags * 50;
                textWeight.Text = weight.ToString();
            }
            else 
            {
                textWeight.Clear();
            }*/
        }

        private void textRate_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(textRate.Text, out decimal rate)) 
            {
                decimal weight = Convert.ToDecimal(textWeight.Text);
                decimal amount = rate * weight;
                textAmount.Text = amount.ToString();
            }
            else 
            {
                textAmount.Clear();
            }
        }

        private void textWeight_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(textRate.Text, out decimal rate))
            {
                decimal.TryParse(textWeight.Text, out decimal weight);
                //decimal weight = Convert.ToDecimal(textWeight.Text);
                decimal amount = rate * weight;
                textAmount.Text = amount.ToString();
            }
            else
            {
                textAmount.Clear();
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void Status_SelectedIndexChanged(object sender, EventArgs e)
        {
                decimal credit = 0;
                decimal debit = 0;
                decimal cash = 0;
                decimal.TryParse(textAmount.Text, out decimal amount);
            /* if(double.TryParse(textAmount.Text, out double amount))
                {
                MessageBox.Show("Please enter a valid amount first!");
                return;
                }*/


            // Get the selected status text
            string selectedStatus = Status.SelectedItem?.ToString();

            // Use if-else if-else structure for mutually exclusive conditions
            if (selectedStatus == "Credit")
            {
                credit = amount;
                debit = 0;
                cash = 0;
                textCredit.Text = credit.ToString("F2");
                textDebit.Text = debit.ToString("F2");
                textCash.Text = cash.ToString("F2");
            }
            else if (selectedStatus == "Debit")
            {
                debit = amount;
                credit = 0;
                cash = 0;
                textDebit.Text = debit.ToString("F2");
                textCredit.Text = credit.ToString("F2");
                textCash.Text = cash.ToString("F2");
            }
            else if (selectedStatus == "Cash")
            {
                // Handle other cases (like "Select Status" or unexpected values)
                cash = amount;
                credit = 0;
                debit = 0;
                textCredit.Text = credit.ToString("F2");
                textDebit.Text = debit.ToString("F2");
                textCash.Text = cash.ToString("F2");
            }
            else 
            {
                cash = 0;
                credit = 0;
                debit = 0;
                textCredit.Text = "0.00";
                textDebit.Text = "0.00";
                textCash.Text = "0.00";
            }

        }

        private void comboParty_SelectedIndexChanged(object sender, EventArgs e)
        {
            // FIX: Get the actual PartyID value, not the SelectedIndex
            if (comboParty.SelectedIndex > 0)
            {
                dynamic selectedItem = comboParty.SelectedItem;
                partyID = Convert.ToInt32(selectedItem.Value); // Get the actual PartyID
                LoadProducts(partyID);
            }
            else
            {
                partyID = 0;
                comboBoxProductName.Items.Clear();
                comboBoxProductName.Items.Add("-- Select Product --");
                comboBoxProductName.SelectedIndex = 0;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            // Check if product is selected
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product!");
                return;
            }

            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE ProInventory 
                        SET InvDate = @InvDate, 
                            LaatNo = @LaatNo, 
                            Bags = @Bags, 
                            Weight = @Weight, 
                            Rate = @Rate, 
                            Amount = @Amount, 
                            Credit = @Credit, 
                            Debit = @Debit, 
                            cash = @cash,
                            ProductID = @ProductID, 
                            PartyID = @PartyID 
                        WHERE InventoryID = @InventoryID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use the stored selectedProductID variable
                        cmd.Parameters.AddWithValue("@InventoryID", Convert.ToInt32(textInventoryID.Text));
                        cmd.Parameters.AddWithValue("@ProductID", selectedProductID);
                        cmd.Parameters.AddWithValue("@PartyID", partyID);
                        cmd.Parameters.AddWithValue("@InvDate", dateTimePickerDate.Value);
                        cmd.Parameters.AddWithValue("@LaatNo", textLaatNo.Text);
                        cmd.Parameters.AddWithValue("@Bags", Convert.ToInt32(textBags.Text));
                        cmd.Parameters.AddWithValue("@Weight", Convert.ToDecimal(textWeight.Text));
                        cmd.Parameters.AddWithValue("@Rate", Convert.ToDecimal(textRate.Text));
                        cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(textAmount.Text));
                        cmd.Parameters.AddWithValue("@Credit", Convert.ToDecimal(textCredit.Text));
                        cmd.Parameters.AddWithValue("@Debit", Convert.ToDecimal(textDebit.Text));
                        cmd.Parameters.AddWithValue("@cash", Convert.ToDecimal(textCash.Text));

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Inventory record updated successfully!");

                            LoadProInventoryDataGridView();
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Failed to update inventory record! Record not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            LoadData(textSearch.Text.Trim());
        }

        // Method to load products for a specific party
       

        // Method to get brand from product name
        // Optional: Load brand from database
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            /*
            // Check if double-click is on a valid row (not header)
            if (e.RowIndex < 0) return;

            try
            {
                // Get the selected row
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Check if there's data in the row
                if (row.Cells["InventoryID"].Value == null) return;

                // Load basic text fields
                textInventoryID.Text = row.Cells["InventoryID"].Value?.ToString() ?? "";
                textLaatNo.Text = row.Cells["LaatNo"].Value?.ToString() ?? "";
                textBags.Text = row.Cells["Bags"].Value?.ToString() ?? "";
                textWeight.Text = row.Cells["Weight"].Value?.ToString() ?? "";
                textRate.Text = row.Cells["Rate"].Value?.ToString() ?? "";
                textAmount.Text = row.Cells["Amount"].Value?.ToString() ?? "";
                textCredit.Text = row.Cells["Credit"].Value?.ToString() ?? "0";
                textDebit.Text = row.Cells["Debit"].Value?.ToString() ?? "0";

                // Load Date from grid
                if (row.Cells["InvDate"].Value != null && DateTime.TryParse(row.Cells["InvDate"].Value.ToString(), out DateTime invDate))
                {
                    dateTimePickerDate.Value = invDate;
                }
                else
                {
                    dateTimePickerDate.Value = DateTime.Today;
                }

                // Load Party - Get ID from database based on name
                if (row.Cells["PartyName"].Value != null)
                {
                    string partyName = row.Cells["PartyName"].Value.ToString();
                    partyID = GetPartyIDByName(partyName);

                    if (partyID > 0)
                    {
                        // Find and select the party in comboParty
                        bool partyFound = false;
                        for (int i = 0; i < comboParty.Items.Count; i++)
                        {
                            if (comboParty.Items[i] is KeyValuePair<int, string> item)
                            {
                                if (item.Key == partyID)
                                {
                                    comboParty.SelectedIndex = i;
                                    partyFound = true;
                                    break;
                                }
                            }
                        }

                        if (!partyFound)
                        {
                            MessageBox.Show($"Party '{partyName}' not found in list!", "Warning",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            comboParty.SelectedIndex = -1;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Could not find Party ID for '{partyName}'", "Warning",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        comboParty.SelectedIndex = -1;
                    }
                }

                // Load Product - Get ID from database based on name and party
                if (row.Cells["ProductName"].Value != null && partyID > 0)
                {
                    string productName = row.Cells["ProductName"].Value.ToString();

                    // Get product ID from database
                    int productId = GetProductIDByNameAndParty(productName, partyID);

                    // Clear and load products for the selected party
                    comboBoxProductName.Items.Clear();

                    if (partyID > 0)
                    {
                        // Load products for this specific party
                        LoadProductsForParty(partyID);

                        if (productId > 0)
                        {
                            // Find and select the product
                            bool productFound = false;
                            for (int i = 0; i < comboBoxProductName.Items.Count; i++)
                            {
                                if (comboBoxProductName.Items[i] is KeyValuePair<int, string> item)
                                {
                                    if (item.Key == productId)
                                    {
                                        comboBoxProductName.SelectedIndex = i;
                                        productFound = true;

                                        // Load brand from product
                                        textBrand.Text = GetBrandFromDatabase(productId);
                                        break;
                                    }
                                }
                            }

                            if (!productFound)
                            {
                                MessageBox.Show($"Product '{productName}' not found for this party!", "Warning",
                                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                comboBoxProductName.SelectedIndex = -1;
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Could not find Product ID for '{productName}'", "Warning",
                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }

                // Load Status based on Credit/Debit values
                decimal creditAmount = decimal.TryParse(textCredit.Text, out decimal credit) ? credit : 0;
                decimal debitAmount = decimal.TryParse(textDebit.Text, out decimal debit) ? debit : 0;

                // Clear and set status items
                Status.Items.Clear();

                if (creditAmount == 0 && debitAmount > 0)
                {
                    // Only Debit is available
                    Status.Items.Add("Debit");
                    Status.SelectedIndex = 0;
                }
                else if (debitAmount == 0 && creditAmount > 0)
                {
                    // Only Credit is available
                    Status.Items.Add("Credit");
                    Status.SelectedIndex = 0;
                }
                else if (creditAmount > 0 && debitAmount > 0)
                {
                    // Both have values - show both options
                    Status.Items.Add("Credit");
                    Status.Items.Add("Debit");

                    // Determine which one to select based on which is larger
                    string statusToSelect = creditAmount >= debitAmount ? "Credit" : "Debit";
                    Status.SelectedItem = statusToSelect;
                }
                else
                {
                    // Both are 0 - default to Credit
                    Status.Items.Add("Credit");
                    Status.SelectedIndex = 0;
                }

                // Optional: Focus on the first editable field
                textLaatNo.Focus();
                textLaatNo.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading record: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            */
        }

        // Helper methods to get IDs from database
       
       
        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            // Check if double-click is on a valid row (not header)
            if (e.RowIndex < 0) return;

            try
            {
                // Get the selected row
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Check if there's data in the row
                if (row.Cells["InventoryID"].Value == null) return;

                // Load basic text fields
                textInventoryID.Text = row.Cells["InventoryID"].Value?.ToString() ?? "";
                textLaatNo.Text = row.Cells["LaatNo"].Value?.ToString() ?? "";
                textBags.Text = row.Cells["Bags"].Value?.ToString() ?? "";
                textWeight.Text = row.Cells["Weight"].Value?.ToString() ?? "";
                textRate.Text = row.Cells["Rate"].Value?.ToString() ?? "";
                textAmount.Text = row.Cells["Amount"].Value?.ToString() ?? "";
                textCredit.Text = row.Cells["Credit"].Value?.ToString() ?? "0";
                textDebit.Text = row.Cells["Debit"].Value?.ToString() ?? "0";

                // Load Date from grid
                if (row.Cells["InvDate"].Value != null && DateTime.TryParse(row.Cells["InvDate"].Value.ToString(), out DateTime invDate))
                {
                    dateTimePickerDate.Value = invDate;
                }
                else
                {
                    dateTimePickerDate.Value = DateTime.Today;
                }

                // Load Party from grid
                if (row.Cells["PartyName"].Value != null)
                {
                    string partyNameFromGrid = row.Cells["PartyName"].Value.ToString().Trim();

                    // Try to find the party in comboParty
                    bool partyFound = false;

                    // Loop through comboParty items (skip first item which is "-- Select Party --")
                    for (int i = 1; i < comboParty.Items.Count; i++)
                    {
                        dynamic item = comboParty.Items[i];
                        string itemDisplay = item.Display.ToString().Trim();

                        // Case-insensitive comparison
                        if (itemDisplay.Equals(partyNameFromGrid, StringComparison.OrdinalIgnoreCase))
                        {
                            comboParty.SelectedIndex = i;
                            partyID = Convert.ToInt32(item.Value); // Store the party ID
                            partyFound = true;

                            // DEBUG: Uncomment to see what matched
                            // MessageBox.Show($"Matched party: '{itemDisplay}'", "Debug");
                            break;
                        }
                    }

                    if (!partyFound)
                    {
                        // Try fuzzy matching (contains)
                        for (int i = 1; i < comboParty.Items.Count; i++)
                        {
                            dynamic item = comboParty.Items[i];
                            string itemDisplay = item.Display.ToString().Trim();

                            if (itemDisplay.IndexOf(partyNameFromGrid, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                partyNameFromGrid.IndexOf(itemDisplay, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                comboParty.SelectedIndex = i;
                                partyID = Convert.ToInt32(item.Value);
                                partyFound = true;

                                // DEBUG: Uncomment to see what matched
                                // MessageBox.Show($"Fuzzy matched party: '{itemDisplay}' with '{partyNameFromGrid}'", "Debug");
                                break;
                            }
                        }
                    }

                    if (!partyFound)
                    {
                        MessageBox.Show($"Party '{partyNameFromGrid}' not found in the list.\n" +
                                       "Please make sure the party exists in the database.",
                                       "Party Not Found",
                                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        comboParty.SelectedIndex = 0;
                        partyID = 0;
                    }
                    else
                    {
                        // Load products for this party (this will trigger automatically via comboParty_SelectedIndexChanged)
                        // But we need to manually trigger it if the party was already selected
                        if (comboParty.SelectedIndex > 0)
                        {
                            LoadProducts(partyID);
                        }
                    }
                }

                // Load Product and Brand from grid
                if (row.Cells["ProductName"].Value != null && row.Cells["Brand"].Value != null)
                {
                    string productNameFromGrid = row.Cells["ProductName"].Value.ToString().Trim();
                    string brandFromGrid = row.Cells["Brand"].Value.ToString().Trim();

                    // Set brand text box
                    textBrand.Text = brandFromGrid;

                    // Wait a moment for products to load (if they're loading async)
                    // Then try to find the product in comboBoxProductName
                    System.Threading.Thread.Sleep(100); // Small delay

                    bool productFound = false;

                    // Check if products are loaded
                    if (comboBoxProductName.Items.Count > 1) // More than just "-- Select Product --"
                    {
                        // Loop through product items (skip first item)
                        for (int i = 1; i < comboBoxProductName.Items.Count; i++)
                        {
                            dynamic item = comboBoxProductName.Items[i];
                            string itemDisplay = item.Display.ToString().Trim();

                            // Check if product name matches
                            if (itemDisplay.Equals(productNameFromGrid, StringComparison.OrdinalIgnoreCase))
                            {
                                comboBoxProductName.SelectedIndex = i;

                                // Update the selected product variables
                                selectedProductID = Convert.ToInt32(item.Value);
                                selectedProductName = item.Display.ToString();
                                selectedBrand = item.Brand.ToString();

                                productFound = true;

                                // DEBUG: Uncomment to see what matched
                                // MessageBox.Show($"Matched product: '{itemDisplay}'", "Debug");
                                break;
                            }
                        }

                        if (!productFound)
                        {
                            // Try fuzzy matching for product
                            for (int i = 1; i < comboBoxProductName.Items.Count; i++)
                            {
                                dynamic item = comboBoxProductName.Items[i];
                                string itemDisplay = item.Display.ToString().Trim();

                                if (itemDisplay.IndexOf(productNameFromGrid, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    comboBoxProductName.SelectedIndex = i;

                                    // Update the selected product variables
                                    selectedProductID = Convert.ToInt32(item.Value);
                                    selectedProductName = item.Display.ToString();
                                    selectedBrand = item.Brand.ToString();

                                    productFound = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!productFound && comboParty.SelectedIndex > 0)
                    {
                        MessageBox.Show($"Product '{productNameFromGrid}' not found for the selected party.\n" +
                                       "The product might not exist or belong to a different party.",
                                       "Product Not Found",
                                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        comboBoxProductName.SelectedIndex = 0;
                        selectedProductID = 0;
                        selectedProductName = "";
                        selectedBrand = "";
                    }
                }

                // Load Status based on Credit/Debit values
                decimal creditAmount = decimal.TryParse(textCredit.Text, out decimal credit) ? credit : 0;
                decimal debitAmount = decimal.TryParse(textDebit.Text, out decimal debit) ? debit : 0;

                // Clear and set status items
                Status.Items.Clear();
                Status.Items.Add("-- Select Status --");

                // Always add both options for selection
                Status.Items.Add("Credit");
                Status.Items.Add("Debit");

                // Determine which status to select based on values
                if (creditAmount > 0 && debitAmount == 0)
                {
                    Status.SelectedItem = "Credit";
                }
                else if (debitAmount > 0 && creditAmount == 0)
                {
                    Status.SelectedItem = "Debit";
                }
                else if (creditAmount > 0 && debitAmount > 0)
                {
                    // If both have values, select the larger one
                    Status.SelectedItem = creditAmount >= debitAmount ? "Credit" : "Debit";
                }
                else
                {
                    // Both are 0, select the default
                    Status.SelectedIndex = 0;
                }

                // Calculate weight and amount based on loaded values
                // Trigger the calculations
                //textBags_TextChanged(null, EventArgs.Empty);
                //textRate_TextChanged(null, EventArgs.Empty);
                //textWeight_TextChanged(null, EventArgs.Empty);

                // If Status is selected, trigger its calculation too
                if (Status.SelectedIndex > 0)
                {
                    Status_SelectedIndexChanged(null, EventArgs.Empty);
                }

                // Focus on the first editable field
                textLaatNo.Focus();
                textLaatNo.SelectAll();

                // Change button text to indicate update mode
                //btnAdd.Text = "Update";

                // Optional: Show success message
                // MessageBox.Show("Record loaded for editing.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading record: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                               "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void textWPB_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(textWPB.Text, out decimal wpb))
            {
                decimal.TryParse(textBags.Text, out decimal bags);
                decimal weight = bags * wpb;
                textWeight.Text = weight.ToString();
            }
            else
            {
                textWeight.Text = "0";

            }
        }
    }
}

