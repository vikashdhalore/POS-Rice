using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class frmSaleExpenseReport : Form
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

        // Controls
        private DateTimePicker dateFrom, dateTo;
        private ComboBox comboReportType;
        private Button btnGenerate, btnExport, btnPrint;
        private DataGridView dataGridViewReport;
        private Panel panelSummary;
        private Label lblTotalSales, lblTotalExpenses, lblNetProfit, lblProfitMargin;
        private Label lblSalesCount, lblExpenseCount, lblAvgSale, lblAvgExpense;

        public frmSaleExpenseReport()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            // Form settings
            this.Text = "Sales vs Expenses Report";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create controls
            CreateControls();
            LoadReportTypes();
        }

        private void CreateControls()
        {
            // Labels
            var lblFrom = new Label { Text = "From Date:", Location = new Point(20, 20), Size = new Size(70, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblTo = new Label { Text = "To Date:", Location = new Point(220, 20), Size = new Size(70, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblReportType = new Label { Text = "Report Type:", Location = new Point(420, 20), Size = new Size(80, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Date Pickers
            dateFrom = new DateTimePicker { Location = new Point(90, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };
            dateTo = new DateTimePicker { Location = new Point(290, 20), Size = new Size(120, 20), Format = DateTimePickerFormat.Short };

            // Set default dates (last 30 days)
            dateFrom.Value = DateTime.Now.AddDays(-30);
            dateTo.Value = DateTime.Now;

            // ComboBox
            comboReportType = new ComboBox { Location = new Point(500, 20), Size = new Size(200, 20), DropDownStyle = ComboBoxStyle.DropDownList };

            // Buttons
            btnGenerate = new Button { Text = "Generate Report", Location = new Point(720, 18), Size = new Size(120, 25), BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnExport = new Button { Text = "Export Excel", Location = new Point(850, 18), Size = new Size(100, 25), BackColor = Color.Green, ForeColor = Color.White };
            btnPrint = new Button { Text = "Print", Location = new Point(960, 18), Size = new Size(80, 25), BackColor = Color.Orange, ForeColor = Color.White };

            // DataGridView
            dataGridViewReport = new DataGridView { Location = new Point(20, 60), Size = new Size(1150, 400) };

            // Summary Panel
            panelSummary = new Panel { Location = new Point(20, 470), Size = new Size(1150, 120), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightYellow };

            // Main Summary Labels (Top Row)
            lblTotalSales = new Label { Text = "Total Sales: RS 0.00", Location = new Point(20, 15), Size = new Size(230, 25), Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.DarkGreen };
            lblTotalExpenses = new Label { Text = "Total Expenses: RS 0.00", Location = new Point(250, 15), Size = new Size(230, 25), Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.DarkRed };
            lblNetProfit = new Label { Text = "Net Profit: RS 0.00", Location = new Point(550, 15), Size = new Size(230, 25), Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.DarkBlue };
            lblProfitMargin = new Label { Text = "Profit Margin: 0.00%", Location = new Point(800, 15), Size = new Size(200, 25), Font = new Font("Arial", 12, FontStyle.Bold), ForeColor = Color.Purple };

            // Secondary Summary Labels (Middle Row)
            lblSalesCount = new Label { Text = "Sales Transactions: 0", Location = new Point(20, 50), Size = new Size(230, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            lblExpenseCount = new Label { Text = "Expense Transactions: 0", Location = new Point(250, 50), Size = new Size(230, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            lblAvgSale = new Label { Text = "Avg Sale: RS 0.00", Location = new Point(550, 50), Size = new Size(230, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            lblAvgExpense = new Label { Text = "Avg Expense: RS 0.00", Location = new Point(800, 50), Size = new Size(230, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Additional Metrics (Bottom Row)
            var lblDailySales = new Label { Text = "Daily Sales Avg: RS 0.00", Location = new Point(20, 80), Size = new Size(230, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblDailyExpenses = new Label { Text = "Daily Expense Avg: RS 0.00", Location = new Point(250, 80), Size = new Size(230, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblExpenseRatio = new Label { Text = "Expense/Sales Ratio: 0.00%", Location = new Point(550, 80), Size = new Size(230, 20), Font = new Font("Arial", 9, FontStyle.Bold) };
            var lblBreakEven = new Label { Text = "Break-even Point: RS 0.00", Location = new Point(800, 80), Size = new Size(230, 20), Font = new Font("Arial", 9, FontStyle.Bold) };

            // Add controls to panel
            panelSummary.Controls.AddRange(new Control[] {
                lblTotalSales, lblTotalExpenses, lblNetProfit, lblProfitMargin,
                lblSalesCount, lblExpenseCount, lblAvgSale, lblAvgExpense,
                lblDailySales, lblDailyExpenses, lblExpenseRatio, lblBreakEven
            });

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                lblFrom, lblTo, lblReportType,
                dateFrom, dateTo, comboReportType,
                btnGenerate, btnExport, btnPrint,
                dataGridViewReport, panelSummary
            });

            // Event handlers
            btnGenerate.Click += btnGenerate_Click;
            btnExport.Click += btnExport_Click;
            btnPrint.Click += btnPrint_Click;
            dataGridViewReport.RowPostPaint += DataGridViewReport_RowPostPaint;
        }

        private void LoadReportTypes()
        {
            comboReportType.Items.AddRange(new string[] {
                "Daily Summary - Sales vs Expenses",
                "Monthly Profit & Loss",
                "Category Wise Analysis",
                "Detailed Transaction View",
                "Profit Trend Analysis"
            });
            comboReportType.SelectedIndex = 0;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            GenerateSaleExpenseReport();
        }

        private void GenerateSaleExpenseReport()
        {
            try
            {
                string reportType = comboReportType.SelectedItem.ToString();
                DataTable dt = new DataTable();

                switch (reportType)
                {
                    case "Daily Summary - Sales vs Expenses":
                        dt = GetDailySummary();
                        break;
                    case "Monthly Profit & Loss":
                        dt = GetMonthlySummary();
                        break;
                    /*case "Category Wise Analysis":
                        dt = GetCategoryAnalysis();
                        break;
                    case "Detailed Transaction View":
                        dt = GetDetailedView();
                        break;
                    case "Profit Trend Analysis":
                        dt = GetProfitTrend();
                        break; */
                }

                dataGridViewReport.DataSource = dt;
                FormatDataGridView();
                CalculateFinancialSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable GetDailySummary()
        {
            DataTable dt = new DataTable();

            string query = @"
WITH DailySales AS (
    SELECT 
        CONVERT(DATE, O.OrderDate) AS TransactionDate,
        'Sale' AS TransactionType,
        SUM(ISNULL(O.Cash, 0) + ISNULL(O.Credit, 0) + ISNULL(O.Debit, 0)) AS SalesAmount,
        COUNT(O.OrderID) AS TransactionCount,
        -- Get total weight sold per day from SaleProduct
        ISNULL(SUM(SP.Weight), 0) AS TotalWeightSold
    FROM Orders O
    INNER JOIN SaleProduct SP ON O.OrderID = SP.OrderID
    WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
    GROUP BY CONVERT(DATE, O.OrderDate)
),
InventoryCosts AS (
    -- Calculate weighted average cost per product
    SELECT 
        PI.ProductID,
        -- Total amount spent on this product (Debit + Credit)
        SUM(ISNULL(PI.Debit, 0) + ISNULL(PI.Credit, 0)) AS TotalInventoryCost,
        -- Total weight purchased of this product
        SUM(ISNULL(PI.Weight, 0)) AS TotalInventoryWeight,
        -- Cost per unit weight
        CASE 
            WHEN SUM(ISNULL(PI.Weight, 0)) > 0 
            THEN SUM(ISNULL(PI.Debit, 0) + ISNULL(PI.Credit, 0)) / SUM(ISNULL(PI.Weight, 0))
            ELSE 0 
        END AS CostPerUnitWeight
    FROM ProInventory PI
    WHERE PI.InvDate <= @ToDate  -- All inventory up to the report end date
    GROUP BY PI.ProductID
),
DailySoldProducts AS (
    -- Get products sold each day with their weights
    SELECT 
        CONVERT(DATE, O.OrderDate) AS TransactionDate,
        SP.ProductID,
        SUM(SP.Weight) AS DailyWeightSold
    FROM Orders O
    INNER JOIN SaleProduct SP ON O.OrderID = SP.OrderID
    WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
    GROUP BY CONVERT(DATE, O.OrderDate), SP.ProductID
),
DailyInventoryCost AS (
    -- Calculate inventory cost for sold products each day
    SELECT 
        DSP.TransactionDate,
        'Inventory Cost' AS CostType,
        SUM(DSP.DailyWeightSold * ISNULL(IC.CostPerUnitWeight, 0)) AS InventoryCostAmount,
        COUNT(DISTINCT DSP.ProductID) AS ProductCount
    FROM DailySoldProducts DSP
    LEFT JOIN InventoryCosts IC ON DSP.ProductID = IC.ProductID
    GROUP BY DSP.TransactionDate
),
DailyLocalExpenses AS (
    SELECT 
        CONVERT(DATE, Local_Exp_Date) AS TransactionDate,
        'Local Expense' AS ExpenseType,
        SUM(ISNULL(Local_Exp_Amount, 0)) AS ExpenseAmount,
        COUNT(Local_Exp_ID) AS TransactionCount
    FROM LocalExpense
    WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate
    GROUP BY CONVERT(DATE, Local_Exp_Date)
),
CombinedDailyData AS (
    SELECT 
        TransactionDate,
        'Sale' AS TransactionType,
        SalesAmount AS Amount,
        TransactionCount,
        0 AS InventoryCost,
        0 AS LocalExpense
    FROM DailySales
    
    UNION ALL
    
    SELECT 
        TransactionDate,
        'Expense' AS TransactionType,
        0 AS Amount,
        0 AS TransactionCount,
        InventoryCostAmount AS InventoryCost,
        0 AS LocalExpense
    FROM DailyInventoryCost
    
    UNION ALL
    
    SELECT 
        TransactionDate,
        'Expense' AS TransactionType,
        0 AS Amount,
        TransactionCount,
        0 AS InventoryCost,
        ExpenseAmount AS LocalExpense
    FROM DailyLocalExpenses
)
SELECT 
    COALESCE(DS.TransactionDate, DIC.TransactionDate, DLE.TransactionDate) AS [Date],
    ISNULL(DS.SalesAmount, 0) AS TotalSales,
    ISNULL(DIC.InventoryCostAmount, 0) AS InventoryCost,
    ISNULL(DLE.ExpenseAmount, 0) AS LocalExpenses,
    ISNULL(DS.SalesAmount, 0) - 
    (ISNULL(DIC.InventoryCostAmount, 0) + ISNULL(DLE.ExpenseAmount, 0)) AS NetProfit,
    CASE 
        WHEN ISNULL(DS.SalesAmount, 0) > 0 
        THEN ((ISNULL(DS.SalesAmount, 0) - 
              (ISNULL(DIC.InventoryCostAmount, 0) + ISNULL(DLE.ExpenseAmount, 0))) / 
              ISNULL(DS.SalesAmount, 0)) * 100
        ELSE 0 
    END AS ProfitMargin,
    ISNULL(DS.TransactionCount, 0) AS SalesCount,
    ISNULL(DLE.TransactionCount, 0) AS ExpenseCount,
    ISNULL(DS.TotalWeightSold, 0) AS TotalWeightSold,
    ISNULL(DIC.ProductCount, 0) AS ProductsSoldCount
FROM DailySales DS
FULL OUTER JOIN DailyInventoryCost DIC ON DS.TransactionDate = DIC.TransactionDate
FULL OUTER JOIN DailyLocalExpenses DLE ON DS.TransactionDate = DLE.TransactionDate
ORDER BY COALESCE(DS.TransactionDate, DIC.TransactionDate, DLE.TransactionDate) DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        private DataTable GetMonthlySummary()
        {
            DataTable dt = new DataTable();

            string query = @"
    WITH MonthlySales AS (
        SELECT 
            FORMAT(O.OrderDate, 'yyyy-MM') AS MonthYear,
            YEAR(O.OrderDate) AS Year,
            MONTH(O.OrderDate) AS Month,
            'Sale' AS TransactionType,
            SUM(ISNULL(O.Cash, 0) + ISNULL(O.Credit, 0) + ISNULL(O.Debit, 0)) AS SalesAmount,
            COUNT(DISTINCT O.OrderID) AS SalesCount,
            SUM(ISNULL(SP.Weight, 0)) AS TotalWeightSold
        FROM Orders O
        INNER JOIN SaleProduct SP ON O.OrderID = SP.OrderID
        WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
        GROUP BY YEAR(O.OrderDate), MONTH(O.OrderDate), FORMAT(O.OrderDate, 'yyyy-MM')
    ),
    ProductCostRates AS (
        -- Calculate average cost rate per product (cost per unit weight)
        SELECT 
            PI.ProductID,
            -- Total cost = Debit (purchases) + Credit (returns)
            SUM(ISNULL(PI.Debit, 0) + ISNULL(PI.Credit, 0)) AS TotalCost,
            -- Total weight in inventory
            SUM(ISNULL(PI.Weight, 0)) AS TotalWeight,
            -- Cost rate = Total Cost / Total Weight
            CASE 
                WHEN SUM(ISNULL(PI.Weight, 0)) > 0 
                THEN SUM(ISNULL(PI.Debit, 0) + ISNULL(PI.Credit, 0)) / SUM(ISNULL(PI.Weight, 0))
                ELSE 0 
            END AS CostRate
        FROM ProInventory PI
        WHERE PI.InvDate <= @ToDate
        GROUP BY PI.ProductID
    ),
    MonthlySoldProducts AS (
        -- Get monthly sales with product details for cost calculation
        SELECT 
            FORMAT(O.OrderDate, 'yyyy-MM') AS MonthYear,
            YEAR(O.OrderDate) AS Year,
            MONTH(O.OrderDate) AS Month,
            SP.ProductID,
            SUM(SP.Weight) AS WeightSold,
            SUM(SP.Amount) AS AmountSold
        FROM Orders O
        INNER JOIN SaleProduct SP ON O.OrderID = SP.OrderID
        WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
        GROUP BY YEAR(O.OrderDate), MONTH(O.OrderDate), FORMAT(O.OrderDate, 'yyyy-MM'), SP.ProductID
    ),
    MonthlyInventoryCost AS (
        -- Calculate inventory cost for each month
        SELECT 
            MSP.MonthYear,
            MSP.Year,
            MSP.Month,
            SUM(MSP.WeightSold * ISNULL(PCR.CostRate, 0)) AS InventoryCost,
            COUNT(DISTINCT MSP.ProductID) AS ProductsSoldCount
        FROM MonthlySoldProducts MSP
        LEFT JOIN ProductCostRates PCR ON MSP.ProductID = PCR.ProductID
        GROUP BY MSP.MonthYear, MSP.Year, MSP.Month
    ),
    MonthlyLocalExpenses AS (
        SELECT 
            FORMAT(Local_Exp_Date, 'yyyy-MM') AS MonthYear,
            YEAR(Local_Exp_Date) AS Year,
            MONTH(Local_Exp_Date) AS Month,
            SUM(ISNULL(Local_Exp_Amount, 0)) AS LocalExpenses,
            COUNT(Local_Exp_ID) AS ExpenseCount
        FROM LocalExpense
        WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate
        GROUP BY YEAR(Local_Exp_Date), MONTH(Local_Exp_Date), FORMAT(Local_Exp_Date, 'yyyy-MM')
    ),
    CombinedMonthlyData AS (
        SELECT 
            MS.MonthYear,
            MS.Year,
            MS.Month,
            MS.SalesAmount,
            MS.SalesCount,
            MS.TotalWeightSold,
            ISNULL(MIC.InventoryCost, 0) AS InventoryCost,
            ISNULL(MLE.LocalExpenses, 0) AS LocalExpenses,
            ISNULL(MIC.ProductsSoldCount, 0) AS ProductsSoldCount,
            ISNULL(MLE.ExpenseCount, 0) AS ExpenseCount
        FROM MonthlySales MS
        LEFT JOIN MonthlyInventoryCost MIC ON MS.MonthYear = MIC.MonthYear
        LEFT JOIN MonthlyLocalExpenses MLE ON MS.MonthYear = MLE.MonthYear
    )
    SELECT 
        MonthYear AS [Month],
        DATENAME(MONTH, DATEFROMPARTS(Year, Month, 1)) + ' ' + CAST(Year AS VARCHAR(4)) AS MonthName,
        ISNULL(SalesAmount, 0) AS TotalSales,
        ISNULL(InventoryCost, 0) AS InventoryCost,
        ISNULL(LocalExpenses, 0) AS LocalExpenses,
        ISNULL(SalesAmount, 0) - (ISNULL(InventoryCost, 0) - ISNULL(LocalExpenses, 0)) AS NetProfit,
        CASE 
            WHEN ISNULL(SalesAmount, 0) > 0 
            THEN ((ISNULL(SalesAmount, 0) - (ISNULL(InventoryCost, 0) - ISNULL(LocalExpenses, 0))) / 
                  ISNULL(SalesAmount, 0)) * 100
            ELSE 0 
        END AS ProfitMargin,
        ISNULL(SalesCount, 0) AS SalesCount,
        ISNULL(ExpenseCount, 0) AS ExpenseCount,
        ISNULL(ProductsSoldCount, 0) AS ProductsSoldCount,
        ISNULL(TotalWeightSold, 0) AS TotalWeightSold,
        RANK() OVER (ORDER BY ISNULL(SalesAmount, 0) DESC) AS SalesRank
    FROM CombinedMonthlyData
    ORDER BY Year DESC, Month DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))


                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        /*private DataTable GetCategoryAnalysis()
        {
            DataTable dt = new DataTable();

            string query = @"
    WITH SalesByCustomer AS (
        SELECT 
            ISNULL(Customer_Name, 'Walk-in Customer') AS Category,
            'Sales' AS CategoryType,
            SUM(ISNULL(Cash, 0) + ISNULL(Credit, 0) + ISNULL(Debit, 0)) AS TotalSales,
            0 AS InventoryCost,
            0 AS LocalExpenses,
            SUM(ISNULL(Cash, 0) + ISNULL(Credit, 0) + ISNULL(Debit, 0)) AS NetProfit,
            COUNT(OrderID) AS TransactionCount,
            'Customer' AS SourceType
        FROM Orders
        WHERE OrderDate BETWEEN @FromDate AND @ToDate
        GROUP BY Customer_Name
    ),
    ProductCostRates AS (
        -- Calculate average cost rate per product (cost per unit weight)
        SELECT 
            PI.ProductID,
            CASE 
                WHEN SUM(ISNULL(PI.Weight, 0)) > 0 
                THEN SUM(ISNULL(PI.Debit, 0) + ISNULL(PI.Credit, 0)) / SUM(ISNULL(PI.Weight, 0))
                ELSE 0 
            END AS CostRate
        FROM ProInventory PI
        WHERE PI.InvDate <= @ToDate
        GROUP BY PI.ProductID
    ),
    CustomerProductSales AS (
        -- Get sales by customer and product
        SELECT 
            O.Customer_Name,
            SP.ProductID,
            SUM(SP.Weight) AS WeightSold,
            SUM(SP.Amount) AS AmountSold
        FROM Orders O
        INNER JOIN SaleProduct SP ON O.OrderID = SP.OrderID
        WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
        GROUP BY O.Customer_Name, SP.ProductID
    ),
    CustomerInventoryCost AS (
        -- Calculate inventory cost for each customer
        SELECT 
            ISNULL(CPS.Customer_Name, 'Walk-in Customer') AS Category,
            SUM(CPS.WeightSold * ISNULL(PCR.CostRate, 0)) AS InventoryCost
        FROM CustomerProductSales CPS
        LEFT JOIN ProductCostRates PCR ON CPS.ProductID = PCR.ProductID
        GROUP BY CPS.Customer_Name
    ),
    ExpensesByCategory AS (
        SELECT 
            ISNULL(ActName, 'General Expense') AS Category,
            'Expense' AS CategoryType,
            0 AS TotalSales,
            0 AS InventoryCost,
            SUM(ISNULL(Local_Exp_Amount, 0)) AS LocalExpenses,
            -SUM(ISNULL(Local_Exp_Amount, 0)) AS NetProfit,
            COUNT(Local_Exp_ID) AS TransactionCount,
            'Local Expense' AS SourceType
        FROM LocalExpense
        WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate
        GROUP BY ActName
    ),
    ProductSales AS (
        -- Sales by product
        SELECT 
            P.ProductName AS Category,
            'Sales by Product' AS CategoryType,
            SUM(SP.Amount) AS TotalSales,
            0 AS InventoryCost,
            0 AS LocalExpenses,
            SUM(SP.Amount) AS NetProfit,
            COUNT(DISTINCT SP.SP_ID) AS TransactionCount,
            'Product' AS SourceType
        FROM SaleProduct SP
        INNER JOIN Product P ON SP.ProductID = P.ProductID
        INNER JOIN Orders O ON SP.OrderID = O.OrderID
        WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
        GROUP BY P.ProductName
    ),
    ProductInventoryCost AS (
        -- Inventory cost by product
        SELECT 
            P.ProductName AS Category,
            'Inventory Cost' AS CategoryType,
            0 AS TotalSales,
            SUM(SP.Weight * ISNULL(PCR.CostRate, 0)) AS InventoryCost,
            0 AS LocalExpenses,
            -SUM(SP.Weight * ISNULL(PCR.CostRate, 0)) AS NetProfit,
            COUNT(DISTINCT SP.SP_ID) AS TransactionCount,
            'Product Cost' AS SourceType
        FROM SaleProduct SP
        INNER JOIN Product P ON SP.ProductID = P.ProductID
        INNER JOIN Orders O ON SP.OrderID = O.OrderID
        LEFT JOIN ProductCostRates PCR ON SP.ProductID = PCR.ProductID
        WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
        GROUP BY P.ProductName
    ),
    CombinedCategories AS (
        -- Combine all categories
        SELECT 
            SBC.Category,
            SBC.CategoryType,
            SBC.TotalSales,
            ISNULL(CIC.InventoryCost, 0) AS InventoryCost,
            0 AS LocalExpenses,
            SBC.TotalSales - ISNULL(CIC.InventoryCost, 0) AS NetProfit,
            SBC.TransactionCount,
            SBC.SourceType
        FROM SalesByCustomer SBC
        LEFT JOIN CustomerInventoryCost CIC ON SBC.Category = CIC.Category
        
        UNION ALL
        
        SELECT 
            Category,
            CategoryType,
            TotalSales,
            InventoryCost,
            LocalExpenses,
            NetProfit,
            TransactionCount,
            SourceType
        FROM ExpensesByCategory
        
        UNION ALL
        
        SELECT 
            Category,
            CategoryType,
            TotalSales,
            InventoryCost,
            LocalExpenses,
            NetProfit,
            TransactionCount,
            SourceType
        FROM ProductSales
        
        UNION ALL
        
        SELECT 
            Category,
            CategoryType,
            TotalSales,
            InventoryCost,
            LocalExpenses,
            NetProfit,
            TransactionCount,
            SourceType
        FROM ProductInventoryCost
    )
    SELECT 
        Category,
        CategoryType,
        SourceType,
        TotalSales,
        InventoryCost,
        LocalExpenses,
        InventoryCost + LocalExpenses AS TotalExpenses,
        NetProfit,
        CASE 
            WHEN TotalSales > 0 
            THEN (NetProfit / TotalSales) * 100
            ELSE 
                CASE 
                    WHEN NetProfit < 0 THEN -100.0
                    ELSE 0 
                END
        END AS ProfitMargin,
        TransactionCount,
        ROUND((TotalSales / NULLIF(SUM(TotalSales) OVER(), 0)) * 100, 2) AS SalesPercentage,
        ROUND(((InventoryCost + LocalExpenses) / NULLIF(SUM(InventoryCost + LocalExpenses) OVER(), 0)) * 100, 2) AS ExpensePercentage
    FROM CombinedCategories
    WHERE TotalSales > 0 OR InventoryCost > 0 OR LocalExpenses > 0
    ORDER BY 
        CASE 
            WHEN CategoryType = 'Sales' THEN 1
            WHEN CategoryType = 'Sales by Product' THEN 2
            WHEN CategoryType = 'Inventory Cost' THEN 3
            WHEN CategoryType = 'Expense' THEN 4
            ELSE 5
        END,
        NetProfit DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        private DataTable GetDetailedView()
        {
            DataTable dt = new DataTable();

            string query = @"
    SELECT 
        TransactionDate,
        TransactionType,
        Description,
        Amount,
        Category,
        ReferenceID,
        PartyName,
        ProductName,
        AdditionalInfo
    FROM (
        -- Sales Transactions
        SELECT 
            O.OrderDate AS TransactionDate,
            'Sale' AS TransactionType,
            'Sale - ' + ISNULL(O.Customer_Name, 'Customer') AS Description,
            (ISNULL(O.Cash, 0) + ISNULL(O.Credit, 0) + ISNULL(O.Debit, 0)) AS Amount,
            'Sales' AS Category,
            'Order #' + CAST(O.OrderID AS VARCHAR(20)) AS ReferenceID,
            NULL AS PartyName,
            NULL AS ProductName,
            'Payment: ' + 
                CASE 
                    WHEN O.Cash > 0 THEN 'Cash ' + CAST(O.Cash AS VARCHAR(20))
                    WHEN O.Credit > 0 THEN 'Credit ' + CAST(O.Credit AS VARCHAR(20))
                    WHEN O.Debit > 0 THEN 'Debit ' + CAST(O.Debit AS VARCHAR(20))
                    ELSE 'Mixed'
                END AS AdditionalInfo
        FROM Orders O
        WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
        
        UNION ALL
        
        -- Local Expenses
        SELECT 
            LE.Local_Exp_Date AS TransactionDate,
            'Expense' AS TransactionType,
            'Local Expense - ' + ISNULL(LE.ActName, 'General') AS Description,
            LE.Local_Exp_Amount AS Amount,
            'Local Expenses' AS Category,
            'Expense #' + CAST(LE.Local_Exp_ID AS VARCHAR(20)) AS ReferenceID,
            NULL AS PartyName,
            NULL AS ProductName,
            'Account: ' + ISNULL(LE.ActName, '') AS AdditionalInfo
        FROM LocalExpense LE
        WHERE LE.Local_Exp_Date BETWEEN @FromDate AND @ToDate
        
        UNION ALL
        
        -- ProInventory Purchases (Debit)
        SELECT 
            PI.InvDate AS TransactionDate,
            'Inventory Purchase' AS TransactionType,
            'Inventory Purchase - ' + ISNULL(P.PartyName, 'Vendor') + ' - ' + ISNULL(PD.ProductName, 'Product') AS Description,
            PI.Debit AS Amount,
            'Inventory Costs' AS Category,
            'Inv #' + CAST(PI.InventoryID AS VARCHAR(20)) AS ReferenceID,
            P.PartyName,
            PD.ProductName,
            'Bags: ' + CAST(PI.Bags AS VARCHAR(10)) + 
            ', Weight: ' + CAST(PI.Weight AS VARCHAR(20)) + 
            ', Rate: ' + CAST(PI.Rate AS VARCHAR(20)) AS AdditionalInfo
        FROM ProInventory PI
        LEFT JOIN Party P ON PI.PartyID = P.PartyID
        LEFT JOIN Product PD ON PI.ProductID = PD.ProductID
        WHERE PI.InvDate BETWEEN @FromDate AND @ToDate
            AND ISNULL(PI.Debit, 0) > 0
        
        UNION ALL
        
        -- ProInventory Returns/Credits (Credit)
        SELECT 
            PI.InvDate AS TransactionDate,
            'Inventory Credit' AS TransactionType,
            'Inventory Credit - ' + ISNULL(P.PartyName, 'Vendor') + ' - ' + ISNULL(PD.ProductName, 'Product') AS Description,
            -PI.Credit AS Amount,  -- Negative amount as it reduces costs
            'Inventory Costs' AS Category,
            'Inv #' + CAST(PI.InventoryID AS VARCHAR(20)) AS ReferenceID,
            P.PartyName,
            PD.ProductName,
            'Return/Credit - Bags: ' + CAST(PI.Bags AS VARCHAR(10)) + 
            ', Weight: ' + CAST(PI.Weight AS VARCHAR(20)) AS AdditionalInfo
        FROM ProInventory PI
        LEFT JOIN Party P ON PI.PartyID = P.PartyID
        LEFT JOIN Product PD ON PI.ProductID = PD.ProductID
        WHERE PI.InvDate BETWEEN @FromDate AND @ToDate
            AND ISNULL(PI.Credit, 0) > 0
        
        UNION ALL
        
        -- Sale Product Items (Detailed sales)
        SELECT 
            O.OrderDate AS TransactionDate,
            'Sale Item' AS TransactionType,
            'Sale Item - ' + ISNULL(O.Customer_Name, 'Customer') + ' - ' + ISNULL(P.ProductName, 'Product') AS Description,
            SP.Amount AS Amount,
            'Sale Items' AS Category,
            'Order #' + CAST(O.OrderID AS VARCHAR(20)) + ', Item #' + CAST(SP.SP_ID AS VARCHAR(20)) AS ReferenceID,
            PT.PartyName,
            P.ProductName,
            'Bags: ' + CAST(SP.QtyBag AS VARCHAR(10)) + 
            ', Weight: ' + CAST(SP.Weight AS VARCHAR(20)) + 
            ', Rate: ' + CAST(SP.Rate AS VARCHAR(20)) AS AdditionalInfo
        FROM SaleProduct SP
        INNER JOIN Orders O ON SP.OrderID = O.OrderID
        LEFT JOIN Product P ON SP.ProductID = P.ProductID
        LEFT JOIN Party PT ON SP.PartyID = PT.PartyID
        WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
    ) AS AllTransactions
    ORDER BY TransactionDate DESC, 
        CASE TransactionType
            WHEN 'Sale' THEN 1
            WHEN 'Sale Item' THEN 2
            WHEN 'Inventory Purchase' THEN 3
            WHEN 'Inventory Credit' THEN 4
            WHEN 'Expense' THEN 5
            ELSE 6
        END";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }
        */
        
       /* private DataTable GetProfitTrend()
        {
            DataTable dt = new DataTable();

            string query = @"
    WITH DailySales AS (
        SELECT 
            CONVERT(DATE, O.OrderDate) AS TransactionDate,
            SUM(ISNULL(O.Cash, 0) + ISNULL(O.Credit, 0) + ISNULL(O.Debit, 0)) AS TotalSales,
            COUNT(DISTINCT O.OrderID) AS SalesCount,
            SUM(ISNULL(SP.Weight, 0)) AS TotalWeightSold
        FROM Orders O
        INNER JOIN SaleProduct SP ON O.OrderID = SP.OrderID
        WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
        GROUP BY CONVERT(DATE, O.OrderDate)
    ),
    ProductCostRates AS (
        -- Calculate average cost per unit weight for each product
        SELECT 
            PI.ProductID,
            CASE 
                WHEN SUM(ISNULL(PI.Weight, 0)) > 0 
                THEN SUM(ISNULL(PI.Debit, 0) - ISNULL(PI.Credit, 0)) / SUM(ISNULL(PI.Weight, 0))
                ELSE 0 
            END AS AvgCostPerWeight
        FROM ProInventory PI
        WHERE PI.InvDate <= @ToDate
        GROUP BY PI.ProductID
    ),
    DailySoldItems AS (
        -- Get daily sales with product details for cost calculation
        SELECT 
            CONVERT(DATE, O.OrderDate) AS TransactionDate,
            SP.ProductID,
            SUM(SP.Weight) AS WeightSold,
            SUM(SP.Amount) AS AmountSold
        FROM Orders O
        INNER JOIN SaleProduct SP ON O.OrderID = SP.OrderID
        WHERE O.OrderDate BETWEEN @FromDate AND @ToDate
        GROUP BY CONVERT(DATE, O.OrderDate), SP.ProductID
    ),
    DailyInventoryCost AS (
        -- Calculate daily inventory cost
        SELECT 
            DSI.TransactionDate,
            SUM(DSI.WeightSold * ISNULL(PCR.AvgCostPerWeight, 0)) AS InventoryCost,
            COUNT(DISTINCT DSI.ProductID) AS ProductsSoldCount
        FROM DailySoldItems DSI
        LEFT JOIN ProductCostRates PCR ON DSI.ProductID = PCR.ProductID
        GROUP BY DSI.TransactionDate
    ),
    DailyLocalExpenses AS (
        SELECT 
            CONVERT(DATE, Local_Exp_Date) AS TransactionDate,
            SUM(ISNULL(Local_Exp_Amount, 0)) AS LocalExpenses,
            COUNT(Local_Exp_ID) AS ExpenseCount
        FROM LocalExpense
        WHERE Local_Exp_Date BETWEEN @FromDate AND @ToDate
        GROUP BY CONVERT(DATE, Local_Exp_Date)
    ),
    DailyData AS (
        SELECT 
            COALESCE(DS.TransactionDate, DIC.TransactionDate, DLE.TransactionDate) AS TransactionDate,
            ISNULL(DS.TotalSales, 0) AS TotalSales,
            ISNULL(DIC.InventoryCost, 0) AS InventoryCost,
            ISNULL(DLE.LocalExpenses, 0) AS LocalExpenses,
            ISNULL(DS.TotalSales, 0) - (ISNULL(DIC.InventoryCost, 0) + ISNULL(DLE.LocalExpenses, 0)) AS NetProfit,
            CASE 
                WHEN ISNULL(DS.TotalSales, 0) > 0 
                THEN ((ISNULL(DS.TotalSales, 0) - (ISNULL(DIC.InventoryCost, 0) + ISNULL(DLE.LocalExpenses, 0))) / 
                      ISNULL(DS.TotalSales, 0)) * 100
                ELSE 0 
            END AS ProfitMargin,
            ISNULL(DS.SalesCount, 0) AS SalesCount,
            ISNULL(DLE.ExpenseCount, 0) AS ExpenseCount,
            ISNULL(DS.TotalWeightSold, 0) AS TotalWeightSold,
            ISNULL(DIC.ProductsSoldCount, 0) AS ProductsSoldCount
        FROM DailySales DS
        FULL OUTER JOIN DailyInventoryCost DIC ON DS.TransactionDate = DIC.TransactionDate
        FULL OUTER JOIN DailyLocalExpenses DLE ON DS.TransactionDate = DLE.TransactionDate
    ),
    WeeklyData AS (
        SELECT 
            DATEPART(WEEK, TransactionDate) AS WeekNumber,
            YEAR(TransactionDate) AS Year,
            MIN(TransactionDate) AS WeekStart,
            MAX(TransactionDate) AS WeekEnd,
            SUM(TotalSales) AS WeeklySales,
            SUM(InventoryCost) AS WeeklyInventoryCost,
            SUM(LocalExpenses) AS WeeklyLocalExpenses,
            SUM(InventoryCost + LocalExpenses) AS WeeklyTotalExpenses,
            SUM(NetProfit) AS WeeklyProfit,
            AVG(ProfitMargin) AS AvgProfitMargin,
            SUM(SalesCount) AS WeeklySalesCount,
            SUM(ExpenseCount) AS WeeklyExpenseCount,
            SUM(TotalWeightSold) AS WeeklyWeightSold,
            AVG(ProductsSoldCount) AS AvgProductsSoldPerDay
        FROM DailyData
        GROUP BY DATEPART(WEEK, TransactionDate), YEAR(TransactionDate)
    )
    SELECT 
        WeekNumber,
        Year,
        WeekStart,
        WeekEnd,
        CONCAT('Week ', WeekNumber, ' (', FORMAT(WeekStart, 'dd MMM'), ' - ', FORMAT(WeekEnd, 'dd MMM'), ')') AS WeekRange,
        WeeklySales,
        WeeklyInventoryCost,
        WeeklyLocalExpenses,
        WeeklyTotalExpenses,
        WeeklyProfit,
        AvgProfitMargin,
        WeeklySalesCount,
        WeeklyExpenseCount,
        WeeklyWeightSold,
        AvgProductsSoldPerDay,
        CASE 
            WHEN WeeklySales > 0 
            THEN (WeeklyInventoryCost / WeeklySales) * 100 
            ELSE 0 
        END AS CostOfGoodsSoldPercentage,
        CASE 
            WHEN WeeklySales > 0 
            THEN (WeeklyLocalExpenses / WeeklySales) * 100 
            ELSE 0 
        END AS ExpenseRatio,
        CASE 
            WHEN WeeklyInventoryCost > 0 
            THEN (WeeklyProfit / WeeklyInventoryCost) * 100 
            ELSE 0 
        END AS ReturnOnInventory
    FROM WeeklyData
    ORDER BY Year DESC, WeekNumber DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", dateFrom.Value.Date);
                cmd.Parameters.AddWithValue("@ToDate", dateTo.Value.Date);

                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }
       */

        private void FormatDataGridView()
        {
            if (dataGridViewReport.Columns.Count == 0) return;

            // Add row number column
            if (!dataGridViewReport.Columns.Contains("RowNumber"))
            {
                DataGridViewTextBoxColumn rowNumberColumn = new DataGridViewTextBoxColumn();
                rowNumberColumn.Name = "RowNumber";
                rowNumberColumn.HeaderText = "Sr. No.";
                rowNumberColumn.Width = 60;
                rowNumberColumn.ReadOnly = true;
                rowNumberColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rowNumberColumn.DefaultCellStyle.BackColor = Color.LightGray;
                rowNumberColumn.DefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);

                dataGridViewReport.Columns.Insert(0, rowNumberColumn);
            }

            dataGridViewReport.RowHeadersVisible = false;
            dataGridViewReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewReport.ReadOnly = true;
            dataGridViewReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Format headers
            dataGridViewReport.EnableHeadersVisualStyles = false;
            dataGridViewReport.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            dataGridViewReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewReport.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            dataGridViewReport.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Format columns based on content
            foreach (DataGridViewColumn column in dataGridViewReport.Columns)
            {
                if (column.Name == "RowNumber") continue;

                // Color code based on column content
                if (column.Name.Contains("Sales") || column.Name.Contains("Profit") && !column.Name.Contains("Margin"))
                {
                    column.DefaultCellStyle.ForeColor = Color.DarkGreen;
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.Name.Contains("Expense"))
                {
                    column.DefaultCellStyle.ForeColor = Color.DarkRed;
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.Name.Contains("Margin") || column.Name.Contains("Percentage"))
                {
                    column.DefaultCellStyle.ForeColor = Color.Purple;
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.Format = "N2";
                }
                else if (column.Name.Contains("Date"))
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                }

                // Format numeric columns
                if (column.ValueType == typeof(decimal) || column.Name.Contains("Amount") ||
                    column.Name.Contains("Sales") || column.Name.Contains("Expenses") ||
                    column.Name.Contains("Profit") && !column.Name.Contains("Margin"))
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            // Populate row numbers
            for (int i = 0; i < dataGridViewReport.Rows.Count; i++)
            {
                if (!dataGridViewReport.Rows[i].IsNewRow)
                {
                    dataGridViewReport.Rows[i].Cells["RowNumber"].Value = (i + 1).ToString();
                }
            }

            dataGridViewReport.AlternatingRowsDefaultCellStyle.BackColor = Color.LightCyan;
        }

        private void CalculateFinancialSummary()
        {
            try
            {
                decimal totalSales = 0;
                decimal totalInventoryCost = 0;
                decimal totalLocalExpenses = 0;
                int salesCount = 0;
                int expenseCount = 0;
                int daysCount = Math.Max(1, (dateTo.Value.Date - dateFrom.Value.Date).Days + 1);

                // Determine which columns exist in the current report
                bool hasInventoryCostColumn = dataGridViewReport.Columns.Contains("InventoryCost");
                bool hasLocalExpensesColumn = dataGridViewReport.Columns.Contains("LocalExpenses");
                bool hasTotalExpensesColumn = dataGridViewReport.Columns.Contains("TotalExpenses");
                bool hasSeparateExpenseColumns = hasInventoryCostColumn || hasLocalExpensesColumn;

                foreach (DataGridViewRow row in dataGridViewReport.Rows)
                {
                    if (row.IsNewRow) continue;

                    string reportType = comboReportType.SelectedItem.ToString();

                    switch (reportType)
                    {
                        case "Daily Summary - Sales vs Expenses":
                            totalSales += GetCellDecimalValue(row, "TotalSales");

                            if (hasSeparateExpenseColumns)
                            {
                                totalInventoryCost += GetCellDecimalValue(row, "InventoryCost");
                                totalLocalExpenses += GetCellDecimalValue(row, "LocalExpenses");
                            }
                            else if (hasTotalExpensesColumn)
                            {
                                totalLocalExpenses += GetCellDecimalValue(row, "TotalExpenses");
                            }

                            salesCount += GetCellIntValue(row, "SalesCount");
                            expenseCount += GetCellIntValue(row, "ExpenseCount");
                            break;

                        case "Monthly Profit & Loss":
                            totalSales += GetCellDecimalValue(row, "TotalSales");

                            if (hasSeparateExpenseColumns)
                            {
                                totalInventoryCost += GetCellDecimalValue(row, "InventoryCost");
                                totalLocalExpenses += GetCellDecimalValue(row, "LocalExpenses");
                            }
                            else if (hasTotalExpensesColumn)
                            {
                                totalLocalExpenses += GetCellDecimalValue(row, "TotalExpenses");
                            }

                            salesCount += GetCellIntValue(row, "SalesCount");
                            expenseCount += GetCellIntValue(row, "ExpenseCount");
                            break;

                        /*case "Category Wise Analysis":
                            totalSales += GetCellDecimalValue(row, "TotalSales");

                            if (hasSeparateExpenseColumns)
                            {
                                totalInventoryCost += GetCellDecimalValue(row, "InventoryCost");
                                totalLocalExpenses += GetCellDecimalValue(row, "LocalExpenses");
                            }
                            else if (hasTotalExpensesColumn)
                            {
                                // Try to categorize based on category name
                                string category = GetCellStringValue(row, "Category");
                                decimal expenseAmount = GetCellDecimalValue(row, "TotalExpenses");

                                if (category.Contains("Inventory") || category.Contains("Cost"))
                                    totalInventoryCost += expenseAmount;
                                else
                                    totalLocalExpenses += expenseAmount;
                            }
                            break;

                        case "Detailed Transaction View":
                            string type = GetCellStringValue(row, "TransactionType");
                            string categorY = GetCellStringValue(row, "Category");
                            decimal amount = GetCellDecimalValue(row, "Amount");

                            if (type.Contains("Sale") || categorY.Contains("Sale"))
                            {
                                totalSales += amount;
                                salesCount++;
                            }
                            else if (type.Contains("Expense") || categorY.Contains("Expense") || categorY.Contains("Cost"))
                            {
                                if (categorY.Contains("Inventory") || type.Contains("Inventory"))
                                    totalInventoryCost += Math.Abs(amount);
                                else
                                    totalLocalExpenses += Math.Abs(amount);
                                expenseCount++;
                            }
                            break;

                        case "Profit Trend Analysis":
                            totalSales += GetCellDecimalValue(row, "WeeklySales");
                            totalInventoryCost += GetCellDecimalValue(row, "WeeklyInventoryCost");
                            totalLocalExpenses += GetCellDecimalValue(row, "WeeklyLocalExpenses");
                            salesCount += GetCellIntValue(row, "WeeklySalesCount");
                            expenseCount += GetCellIntValue(row, "WeeklyExpenseCount");
                            break;
                        */
                    }
                }

                // Calculate total expenses and key metrics
                decimal totalExpenses = totalInventoryCost + totalLocalExpenses;
                decimal netProfit = totalSales - totalExpenses;
                decimal profitMargin = totalSales > 0 ? (netProfit / totalSales) * 100 : 0;
                decimal expenseRatio = totalSales > 0 ? (totalExpenses / totalSales) * 100 : 0;
                decimal inventoryCostRatio = totalSales > 0 ? (totalInventoryCost / totalSales) * 100 : 0;
                decimal localExpenseRatio = totalSales > 0 ? (totalLocalExpenses / totalSales) * 100 : 0;

                // Update main summary labels
                lblTotalSales.Text = $"Total Sales: RS{totalSales:N2}";
                lblTotalExpenses.Text = $"Total Expenses: RS{totalExpenses:N2}";
                lblNetProfit.Text = $"Net Profit: RS{netProfit:N2}";
                lblProfitMargin.Text = $"Profit Margin: {profitMargin:N2}%";

                // Update secondary summary labels
                lblSalesCount.Text = $"Sales Transactions: {salesCount}";
                lblExpenseCount.Text = $"Expense Transactions: {expenseCount}";
                lblAvgSale.Text = $"Avg Sale: RS{(salesCount > 0 ? totalSales / salesCount : 0):N2}";
                lblAvgExpense.Text = $"Avg Expense: RS{(expenseCount > 0 ? totalExpenses / expenseCount : 0):N2}";

                // Update existing panel labels (keeping backward compatibility)
                UpdateExistingPanelLabels(totalSales, totalExpenses, daysCount, expenseRatio);

                // Add new expense breakdown labels if we have separate expense data
                if (hasSeparateExpenseColumns || totalInventoryCost > 0)
                {
                    UpdateExpenseBreakdownLabels(totalInventoryCost, totalLocalExpenses, inventoryCostRatio, localExpenseRatio);
                }

                // Color code based on performance
                UpdateColorCoding(netProfit, profitMargin, inventoryCostRatio, localExpenseRatio);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating financial summary: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateExistingPanelLabels(decimal totalSales, decimal totalExpenses, int daysCount, decimal expenseRatio)
        {
            // Update existing labels (keeping backward compatibility)
            var lblDailySales = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Daily Sales Avg"));
            var lblDailyExpenses = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Daily Expense Avg"));
            var lblExpenseRatio = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Expense/Sales Ratio"));
            var lblBreakEven = panelSummary.Controls.OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Break-even Point"));

            if (lblDailySales != null)
                lblDailySales.Text = $"Daily Sales Avg: RS{(daysCount > 0 ? totalSales / daysCount : 0):N2}";

            if (lblDailyExpenses != null)
                lblDailyExpenses.Text = $"Daily Expense Avg: RS{(daysCount > 0 ? totalExpenses / daysCount : 0):N2}";

            if (lblExpenseRatio != null)
                lblExpenseRatio.Text = $"Expense/Sales Ratio: {expenseRatio:N2}%";

            if (lblBreakEven != null)
                lblBreakEven.Text = $"Break-even Point: RS{totalExpenses:N2}";
        }

        private void UpdateExpenseBreakdownLabels(decimal inventoryCost, decimal localExpenses,
                                                 decimal inventoryCostRatio, decimal localExpenseRatio)
        {
            // Find or create expense breakdown labels
            Label lblInventoryCost = FindOrCreateLabel("Inventory Cost", 20, 105);
            Label lblLocalExpensesBreakdown = FindOrCreateLabel("Local Expenses", 250, 105);

            lblInventoryCost.Text = $"Inventory Cost: RS{inventoryCost:N2} ({inventoryCostRatio:N1}%)";
            lblLocalExpensesBreakdown.Text = $"Local Expenses: RS{localExpenses:N2} ({localExpenseRatio:N1}%)";

            // Add profitability indicator
            decimal profitMargin = 0;
            if (lblProfitMargin.Text.Contains("%"))
            {
                string marginText = lblProfitMargin.Text.Replace("Profit Margin:", "").Replace("%", "").Trim();
                decimal.TryParse(marginText, out profitMargin);
            }

            Label lblProfitability = FindOrCreateLabel("Profitability Status", 550, 105);
            string status = GetProfitabilityStatus(profitMargin);
            lblProfitability.Text = $"Status: {status}";
            lblProfitability.ForeColor = GetStatusColor(profitMargin);
        }

        private Label FindOrCreateLabel(string startsWith, int x, int y)
        {
            Label label = panelSummary.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text.StartsWith(startsWith));

            if (label == null)
            {
                label = new Label
                {
                    Text = startsWith,
                    Location = new Point(x, y),
                    Size = new Size(200, 20),
                    Font = new Font("Arial", 8, FontStyle.Bold),
                    Tag = startsWith.Replace(" ", "")
                };
                panelSummary.Controls.Add(label);
            }

            label.Location = new Point(x, y);
            return label;
        }

        private string GetProfitabilityStatus(decimal profitMargin)
        {
            if (profitMargin > 20) return "Highly Profitable";
            if (profitMargin > 10) return "Profitable";
            if (profitMargin > 5) return "Moderately Profitable";
            if (profitMargin > 0) return "Marginally Profitable";
            if (profitMargin == 0) return "Break-even";
            return "Loss Making";
        }

        private Color GetStatusColor(decimal profitMargin)
        {
            if (profitMargin > 10) return Color.DarkGreen;
            if (profitMargin > 0) return Color.Orange;
            return Color.DarkRed;
        }

        private void UpdateColorCoding(decimal netProfit, decimal profitMargin,
                                      decimal inventoryCostRatio, decimal localExpenseRatio)
        {
            // Color code net profit
            lblNetProfit.ForeColor = netProfit >= 0 ? Color.DarkGreen : Color.DarkRed;
            lblProfitMargin.ForeColor = profitMargin >= 0 ? Color.DarkGreen : Color.DarkRed;

            // Color code expense ratios in panel if they exist
            foreach (Control control in panelSummary.Controls)
            {
                if (control is Label label)
                {
                    if (label.Text.Contains("Inventory Cost") && label.Text.Contains("%"))
                    {
                        label.ForeColor = inventoryCostRatio < 60 ? Color.DarkGreen :
                                         inventoryCostRatio < 75 ? Color.Orange : Color.DarkRed;
                    }
                    else if (label.Text.Contains("Local Expenses") && label.Text.Contains("%"))
                    {
                        label.ForeColor = localExpenseRatio < 15 ? Color.DarkGreen :
                                         localExpenseRatio < 25 ? Color.Orange : Color.DarkRed;
                    }
                    else if (label.Text.Contains("Expense/Sales Ratio"))
                    {
                        decimal totalExpenseRatio = inventoryCostRatio + localExpenseRatio;
                        label.ForeColor = totalExpenseRatio < 75 ? Color.DarkGreen :
                                         totalExpenseRatio < 85 ? Color.Orange : Color.DarkRed;
                    }
                }
            }
        }

        // Enhanced GetCellDecimalValue to handle different column structures
        private decimal GetCellDecimalValue(DataGridViewRow row, string columnName)
        {
            try
            {
                // Try the exact column name first
                if (row.Cells[columnName]?.Value != null)
                {
                    string value = row.Cells[columnName].Value.ToString();
                    if (decimal.TryParse(value, out decimal result))
                        return result;
                }

                // Try common alternatives
                Dictionary<string, List<string>> columnAliases = new Dictionary<string, List<string>>
        {
            { "TotalSales", new List<string> { "Sales", "Amount", "SalesAmount", "Total Sales" } },
            { "InventoryCost", new List<string> { "Cost", "CostAmount", "Inventory Cost", "WeeklyInventoryCost" } },
            { "LocalExpenses", new List<string> { "LocalExpense", "ExpenseAmount", "Local Expenses", "WeeklyLocalExpenses" } },
            { "TotalExpenses", new List<string> { "Expenses", "WeeklyExpenses", "Total Expenses" } },
            { "SalesCount", new List<string> { "SalesCount", "TransactionCount", "WeeklySalesCount" } },
            { "ExpenseCount", new List<string> { "ExpenseCount", "WeeklyExpenseCount" } }
        };

                if (columnAliases.ContainsKey(columnName))
                {
                    foreach (string alias in columnAliases[columnName])
                    {
                        if (row.Cells[alias]?.Value != null)
                        {
                            string value = row.Cells[alias].Value.ToString();
                            if (decimal.TryParse(value, out decimal result))
                                return result;
                        }
                    }
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private int GetCellIntValue(DataGridViewRow row, string columnName)
        {
            return (int)Math.Round(GetCellDecimalValue(row, columnName));
        }

        private string GetCellStringValue(DataGridViewRow row, string columnName)
        {
            try
            {
                if (row.Cells[columnName]?.Value != null)
                    return row.Cells[columnName].Value.ToString();
                return "";
            }
            catch
            {
                return "";
            }


        }

        private void DataGridViewReport_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Optional: Additional row formatting
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintReport();
        }

        private void ExportToExcel()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                saveFileDialog.FileName = $"SaleExpense_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        // Write headers (skip RowNumber column)
                        var headers = dataGridViewReport.Columns.Cast<DataGridViewColumn>()
                                            .Where(col => col.Name != "RowNumber")
                                            .Select(column => column.HeaderText);
                        sw.WriteLine(string.Join(",", headers));

                        // Write data (skip RowNumber column)
                        foreach (DataGridViewRow row in dataGridViewReport.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                var cells = row.Cells.Cast<DataGridViewCell>()
                                            .Where(cell => cell.OwningColumn.Name != "RowNumber")
                                            .Select(cell => $"\"{cell.Value}\"");
                                sw.WriteLine(string.Join(",", cells));
                            }
                        }
                    }

                    MessageBox.Show($"Report exported successfully to {saveFileDialog.FileName}", "Success",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintReport()
        {
            MessageBox.Show("Print functionality would be implemented here with proper formatting",
                          "Print Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}