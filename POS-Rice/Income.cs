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
    public partial class Income : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
        public Income()
        {
            InitializeComponent();
        }

        private void ClearForm()
        {
            ///textCustID.Text = "0";
            textCashEntry.Text = "0";
            //textCustID.Clear();
            txtSearch.Clear();
            dataGridViewCashBook.ClearSelection();
            textCashEntry.Focus();
        }
        private void AutoGenerateCashID()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(CashID), 0) + 1 FROM CashBook", conn);
                int nextID = Convert.ToInt32(cmd.ExecuteScalar());
                lblCashID.Text = nextID.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Cash ID: " + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void LoadCustSummary()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
    CA.CustID,
    CA.Customer_Name,
    ISNULL((SELECT SUM(Cash_Entry) FROM CashBook WHERE CustID = CA.CustID), 0) as Credited,
    ISNULL((SELECT SUM(Credit) FROM Orders WHERE CustID = CA.CustID), 0) as Debited,
    ISNULL((SELECT SUM(Cash_Entry) FROM CashBook WHERE CustID = CA.CustID), 0) 
    - ISNULL((SELECT SUM(Credit) FROM Orders WHERE CustID = CA.CustID), 0) as Balance
FROM CustomerAct CA
ORDER BY CA.Customer_Name";

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridViewBalanceSheet.DataSource = dt;

                        // Format columns
                        dataGridViewBalanceSheet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridViewBalanceSheet.Columns["Customer_Name"].HeaderText = "Customer Name";
                        //dataGridViewBalanceSheet.Columns["Credited"].HeaderText = "Credited";
                        //dataGridViewBalanceSheet.Columns["Date"].HeaderText = "Cash Date";
                        //dataGridViewBalanceSheet.Columns["CustID"].HeaderText = "Customer ID";

                        dataGridViewBalanceSheet.Columns["Credited"].DefaultCellStyle.Format = "N2";
                        dataGridViewBalanceSheet.Columns["Debited"].DefaultCellStyle.Format = "N2";
                        dataGridViewBalanceSheet.Columns["Balance"].DefaultCellStyle.Format = "N2";

                        // if (dataGridViewBalanceSheet.Columns["Debited"] != null)
                        //   dataGridViewCashBook.Columns["Debited"].DefaultCellStyle.Format = "N2";

                        // if (dataGridViewBalanceSheet.Columns["Balance"] != null)
                        //  dataGridViewCashBook.Columns["Balance"].DefaultCellStyle.Format = "N2";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading cash book data: {ex.Message}");
            }
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
                        dataGridViewCashBook.DataSource = dt;

                        // Format columns
                        dataGridViewCashBook.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridViewCashBook.Columns["CashID"].HeaderText = "Cash ID";
                        dataGridViewCashBook.Columns["Cash_Entry"].HeaderText = "Cash Amount";
                        dataGridViewCashBook.Columns["Date"].HeaderText = "Cash Date";
                        dataGridViewCashBook.Columns["CustID"].HeaderText = "Customer ID";

                        // Format numeric column
                        if (dataGridViewCashBook.Columns["Cash_Entry"] != null)
                            dataGridViewCashBook.Columns["Cash_Entry"].DefaultCellStyle.Format = "N2";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading cash book data: {ex.Message}");
            }
        }
        private void SearchOrders(string searchTerm)
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
                    // AGGREGATED VERSION: If you want totals per customer
                    string query = @"
            SELECT 
                O.CustID,
                O.Customer_Name,
                SUM(O.Credit) as TotalCredit,
                SUM(O.Debit) as TotalDebit, 
                SUM(O.Cash) as TotalCash, 
                SUM(O.Balance) as TotalBalance
            FROM Orders O 
            JOIN CustomerAct C on O.CustID = C.CustID 
            WHERE O.Customer_Name LIKE @SearchTerm 
            GROUP BY O.CustID, O.Customer_Name
            ORDER BY O.Customer_Name";

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
                                item.SubItems.Add(reader["Customer_Name"].ToString());

                                // Use the aggregated column names
                                decimal credit = reader["TotalCredit"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCredit"]) : 0;
                                decimal debit = reader["TotalDebit"] != DBNull.Value ? Convert.ToDecimal(reader["TotalDebit"]) : 0;
                                decimal cash = reader["TotalCash"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCash"]) : 0;
                                decimal balance = reader["TotalBalance"] != DBNull.Value ? Convert.ToDecimal(reader["TotalBalance"]) : 0;

                                item.SubItems.Add(credit.ToString("N2"));
                                item.SubItems.Add(debit.ToString("N2"));
                                item.SubItems.Add(cash.ToString("N2"));
                                item.SubItems.Add(balance.ToString("N2"));

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

        private void LoadOrderDetails(string orderID)
        {
            // Implement your order loading logic here
            MessageBox.Show($"Loading Order: {orderID}", "Order Selected",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void Income_Load(object sender, EventArgs e)
        {
            LoadCashBookData();
            //ClearForm();
            AutoGenerateCashID();
            LoadCustSummary();



        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            SearchOrders(txtSearch.Text.Trim());
        }

        private void listViewResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewResults.SelectedItems[0];
                string custID = selectedItem.SubItems[0].Text;
                //string customerName = selectedItem.SubItems[1].Text;
                //string amount = selectedItem.SubItems[3].Text;

                // You can use these values to load the full order details
                textCustID.Text = custID;
                //textCashEntry.Text = "0";
                
                //LoadOrderDetails(orderID);
            }
        }

        private void listViewResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && listViewResults.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewResults.SelectedItems[0];
                string orderID = selectedItem.SubItems[0].Text;
                LoadOrderDetails(orderID);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && listViewResults.Items.Count > 0)
            {
                listViewResults.Focus();
                if (listViewResults.Items.Count > 0)
                    listViewResults.Items[0].Selected = true;
            }
        }

        private void textAmount_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
        
            // Validate inputs
            if (string.IsNullOrEmpty(textCustID.Text) || textCustID.Text == "0")
            {
                MessageBox.Show("Please enter a valid Customer ID!");
                return;
            }

            if (string.IsNullOrEmpty(textCashEntry.Text) || !decimal.TryParse(textCashEntry.Text, out _))
            {
                MessageBox.Show("Please enter a valid cash amount!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO CashBook 
                   (CashID, Cash_Entry, CustID, Date) 
                   VALUES 
                   (@CashID, @Cash_Entry, @CustID, @Date)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CashID", Convert.ToInt32(lblCashID.Text));
                        cmd.Parameters.AddWithValue("@Cash_Entry", Convert.ToDecimal(textCashEntry.Text));
                        cmd.Parameters.AddWithValue("@CustID", Convert.ToInt32(textCustID.Text));
                        cmd.Parameters.AddWithValue("@Date", dateTimeDate.Value);

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Cash book record added successfully!");

                            // Refresh data and clear form
                            ClearForm();
                            LoadCashBookData();
                            AutoGenerateCashID();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add cash book record!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding cash book record: " + ex.Message);
            }
        }
        

        private void button2_Click(object sender, EventArgs e)
        {
            
            // Validate inputs
            if (string.IsNullOrEmpty(lblCashID.Text) || lblCashID.Text == "0")
            {
                MessageBox.Show("Please select a record to update!");
                return;
            }

            if (string.IsNullOrEmpty(textCustID.Text) || textCustID.Text == "0")
            {
                MessageBox.Show("Please enter a valid Customer ID!");
                return;
            }

            if (string.IsNullOrEmpty(textCashEntry.Text) || !decimal.TryParse(textCashEntry.Text, out _))
            {
                MessageBox.Show("Please enter a valid cash amount!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"UPDATE CashBook 
                   SET 
                    Cash_Entry = @Cash_Entry,
                    CustID = @CustID
                   WHERE CashID = @CashID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CashID", Convert.ToInt32(lblCashID.Text));
                        cmd.Parameters.AddWithValue("@Cash_Entry", Convert.ToDecimal(textCashEntry.Text));
                        cmd.Parameters.AddWithValue("@CustID", Convert.ToInt32(textCustID.Text));

                        conn.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Cash book record updated successfully!");

                            // Refresh data and clear form
                            ClearForm();
                            LoadCashBookData();
                            AutoGenerateCashID();
                        }
                        else
                        {
                            MessageBox.Show("Failed to update cash book record! Record not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating cash book record: " + ex.Message);
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            ClearForm();
            LoadCashBookData();
            AutoGenerateCashID();
            
        }

        private void dataGridViewCashBook_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewCashBook.CurrentRow != null && dataGridViewCashBook.CurrentRow.Cells["CashID"].Value != null)
            {
                DataGridViewRow row = dataGridViewCashBook.CurrentRow;

                lblCashID.Text = row.Cells["CashID"].Value?.ToString() ?? "";
                textCashEntry.Text = row.Cells["Cash_Entry"].Value?.ToString() ?? "";
                textCustID.Text = row.Cells["CustID"].Value?.ToString() ?? "";
            }
        }
    }
}

