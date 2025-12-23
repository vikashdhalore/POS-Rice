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
    public partial class Stock : Form
    {
        string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

        string SelectedType = "";
        public Stock()
        {
            InitializeComponent();
        }
        private void LoadStockReport()
        {
            try
            {
                string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
            SELECT 
                P.PartyName,
                PD.ProductName,
                PD.Brand,
                ISNULL((SELECT SUM(Bags) FROM ProInventory WHERE PartyID = P.PartyID AND ProductID = PD.ProductID), 0) as [Total Bags],
                ISNULL((SELECT SUM(QtyBag) FROM SaleProduct WHERE PartyID = P.PartyID AND ProductID = PD.ProductID), 0) as [Sold Bags],
                ISNULL((SELECT SUM(Bags) FROM ProInventory WHERE PartyID = P.PartyID AND ProductID = PD.ProductID), 0) 
                - ISNULL((SELECT SUM(QtyBag) FROM SaleProduct WHERE PartyID = P.PartyID AND ProductID = PD.ProductID), 0) as [Available Stock],
                ISNULL((SELECT SUM(Amount) FROM ProInventory WHERE PartyID = P.PartyID And ProductID = PD.ProductID), 0) as [PURCHASED AMOUNT],
                ISNULL((SELECT SUM(Amount) FROM SaleProduct WHERE PartyID = P.PartyID AND ProductID = PD.ProductID),0 ) AS [SOLD AMOUNT],
                ISNULL((SELECT SUM(Amount) FROM ProInventory WHERE PartyID = P.PartyID And ProductID = PD.ProductID), 0)
                -ISNULL((SELECT SUM(Amount) FROM SaleProduct WHERE PartyID = P.PartyID AND ProductID = PD.ProductID),0 ) AS [BALANCE]
            FROM Party P
            INNER JOIN Product PD ON P.PartyID = PD.PartyID
            ORDER BY P.PartyName, PD.ProductName";

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dataGridViewStock.DataSource = dt;

                        // Format columns
                        //dataGridViewStock.Columns["PartyID"].HeaderText = "Party ID";
                        dataGridViewStock.Columns["PartyName"].HeaderText = "PARTY NAME";
                        //dataGridViewStock.Columns["ProductID"].HeaderText = "Product ID";
                        dataGridViewStock.Columns["ProductName"].HeaderText = "PRODUCT NAME";
                        dataGridViewStock.Columns["Brand"].HeaderText = "BRAND";
                        dataGridViewStock.Columns["Total Bags"].HeaderText = "TOTAL BAGS";
                        dataGridViewStock.Columns["Sold Bags"].HeaderText = "SOLD BAGS";
                        dataGridViewStock.Columns["Available Stock"].HeaderText = "AVAILABLE STOCK";


                        // Format numeric columns
                        dataGridViewStock.Columns["Total Bags"].DefaultCellStyle.Format = "N0";
                        dataGridViewStock.Columns["Sold Bags"].DefaultCellStyle.Format = "N0";
                        dataGridViewStock.Columns["Available Stock"].DefaultCellStyle.Format = "N0";

                        // Color code available stock
                        foreach (DataGridViewRow row in dataGridViewStock.Rows)
                        {
                            if (row.Cells["Available Stock"].Value != null)
                            {
                                int availableStock = Convert.ToInt32(row.Cells["Available Stock"].Value);
                                if (availableStock <= 0)
                                {
                                    row.Cells["Available Stock"].Style.ForeColor = Color.DarkRed;
                                    row.Cells["Available Stock"].Style.BackColor = Color.LightPink;
                                }
                                else if (availableStock < 10)
                                {
                                    row.Cells["Available Stock"].Style.ForeColor = Color.DarkOrange;
                                    row.Cells["Available Stock"].Style.BackColor = Color.LightYellow;
                                }
                                else
                                {
                                    row.Cells["Available Stock"].Style.ForeColor = Color.DarkGreen;
                                    row.Cells["Available Stock"].Style.BackColor = Color.LightGreen;
                                }
                            }

                            if (row.Cells["BALANCE"].Value != null)
                            {
                                int profitLoss = Convert.ToInt32(row.Cells["BALANCE"].Value);
                                if (profitLoss <= 0)
                                {
                                    row.Cells["BALANCE"].Style.ForeColor = Color.DarkRed;
                                    row.Cells["BALANCE"].Style.BackColor = Color.LightPink;
                                }
                                else
                                {
                                    row.Cells["Available Stock"].Style.ForeColor = Color.DarkGreen;
                                    row.Cells["Available Stock"].Style.BackColor = Color.LightGreen;
                                }
                            }
                        }

                        // Auto-size columns
                        dataGridViewStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading stock report: {ex.Message}");
            }
        }

        private void LoadParties()
        {
            // Clear existing items
            comboSelectedType.Items.Clear();

            // Add "All Parties" option if needed
            // comboFilter.Items.Add("All Parties");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT PartyID, PartyName FROM Party ORDER BY PartyName";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Store ID in Tag and show Name
                            string partyName = reader["PartyName"].ToString();
                            int partyId = Convert.ToInt32(reader["PartyID"]);

                            // You can add as string only
                            comboSelectedType.Items.Add(partyName);

                            // OR if you want to store ID separately, use a class or Tuple
                            // comboFilter.Items.Add(new { ID = partyId, Name = partyName });
                        }
                    }
                }
            }

            //if (comboFilter.Items.Count > 0)
              //  comboFilter.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            comboSelectedType.Items.Clear();
            // comboFilter.Items.Add("All Products"); // Optional

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ProductID, ProductName FROM Product ORDER BY ProductName";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productName = reader["ProductName"].ToString();
                            comboSelectedType.Items.Add(productName);
                        }
                    }
                }
            }

            //if (comboSelectedType.Items.Count > 0)
              //  comboFilter.SelectedIndex = 0;
        }

        private void LoadBrands()
        {
            comboSelectedType.Items.Clear();
            // comboFilter.Items.Add("All Brands"); // Optional

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Get DISTINCT brands from Product table
                string query = "SELECT DISTINCT Brand FROM Product WHERE Brand IS NOT NULL AND Brand <> '' ORDER BY Brand";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string brand = reader["Brand"].ToString();
                            comboSelectedType.Items.Add(brand);
                        }
                    }
                }
            }

            //if (comboSelectedType.Items.Count > 0)
                //comboFilter.SelectedIndex = 0;
        }


        /* private void loadStock() 
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
                 string query = "select P.PartyName, PD.ProductName,Sum(SP.QtyBag) As [Sold Bags],SUM(I.Bags) as [Total Bags], (sum(I.Bags) - sum(SP.QtyBag)) As [Available Stock] from SaleProduct SP lEFT JOIN Party P on P.PartyID = SP.PartyID lEFT JOIN Product PD on PD.ProductID = SP.ProductID lEFT JOIN ProInventory I on I.PartyID = SP.PartyID And I.ProductID = SP.ProductID Group by P.PartyID,P.PartyName,PD.ProductID,PD.ProductName";

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
                     dataGridView1.Columns["PartyName"].HeaderText = "Party";
                     dataGridView1.Columns["ProductName"].HeaderText = "Product";



                     // Optional: Auto-size columns
                     dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                     // Add checkbox column after data binding
                     //AddCheckBoxColumn();
                     //ClearForm();
                 }
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Error loading data: {ex.Message}", "Database Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }
        */

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Stock_Load(object sender, EventArgs e)
        {
            LoadStockReport();
            comboFilter.SelectedItem = "Select Type";
            comboSelectedType.Items.Clear();

            //loadStock();
        }

        private void LoadStockData(string search = "")
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // SQL select query for stock data
                    string query = @"
            SELECT 
                P.PartyName,
                PD.ProductName,
                PD.Brand,
                ISNULL((SELECT SUM(Bags) FROM ProInventory WHERE PartyID = P.PartyID AND ProductID = PD.ProductID), 0) as [Total Bags],
                ISNULL((SELECT SUM(QtyBag) FROM SaleProduct WHERE PartyID = P.PartyID AND ProductID = PD.ProductID), 0) as [Sold Bags],
                ISNULL((SELECT SUM(Bags) FROM ProInventory WHERE PartyID = P.PartyID AND ProductID = PD.ProductID), 0) 
                - ISNULL((SELECT SUM(QtyBag) FROM SaleProduct WHERE PartyID = P.PartyID AND ProductID = PD.ProductID), 0) as [Available Stock],
                ISNULL((SELECT SUM(Amount) FROM ProInventory WHERE PartyID = P.PartyID And ProductID = PD.ProductID), 0) as [PURCHASED AMOUNT],
                ISNULL((SELECT SUM(Amount) FROM SaleProduct WHERE PartyID = P.PartyID AND ProductID = PD.ProductID),0 ) AS [SOLD AMOUNT],
                ISNULL((SELECT SUM(Amount) FROM ProInventory WHERE PartyID = P.PartyID And ProductID = PD.ProductID), 0)
                -ISNULL((SELECT SUM(Amount) FROM SaleProduct WHERE PartyID = P.PartyID AND ProductID = PD.ProductID),0 ) AS [BALANCE]
            FROM Party P
            INNER JOIN Product PD ON P.PartyID = PD.PartyID
            WHERE P.PartyName LIKE @search OR PD.ProductName LIKE @search OR PD.Brand LIKE @search 
               OR PD.ProductName LIKE @search 
               OR PD.Brand LIKE @search
            ORDER BY P.PartyName, PD.ProductName";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                    // Create connection and data adapter               
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);

                    // Create DataTable to hold the data
                    DataTable dataTable = new DataTable();

                    // Fill DataTable with data from database
                    dataAdapter.Fill(dataTable);

                    // Set DataGridView data source
                    dataGridViewStock.DataSource = dataTable;

                    // Configure column headers
                    //dataGridViewStock.Columns["PartyID"].HeaderText = "Party ID";
                    dataGridViewStock.Columns["PartyName"].HeaderText = "Party Name";
                    //dataGridViewStock.Columns["ProductID"].HeaderText = "Product ID";
                    dataGridViewStock.Columns["ProductName"].HeaderText = "Product Name";
                    dataGridViewStock.Columns["Brand"].HeaderText = "Brand";
                    dataGridViewStock.Columns["Total Bags"].HeaderText = "Total Bags";
                    dataGridViewStock.Columns["Sold Bags"].HeaderText = "Sold Bags";
                    dataGridViewStock.Columns["Available Stock"].HeaderText = "Available Stock";

                    // Format numeric columns
                    dataGridViewStock.Columns["Total Bags"].DefaultCellStyle.Format = "N0";
                    dataGridViewStock.Columns["Sold Bags"].DefaultCellStyle.Format = "N0";
                    dataGridViewStock.Columns["Available Stock"].DefaultCellStyle.Format = "N0";

                    foreach (DataGridViewRow row in dataGridViewStock.Rows)
                    {
                        if (row.Cells["Available Stock"].Value != null)
                        {
                            int availableStock = Convert.ToInt32(row.Cells["Available Stock"].Value);
                            if (availableStock <= 0)
                            {
                                row.Cells["Available Stock"].Style.ForeColor = Color.DarkRed;
                                row.Cells["Available Stock"].Style.BackColor = Color.LightPink;
                            }
                            else if (availableStock < 10)
                            {
                                row.Cells["Available Stock"].Style.ForeColor = Color.DarkOrange;
                                row.Cells["Available Stock"].Style.BackColor = Color.LightYellow;
                            }
                            else
                            {
                                row.Cells["Available Stock"].Style.ForeColor = Color.DarkGreen;
                                row.Cells["Available Stock"].Style.BackColor = Color.LightGreen;
                            }
                        }

                        if (row.Cells["BALANCE"].Value != null)
                        {
                            int profitLoss = Convert.ToInt32(row.Cells["BALANCE"].Value);
                            if (profitLoss <= 0)
                            {
                                row.Cells["BALANCE"].Style.ForeColor = Color.DarkRed;
                                row.Cells["BALANCE"].Style.BackColor = Color.LightPink;
                            }
                            else
                            {
                                row.Cells["Available Stock"].Style.ForeColor = Color.DarkGreen;
                                row.Cells["Available Stock"].Style.BackColor = Color.LightGreen;
                            }
                        }
                    }


                    // Auto-size columns
                    dataGridViewStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Color code available stock (optional enhancement)
                    ColorCodeStockLevels();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading stock data: {ex.Message}", "Database Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Optional: Method to color code stock levels
        private void ColorCodeStockLevels()
        {
            foreach (DataGridViewRow row in dataGridViewStock.Rows)
            {
                if (row.Cells["Available Stock"].Value != null)
                {
                    int availableStock = Convert.ToInt32(row.Cells["Available Stock"].Value);
                    if (availableStock <= 0)
                    {
                        row.Cells["Available Stock"].Style.ForeColor = Color.DarkRed;
                        row.Cells["Available Stock"].Style.BackColor = Color.LightPink;
                    }
                    else if (availableStock < 10)
                    {
                        row.Cells["Available Stock"].Style.ForeColor = Color.DarkOrange;
                        row.Cells["Available Stock"].Style.BackColor = Color.LightYellow;
                    }
                    else
                    {
                        row.Cells["Available Stock"].Style.ForeColor = Color.DarkGreen;
                        row.Cells["Available Stock"].Style.BackColor = Color.LightGreen;
                    }
                }
            }
        }

        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            LoadStockData(textSearchStock.Text.Trim());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadStockData(SelectedType);
        }

        private void comboSelectedType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedType = comboSelectedType.SelectedItem.ToString();
        }

        private void comboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedType = comboFilter.SelectedItem?.ToString();
            comboSelectedType.Items.Clear();
            comboSelectedType.SelectedIndex = -1;
            //comboFilter.Text = "";

            if (string.IsNullOrEmpty(selectedType))
                return;

            try
            {
                switch (selectedType)
                {
                    case "Party":
                        LoadParties();
                        break;
                    case "Product":
                        LoadProducts();
                        break;
                    case "Brand":
                        LoadBrands();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading {selectedType}: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            comboFilter.SelectedItem = "Select Type";
            comboSelectedType.Items.Clear();
            comboSelectedType.SelectedIndex = -1;
            SelectedType = "";
            LoadStockReport();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Simply pass your DataGridView to the PDF generator
            PDFStockReportGenerator pdfGenerator = new PDFStockReportGenerator();
            pdfGenerator.GenerateStockPDFFromGrid(dataGridViewStock, SelectedType);
        }
    }
}
