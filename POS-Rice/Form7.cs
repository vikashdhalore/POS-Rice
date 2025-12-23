using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace POS_Rice
{
    public partial class saleProduct : Form
    {
        public string CustomerID = "";
        public string CustomerName = "";
        private int saleItemID = 1;
        private int selectedProductID = 0;
        private string selectedProductName = "";
        private string selectedBrand = "";
        private int partyID = 0;
        private int orderID = 0;
        private int CustID = 0;

        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
        public saleProduct()
        {
            InitializeComponent();
        }
        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(textRate.Text, out decimal rate))
            {
                Decimal.TryParse(textWeight.Text, out decimal weight);
                decimal amount = rate * weight;
                textAmount.Text = amount.ToString();
            }
            else
            {
                textAmount.Clear();
            }
        }
        private void saleProduct_Load(object sender, EventArgs e)
        {
            OrderGridView();
            //ItemClear();
            LoadParty();
            AutoGenerateOrderID();
            //GenerateSaleItemID();

            ItemGridView(textOrderID.Text);
            GenerateSaleItemID();
            ItemClear();
            OrderClear();
            comboParty.SelectedIndex = 0;
            comboProduct.SelectedIndex = 0;

            //dataGridView2.CellDoubleClick += dataGridView2_CellDoubleClick;
            // If you want both cell content and cell double-click to work
            //dataGridView2.CellContentDoubleClick += dataGridView2_CellContentDoubleClick;
        }
        private void GenerateSaleItemID()
        {
            SqlConnection con = new SqlConnection(connectionString);
            try
            {
                con.Open();

                //if OrderID is Null Or Not available


                // check if Order ID exists in database
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM SaleProduct Where OrderID = @OrderID", con);

                checkCmd.Parameters.AddWithValue("@OrderID", orderID);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count == 0)
                {
                    //Order id not found start SPID from 1

                    textSPID.Text = "1";

                }

                else
                {
                    // Get max SP_ID for that OrderID
                    SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(SP_ID), 0) +1 FROM SaleProduct WHERE OrderID = @OrderID", con);
                    cmd.Parameters.AddWithValue("@OrderID", orderID);
                    int newSpID = Convert.ToInt32(cmd.ExecuteScalar());
                    textSPID.Text = newSpID.ToString();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.Message);
            }
            finally
            {
                con.Close();
            }

        }
        private void itemNoIsOne()
        {

        }
        private void GenerateTotalBags()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ISNULL(SUM(Bags), 0) - ISNULL((SELECT SUM(QtyBag) FROM SaleProduct WHERE PartyID = ProInventory.PartyID AND ProductID = ProInventory.ProductID), 0) AS Stock FROM ProInventory WHERE PartyID = @PartyID AND ProductID = @ProductID GROUP BY PartyID, ProductID", conn);

                cmd.Parameters.AddWithValue("@PartyID", partyID);
                cmd.Parameters.AddWithValue("@ProductID", selectedProductID);

                Object result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    int nextID = Convert.ToInt32(result);
                    textTotalBags.Text = nextID.ToString();
                }
                else
                {
                    textTotalBags.Text = "0";
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Generating Total Bags: " + ex.Message);
                textTotalBags.Text = "0"; // Set default value on error
            }
            finally
            {
                conn.Close();
                textTotalBags.Focus();
            }
        }
        private void AutoGenerateOrderID()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("Select ISNULL(MAX(OrderID), 0) + 1 from Orders", conn);
                orderID = Convert.ToInt32(cmd.ExecuteScalar());
                textOrderID.Text = orderID.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Order ID: " + ex.Message);
            }
            finally
            {
                conn.Close();
                textOrderID.Focus();
            }
        }
        private void AutoGenerateSaleProductID()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("Select ISNULL(MAX(SP_ID), 0) + 1 from SaleProduct", conn);
                int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                //int orderID = Convert.ToInt32(textOrderID.Text);
                //cmd.Parameters.AddWithValue("@OrderID", orderID);
                textSPID.Text = nextID.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Sale Product ID: " + ex.Message);
            }
            finally
            {
                conn.Close();
                textSPID.Focus();
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
                        comboProduct.Items.Clear();
                        comboProduct.Items.Add("-- Select Product --");

                        while (reader.Read())
                        {
                            comboProduct.Items.Add(new
                            {
                                Display = reader["ProductName"].ToString(),
                                Value = reader["ProductID"].ToString(),
                                Brand = reader["Brand"].ToString()
                            });
                            //partyID = Convert.ToInt32(comboParty.SelectedIndex);
                        }

                        comboProduct.DisplayMember = "Display";
                        comboProduct.ValueMember = "Value";
                        //comboBoxBrand.DisplayMember = "Brand";
                        //comboBoxBrand.ValueMember = "Value";
                        //comboBoxProductName.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
        }
        private void OrderClear()
        {
            //comboStatus.SelectedIndex = 0;
            textCredit.Text = "0";
            textDebit.Text = "0";
            textCash.Text = "0";
            textBalance.Text = "0";
            textCustName.Text = "";
            textSearchCust.Clear();
            dateTimeDate.Value = DateTime.Now;

        }
        private void ItemClear()
        {
            //textSPID.Clear();
            textBrand.Clear();
            textBags.Text = "0";
            textTotalBags.Text = "0";
            textRemBags.Text = "0";
            textWeight.Text = "0";
            textWPB.Text = "0";
            textRate.Text = "0";
            textAmount.Text = "0";
            textSPID.Focus();
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
                    //AddCheckBoxColumn();
                    ItemClear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void OrderGridView()
        {
            try
            {
                if (dataGridView2.Columns.Contains("CheckBoxColumn"))
                {
                    dataGridView2.Columns.Remove("CheckBoxColumn");
                }

                // Create connection string - update with your database details
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

                // SQL select query
                string query = "select OrderID, Customer_Name,OrderDate,Credit,Debit,Cash,Balance from Orders";

                // Create connection and data adapter
                using (SqlConnection connection = new SqlConnection(connectionString))
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
                {
                    // Create DataTable to hold the data
                    DataTable dataTable = new DataTable();

                    // Fill DataTable with data from database
                    dataAdapter.Fill(dataTable);

                    // Set DataGridView data source
                    dataGridView2.DataSource = dataTable;

                    // Optional: Configure column headers
                    dataGridView2.Columns["OrderID"].HeaderText = "Order No";
                    dataGridView2.Columns["Customer_Name"].HeaderText = "Customer Name";
                    dataGridView2.Columns["OrderDate"].HeaderText = "Date";
                    dataGridView2.Columns["Credit"].HeaderText = "Credit";
                    dataGridView2.Columns["Debit"].HeaderText = "Debit";
                    dataGridView2.Columns["Cash"].HeaderText = "Cash";
                    dataGridView2.Columns["Balance"].HeaderText = "Balance";




                    // Optional: Auto-size columns
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Add checkbox column after data binding
                    //AddCheckBoxColumn();
                    ItemClear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void GenerateSaleItemIFromDGrid2()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {

                conn.Open();
                SqlCommand cmd = new SqlCommand("Select ISNULL(MAX(SP_ID), 0) + 1 from SaleProduct", conn);
                int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                textSPID.Text = nextID.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Sale Product ID: " + ex.Message);
            }
            finally
            {
                conn.Close();
                textSPID.Focus();
            }
        }
        private void LoadOrderItems(string orderID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"select S.SP_ID, Pt.PartyName,P.ProductName,P.Brand,S.QtyBag,S.TotalQtyBags,S.RemBags,S.Weight,S.Rate,S.Amount from SaleProduct S left join product P on S.ProductID = P.ProductID left join Party Pt on S.PartyID = Pt.PartyID Where OrderID = @OrderID";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@OrderID", orderID);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Bind to GridView1
                    dataGridView1.DataSource = dt;

                    // Optional: Format the items grid
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView1.ReadOnly = true;

                    // Show order info in status or label
                    //UpdateOrderInfo(orderID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order items: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool ValidateInputs()
        {
            if (comboProduct.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a product!");
                return false;
            }

            /*if (Status.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a Status of Credit Or Debit!");
                return false;
            }*/

            if (comboParty.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select parties!");
                return false;
            }

            /*if (string.IsNullOrEmpty(textLaatNo.Text))
            {
                MessageBox.Show("Please enter Lat No!");
                return false;
            }*/

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

            if (!decimal.TryParse(textTotalBags.Text, out _))
            {
                MessageBox.Show("Please enter valid number for Total Bags!");
                return false;
            }

            if (!decimal.TryParse(textRemBags.Text, out _))
            {
                MessageBox.Show("Please enter valid number for Remaining Bags!");
                return false;
            }

            return true;
        }
        private void SumOfAmount(int orderId)
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select sum(amount) from saleProduct Where OrderID = @OrderID", conn);
                cmd.Parameters.AddWithValue("OrderID", orderId);

                // Handle NULL result
                object result = cmd.ExecuteScalar();
                decimal totAmount = (result != null && result != DBNull.Value) ? Convert.ToDecimal(result) : 0;

                if (comboStatus.SelectedIndex == 1)
                {
                    textCash.Text = totAmount.ToString("N2");
                    textCredit.Text = "0";
                    textDebit.Text = "0";
                    textBalance.Text = "0";
                }
                // ... rest of the code
                if (comboStatus.SelectedIndex == 2)
                {
                    textCash.Text = "0";
                    textCredit.Text = totAmount.ToString("N2");
                    textDebit.Text = "0";
                    textBalance.Text = totAmount.ToString("N2");
                }
                if (comboStatus.SelectedIndex == 3)
                {
                    textCash.Text = "0";
                    textCredit.Text = "0";
                    textDebit.Text = totAmount.ToString("N2");
                    textBalance.Text = "0";
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating total amount: " + ex.Message);
            }
            finally
            {
                conn.Close();
                textBags.Focus();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            PDFInvoiceGenerator generator = new PDFInvoiceGenerator();
            generator.GeneratePDFInvoiceFromOrder(
                textOrderID, textCustName, dateTimeDate, textCash, textCredit,
                textDebit, textBalance
            );
        }
        private void comboProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboProduct.SelectedIndex > 0)
                {
                    // Get the selected item
                    dynamic selectedItem = comboProduct.SelectedItem;

                    // Extract ProductID, ProductName, and Brand
                    selectedProductID = Convert.ToInt32(selectedItem.Value);
                    selectedProductName = selectedItem.Display.ToString();
                    selectedBrand = selectedItem.Brand.ToString();
                    textBrand.Text = selectedBrand;

                    GenerateTotalBags();

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
                comboProduct.Items.Clear();
                comboProduct.Items.Add("-- Select Product --");
                comboProduct.SelectedIndex = 0;
            }
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void textBags_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBags.Text, out int bags))
            {
                decimal.TryParse(textWPB.Text, out decimal wpb);
                decimal weight = bags * wpb;
                textWeight.Text = weight.ToString();
                int.TryParse(textTotalBags.Text, out int totalBags);
                int remBag = totalBags - bags;
                textRemBags.Text = remBag.ToString();
            }
            else
            {
                textWeight.Text = "0";
                textRemBags.Text = "0";

            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            // Check if product is selected
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product!");
                return;
            }
            int bag = Convert.ToInt32(textBags.Text);
            int TotBags = Convert.ToInt32(textTotalBags.Text);
            if (bag > TotBags)
            {
                MessageBox.Show("You can't purchase bags more than available!");
                return;
            }

            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                SqlConnection conn = new SqlConnection(connectionString);
                {
                    string query = @"INSERT INTO SaleProduct 
                           (SP_ID, PartyID, ProductID, QtyBag, TotalQtyBags, RemBags, Weight, Rate, Amount, OrderID) 
                           VALUES 
                           (@SP_ID, @PartyID, @ProductID, @QtyBag, @TotalQtyBags, @RemBags, @Weight, @Rate, @Amount, @OrderID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use the stored selectedProductID variable
                        cmd.Parameters.AddWithValue("@SP_ID", Convert.ToInt32(textSPID.Text));
                        cmd.Parameters.AddWithValue("@PartyID", partyID);
                        cmd.Parameters.AddWithValue("@ProductID", selectedProductID);
                        cmd.Parameters.AddWithValue("@QtyBag", Convert.ToInt32(textBags.Text));
                        cmd.Parameters.AddWithValue("@TotalQtyBags", textTotalBags.Text);
                        cmd.Parameters.AddWithValue("@RemBags", Convert.ToInt32(textRemBags.Text));
                        cmd.Parameters.AddWithValue("@Weight", Convert.ToDecimal(textWeight.Text));
                        cmd.Parameters.AddWithValue("@Rate", Convert.ToDecimal(textRate.Text));
                        cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(textAmount.Text));
                        cmd.Parameters.AddWithValue("@OrderID", Convert.ToDecimal(textOrderID.Text));


                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            //MessageBox.Show("Item record added successfully!");

                            ItemGridView(textOrderID.Text);
                            GenerateSaleItemID();
                            ItemClear();
                            comboParty.SelectedIndex = 0;
                            comboProduct.SelectedIndex = 0;
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
        private void button3_Click(object sender, EventArgs e)
        {
            ItemClear();
            ItemGridView(textOrderID.Text);
            GenerateSaleItemID();
            comboParty.SelectedIndex = 0;
            comboProduct.SelectedIndex = 0;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            // Check if status is selected
            if (comboStatus.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a Status!");
                return;
            }

            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                SqlConnection conn = new SqlConnection(connectionString);
                {
                    string query = @"INSERT INTO Orders 
                           (OrderID, Customer_Name, OrderDate, Credit, Debit, Cash, Balance,CustID) 
                           VALUES 
                           (@OrderID, @Customer_Name, @OrderDate, @Credit, @Debit, @Cash, @Balance,@CustID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use the stored selectedProductID variable
                        cmd.Parameters.AddWithValue("@OrderID", Convert.ToInt32(textOrderID.Text));
                        cmd.Parameters.AddWithValue("@CustID", CustID);
                        cmd.Parameters.AddWithValue("@Customer_Name", textCustName.Text);
                        cmd.Parameters.AddWithValue("@OrderDate", dateTimeDate.Value);
                        cmd.Parameters.AddWithValue("@Credit", Convert.ToDecimal(textCredit.Text));
                        cmd.Parameters.AddWithValue("@Debit", Convert.ToDecimal(textDebit.Text));
                        cmd.Parameters.AddWithValue("@Cash", Convert.ToDecimal(textCash.Text));
                        cmd.Parameters.AddWithValue("@Balance", Convert.ToDecimal(textWeight.Text));



                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            //MessageBox.Show("Order record added successfully!");

                            OrderClear();
                            OrderGridView();
                            AutoGenerateOrderID();
                            ItemGridView(textOrderID.Text);
                            GenerateSaleItemID();
                            ItemClear();
                            comboParty.SelectedIndex = 0;
                            comboProduct.SelectedIndex = 0;
                            comboStatus.SelectedIndex = 0;
 
                        }
                        else
                        {
                            MessageBox.Show("Failed to add Order record!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding orders: " + ex.Message);
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if (textCustName.Text == null)
            {
                MessageBox.Show("Please enter customer name!");
                return;
            }

            if (textCash.Text == null && textCredit.Text == null && textDebit.Text == null && textBalance.Text == null)
            {
                MessageBox.Show("Some of the field not added to updated");
                return;
            }

            // Check if product is selected
            if (comboStatus.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a Status from (Cash, Credit, Debit)!");
                return;
            }

            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE Orders 
                        SET   
                            Customer_Name = @Customer_Name, 
                            OrderDate = @OrderDate, 
                            Credit = @Credit, 
                            Debit = @Debit, 
                            Cash = @Cash, 
                            Balance = @Balance,
                            CustID = @CustID
                            Where OrderID = @OrderID";
                            

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use the stored selectedProductID variable
                        cmd.Parameters.AddWithValue("@OrderID", textOrderID.Text.Trim());
                        cmd.Parameters.AddWithValue("@Customer_Name", textCustName.Text.Trim());
                        cmd.Parameters.AddWithValue("@OrderDate", dateTimeDate.Value);
                        cmd.Parameters.AddWithValue("@Credit", Convert.ToDecimal(textCredit.Text.Trim()));
                        cmd.Parameters.AddWithValue("@Debit", Convert.ToDecimal(textDebit.Text.Trim()));
                        cmd.Parameters.AddWithValue("@Cash", Convert.ToDecimal(textCash.Text.Trim()));
                        cmd.Parameters.AddWithValue("@Balance", Convert.ToDecimal(textBalance.Text.Trim()));
                        cmd.Parameters.AddWithValue("@CustID", CustID);
                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Updated Order record successfully!");

                            LoadOrderItems(textOrderID.Text);

                            ItemClear();

                        }
                        else
                        {
                            MessageBox.Show("Failed to update Orders record! Record not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void comboStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboStatus.SelectedIndex == 0) 
            {
                textCredit.Text = "0";
                textDebit.Text = "0";
                textCash.Text = "0";
                textBalance.Text = "0";
            }
            else 
            {
                int OrdID = Convert.ToInt32(textOrderID.Text);
                SumOfAmount(OrdID);
            }
                
        }
        private string GetSafeString(object value)
        {
            if (value == null || value == DBNull.Value)
                return "0";
            return value.ToString();
        }
        private void dataGridView2_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            /*try
            {
                // Check if a row is selected and has data
                if (dataGridView2.CurrentRow != null && dataGridView2.CurrentRow.Cells["OrderID"].Value != null)
                {
                    DataGridViewRow row = dataGridView2.CurrentRow;

                    // Populate form fields with selected row data
                    textOrderID.Text = row.Cells["OrderID"].Value.ToString();
                    textCustName.Text = row.Cells["Customer_Name"].Value?.ToString() ?? "";
                    dateTimeDate.Text = row.Cells["OrderDate"].Value?.ToString() ?? "";
                    textCredit.Text = row.Cells["Credit"].Value?.ToString() ?? "";
                    textDebit.Text = row.Cells["Debit"].Value?.ToString() ?? "";
                    textCash.Text = row.Cells["Cash"].Value?.ToString() ?? "";
                    textBalance.Text = row.Cells["Balance"].Value?.ToString() ?? "";
                    //string customerID= row.Cells["CustID"].Value?.ToString() ?? "";

                    //CustID = Convert.ToInt32(customerID);

                    if (textCash.Text != "0" && textCash.Text != "")
                    {
                        comboStatus.SelectedIndex = 1;
                    }
                    if (textCredit.Text != "0" && textCredit.Text != "")
                    {
                        comboStatus.SelectedIndex = 2;
                    }
                    if (textDebit.Text != "0" && textDebit.Text != "")
                    {
                        comboStatus.SelectedIndex = 3;
                    }
                    if (textCredit.Text != "0" && textCredit.Text != "")
                    {
                        comboStatus.SelectedIndex = 2;
                    }

                    else
                    {
                        comboStatus.SelectedIndex = 0;
                    }
                    ItemClear();
                    GenerateSaleItemIFromDGrid2();
                    ItemGridView(textOrderID.Text);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting Items: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            */
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            
        }
        private void button4_Click(object sender, EventArgs e)
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
                    string query = @"UPDATE SaleProduct 
                        SET  
                            QtyBag = @QtyBag, 
                            TotalQtyBags = @TotalQtyBags, 
                            RemBags = @RemBags, 
                            Weight = @Weight, 
                            Rate = @Rate, 
                            Amount = @Amount, 
                            ProductID = @ProductID, 
                            PartyID = @PartyID 
                        WHERE SP_ID = @SP_ID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use the stored selectedProductID variable
                        cmd.Parameters.AddWithValue("@SP_ID", Convert.ToInt32(textSPID.Text));
                        cmd.Parameters.AddWithValue("@ProductID", selectedProductID);
                        cmd.Parameters.AddWithValue("@PartyID", partyID);
                        cmd.Parameters.AddWithValue("@QtyBag", Convert.ToInt32(textBags.Text));
                        cmd.Parameters.AddWithValue("@TotalQtyBags", Convert.ToDecimal(textTotalBags.Text));
                        cmd.Parameters.AddWithValue("@RemBags", Convert.ToDecimal(textRemBags.Text));
                        cmd.Parameters.AddWithValue("@Weight", Convert.ToDecimal(textWeight.Text));
                        cmd.Parameters.AddWithValue("@Rate", Convert.ToDecimal(textRate.Text));
                        cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(textAmount.Text));

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Updated record updated successfully!");

                            LoadOrderItems(textOrderID.Text);
                            ItemClear();
                            comboParty.SelectedIndex = 0;
                            comboProduct.SelectedIndex = 0;
                        }
                        else
                        {
                            MessageBox.Show("Failed to update Items record! Record not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
          /*  try
            {
                // Check if a row is selected and has data
                if (dataGridView2.CurrentRow != null && dataGridView2.CurrentRow.Cells["OrderID"].Value != null)
                {
                    DataGridViewRow row = dataGridView2.CurrentRow;

                    // Populate form fields with selected row data
                    textOrderID.Text = row.Cells["OrderID"].Value.ToString();
                    textCredit.Text = row.Cells["Credit"].Value?.ToString() ?? "";
                    textDebit.Text = row.Cells["Debit"].Value?.ToString() ?? "";
                    textCash.Text = row.Cells["Cash"].Value?.ToString() ?? "";
                    textBalance.Text = row.Cells["Balance"].Value?.ToString() ?? "";
                    //textRate.Text = row.Cells["Rate"].Value?.ToString() ?? "";
                    //textAmount.Text = row.Cells["Amount"].Value?.ToString() ?? "";
                    comboStatus.SelectedIndex = 0;
                    //OrderClear();
                    //comboProduct.SelectedIndex = 0;
                    //Status.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting Items: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
          */
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
                           OrderID,
                           Customer_Name,
                           OrderDate,
                           Credit,
                           Debit,
                           Cash,
                           Balance                       
                           FROM Orders
                           WHERE Customer_Name LIKE @search 
                           OR OrderDate LIKE @search 
                           OR Credit LIKE @search 
                           OR Debit LIKE @search
                           OR Cash LIKE @search
                           OR Balance LIKE @search";

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
                            dataGridView2.DataSource = dataTable;

                            // Configure column headers
                            // Optional: Configure column headers
                            dataGridView2.Columns["OrderID"].HeaderText = "Order No";
                            dataGridView2.Columns["Customer_Name"].HeaderText = "Customer Name";
                            dataGridView2.Columns["OrderDate"].HeaderText = "Date";
                            dataGridView2.Columns["Credit"].HeaderText = "Credit";
                            dataGridView2.Columns["Debit"].HeaderText = "Debit";
                            dataGridView2.Columns["Cash"].HeaderText = "Cash";
                            dataGridView2.Columns["Balance"].HeaderText = "Balance";

                            // Optional: Auto-size columns
                            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                            // Optional: Format columns
                            if (dataGridView2.Columns["OrderDate"] != null)
                                dataGridView2.Columns["OrderDate"].DefaultCellStyle.Format = "dd/MM/yyyy";

                            if (dataGridView1.Columns["Customer_Name"] != null)
                                dataGridView1.Columns["Customer_Name"].DefaultCellStyle.Format = "N2";

                            if (dataGridView1.Columns["Cash"] != null)
                                dataGridView1.Columns["Cash"].DefaultCellStyle.Format = "N2";

                            if (dataGridView1.Columns["Balance"] != null)
                                dataGridView1.Columns["Balance"].DefaultCellStyle.Format = "N2";

                            if (dataGridView1.Columns["Credit"] != null)
                                dataGridView1.Columns["Credit"].DefaultCellStyle.Format = "N2";

                            if (dataGridView1.Columns["Debit"] != null)
                                dataGridView1.Columns["Debit"].DefaultCellStyle.Format = "N2";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading Order data: {ex.Message}", "Database Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            LoadData(textSearch.Text.Trim());
        }
        private void button8_Click(object sender, EventArgs e)
        {
            CustomerInfo custInfo = new CustomerInfo();
            custInfo.Show();
        }
        private void SearchCustomer(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                listViewResults.Items.Clear();
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"select CustID, Customer_Name, PhoneNo from CustomerAct Where Customer_Name LIKE @SearchTerm or PhoneNo LIKE @SearchTerm";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            listViewResults.Items.Clear();

                            while (reader.Read())
                            {
                                ListViewItem item = new ListViewItem(reader["CustID"].ToString());
                                // item.SubItems.Add(Convert.ToInt32(reader["CustID"]).ToString("N2"));
                                item.SubItems.Add(reader["Customer_Name"].ToString());
                                //item.SubItems.Add(Convert.ToDateTime(reader["OrderDate"]).ToString("dd/MM/yyyy"));
                                item.SubItems.Add(reader["PhoneNo"].ToString());

                                listViewResults.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching orders: {ex.Message}", "Search Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {
            SearchCustomer(textSearchCust.Text);
        }
        private void listViewResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewResults.SelectedItems[0];
                string custoID = selectedItem.SubItems[0].Text;
                string customerName = selectedItem.SubItems[1].Text;

                // You can use these values to load the full order details
                CustID = Convert.ToInt32(custoID);
                textCustName.Text = customerName;
            }
        }
        private void RemoveItems_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Refresh data when remove form is closed
            ItemClear();
            ItemGridView(textOrderID.Text);
            GenerateSaleItemID();
            comboParty.SelectedIndex = 0;
            comboProduct.SelectedIndex = 0;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            RemoveItems removeItems = new RemoveItems(textOrderID.Text);
            removeItems.FormClosed += RemoveItems_FormClosed;
            removeItems.Show();
        }
        private void button9_Click(object sender, EventArgs e)
        {
            OrderClear();
            ItemClear();
            ItemGridView(textOrderID.Text);
            OrderGridView();
            comboParty.SelectedIndex = 0;
            comboProduct.SelectedIndex = 0;
            comboStatus.SelectedIndex = 0;
            AutoGenerateOrderID();
        }
        private void textWeight_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(textRate.Text, out decimal rate))
            {
                Decimal.TryParse(textWeight.Text, out decimal weight);
                decimal amount = rate * weight;
                textAmount.Text = amount.ToString();
            }
            else
            {
                textAmount.Clear();
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
        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            /*
            try
            {
                // Check if a row is selected and has data
                if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.Cells["SP_ID"].Value != null)
                {
                    DataGridViewRow row = dataGridView1.CurrentRow;

                    // Populate form fields with selected row data
                    textSPID.Text = row.Cells["SP_ID"].Value.ToString();
                    textBags.Text = row.Cells["QtyBag"].Value?.ToString() ?? "";
                    textTotalBags.Text = row.Cells["TotalQtyBags"].Value?.ToString() ?? "";
                    textRemBags.Text = row.Cells["RemBags"].Value?.ToString() ?? "";
                    textWeight.Text = row.Cells["Weight"].Value?.ToString() ?? "";
                    textRate.Text = row.Cells["Rate"].Value?.ToString() ?? "";
                    textAmount.Text = row.Cells["Amount"].Value?.ToString() ?? "";
                    comboParty.SelectedIndex = 0;
                    comboProduct.SelectedIndex = 0;
                    //Status.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting Items: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            */
        }
        private decimal? GetSafeDecimalValue(object value)
        {
            if (value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
                return 0;

            if (decimal.TryParse(value.ToString(), out decimal result))
                return result;

            return 0;
        }
        // Helper method to safely parse integer values
        private int? GetSafeIntValue(object value)
        {
            if (value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
                return 0;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return 0;
        }
        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if double-click is on a valid row (not header)
            if (e.RowIndex < 0) return;

            try
            {
                // Get the selected row
                DataGridViewRow row = dataGridView2.Rows[e.RowIndex];

                // Check if there's data in the row
                if (row.Cells["OrderID"].Value == null) return;

                // Get order ID
                string orderId = row.Cells["OrderID"].Value?.ToString() ?? "";
                textOrderID.Text = orderId;
                orderID = Convert.ToInt32(orderId);

                // Load customer information
                if (row.Cells["Customer_Name"].Value != null)
                {
                    textCustName.Text = row.Cells["Customer_Name"].Value.ToString();

                    // Try to find and select customer in the list
                    SearchCustomer(textCustName.Text);

                    // Auto-select if found in list
                    foreach (ListViewItem item in listViewResults.Items)
                    {
                        if (item.SubItems[1].Text.Equals(textCustName.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            item.Selected = true;
                            CustID = Convert.ToInt32(item.SubItems[0].Text);
                            break;
                        }
                    }
                }

                // Load date
                if (row.Cells["OrderDate"].Value != null && DateTime.TryParse(row.Cells["OrderDate"].Value.ToString(), out DateTime orderDate))
                {
                    dateTimeDate.Value = orderDate;
                }
                else
                {
                    dateTimeDate.Value = DateTime.Today;
                }

                // Load payment fields with safe parsing
                textCredit.Text = GetSafeDecimalValue(row.Cells["Credit"].Value)?.ToString("N2") ?? "0.00";
                textDebit.Text = GetSafeDecimalValue(row.Cells["Debit"].Value)?.ToString("N2") ?? "0.00";
                textCash.Text = GetSafeDecimalValue(row.Cells["Cash"].Value)?.ToString("N2") ?? "0.00";
                textBalance.Text = GetSafeDecimalValue(row.Cells["Balance"].Value)?.ToString("N2") ?? "0.00";

                // Determine and set status based on payment values
                decimal credit = GetSafeDecimalValue(row.Cells["Credit"].Value) ?? 0;
                decimal debit = GetSafeDecimalValue(row.Cells["Debit"].Value) ?? 0;
                decimal cash = GetSafeDecimalValue(row.Cells["Cash"].Value) ?? 0;

                if (cash > 0 && credit == 0 && debit == 0)
                {
                    comboStatus.SelectedIndex = 1; // Cash
                }
                else if (credit > 0 && cash == 0 && debit == 0)
                {
                    comboStatus.SelectedIndex = 2; // Credit
                }
                else if (debit > 0 && cash == 0 && credit == 0)
                {
                    comboStatus.SelectedIndex = 3; // Debit
                }
                else
                {
                    // Mixed payment or zero values
                    comboStatus.SelectedIndex = 0; // -- Select Status --

                    // If there's any payment, show a message
                    if (cash > 0 || credit > 0 || debit > 0)
                    {
                        MessageBox.Show("This order has mixed payment methods. Please review manually.",
                                      "Payment Info",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // Load items for this order
                LoadOrderItems(orderId);

                // Generate new SPID for adding new items to this order
                GenerateSaleItemID();

                // Clear item fields for new entry
                ItemClear();

                // Reset party and product selections
                comboParty.SelectedIndex = 0;
                comboProduct.SelectedIndex = 0;

                // Update button Visiblity true
                button10.Enabled = true;
                button7.Enabled = true;
                button6.Enabled = false;

                // Focus on customer name field
                textCustName.Focus();
                textCustName.SelectAll();

                // Show status message
                toolStripStatusLabel1.Text = $"Status: Loaded Order #{orderId} for editing";

                // Optional: Highlight the selected order in the grid
                dataGridView2.ClearSelection();
                row.Selected = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading order: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                               "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button10_Click(object sender, EventArgs e)
        {
            if (textCustName.Text == null)
            {
                MessageBox.Show("Please enter customer name!");
                return;
            }

            if (textCash.Text == null && textCredit.Text == null && textDebit.Text == null && textBalance.Text == null)
            {
                MessageBox.Show("Some of the field not added to updated");
                return;
            }

            // Check if product is selected
            if (comboStatus.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a Status from (Cash, Credit, Debit)!");
                return;
            }

            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"delete from Orders where OrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Use the stored selectedProductID variable
                        cmd.Parameters.AddWithValue("@OrderID", textOrderID.Text.Trim());
                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Order record deleted successfully!");

                            OrderClear();
                            ItemClear();
                            ItemGridView(textOrderID.Text);
                            OrderGridView();
                            comboParty.SelectedIndex = 0;
                            comboProduct.SelectedIndex = 0;
                            comboStatus.SelectedIndex = 0;
                            AutoGenerateOrderID();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete Orders record! Record not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Method to load row data into form fields
        

        // Example of Save/Update button click handler
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if double-click is on a valid row (not header)
            if (e.RowIndex < 0) return;

            try
            {
                // Get the selected row
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Check if there's data in the row
                if (row.Cells["SP_ID"].Value == null) return;

                // Load basic text fields
                textSPID.Text = row.Cells["SP_ID"].Value?.ToString() ?? "";
                
                textWeight.Text = row.Cells["Weight"].Value?.ToString() ?? "";
                textRate.Text = row.Cells["Rate"].Value?.ToString() ?? "";
                textAmount.Text = row.Cells["Amount"].Value?.ToString() ?? "0";
                //textDebit.Text = row.Cells["Debit"].Value?.ToString() ?? "0";

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

                    bool productFound = false;

                    // Check if products are loaded
                    if (comboProduct.Items.Count > 1) // More than just "-- Select Product --"
                    {
                        // Loop through product items (skip first item)
                        for (int i = 1; i < comboProduct.Items.Count; i++)
                        {
                            dynamic item = comboProduct.Items[i];
                            string itemDisplay = item.Display.ToString().Trim();

                            // Check if product name matches
                            if (itemDisplay.Equals(productNameFromGrid, StringComparison.OrdinalIgnoreCase))
                            {
                                comboProduct.SelectedIndex = i;

                                // Update the selected product variables
                                selectedProductID = Convert.ToInt32(item.Value);
                                selectedProductName = item.Display.ToString();
                                selectedBrand = item.Brand.ToString();

                                productFound = true;

                                break;
                            }
                        }

                        if (!productFound)
                        {
                            // Try fuzzy matching for product
                            for (int i = 1; i < comboProduct.Items.Count; i++)
                            {
                                dynamic item = comboProduct.Items[i];
                                string itemDisplay = item.Display.ToString().Trim();

                                if (itemDisplay.IndexOf(productNameFromGrid, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    comboProduct.SelectedIndex = i;

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
                        comboProduct.SelectedIndex = 0;
                        selectedProductID = 0;
                        selectedProductName = "";
                        selectedBrand = "";
                    }
                }

                button4.Enabled = true;
                button2.Enabled = false;
                textBags.Text = row.Cells["QtyBag"].Value?.ToString() ?? "";
                textTotalBags.Text = row.Cells["TotalQtyBags"].Value?.ToString() ?? "";
                textRemBags.Text = row.Cells["RemBags"].Value?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading record: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                               "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

