using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace POS_Rice
{
    internal class PDFStockReportGenerator
    {
        public void GenerateStockPDFFromGrid(DataGridView dataGridView, string searchText = "")
        {
            try
            {
                if (dataGridView == null || dataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("No data available in the grid to export!");
                    return;
                }

                // Get stock data from DataGridView
                StockReportData stockData = GetStockDataFromGridView(dataGridView);

                // Generate PDF
                GenerateStockPDF(stockData, searchText);

                MessageBox.Show("Stock PDF report generated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating stock PDF: {ex.Message}");
            }
        }

        private StockReportData GetStockDataFromGridView(DataGridView dataGridView)
        {
            StockReportData stockData = new StockReportData
            {
                ReportDate = DateTime.Now
            };

            // Get data from each row in DataGridView
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.IsNewRow)
                {
                    StockItem item = new StockItem
                    {
                        PartyName = row.Cells["PartyName"].Value?.ToString() ?? "N/A",
                        ProductName = row.Cells["ProductName"].Value?.ToString() ?? "N/A",
                        Brand = row.Cells["Brand"].Value?.ToString() ?? "N/A",
                        TotalBags = ParseInt(row.Cells["Total Bags"].Value),
                        SoldBags = ParseInt(row.Cells["Sold Bags"].Value),
                        AvailableStock = ParseInt(row.Cells["Available Stock"].Value),
                        PurchasedAmount = ParseDecimal(row.Cells["PURCHASED AMOUNT"].Value),
                        SoldAmount = ParseDecimal(row.Cells["SOLD AMOUNT"].Value),
                        Balance = ParseDecimal(row.Cells["BALANCE"].Value)
                    };

                    stockData.StockItems.Add(item);

                    // Calculate totals
                    stockData.TotalBags += item.TotalBags;
                    stockData.TotalSoldBags += item.SoldBags;
                    stockData.TotalAvailableStock += item.AvailableStock;
                    stockData.TotalPurchasedAmount += item.PurchasedAmount;
                    stockData.TotalSoldAmount += item.SoldAmount;
                    stockData.TotalBalance += item.Balance;
                }
            }

            return stockData;
        }

        private int ParseInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            if (int.TryParse(value.ToString(), out int result)) return result;
            return 0;
        }

        private decimal ParseDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            if (decimal.TryParse(value.ToString(), out decimal result)) return result;
            return 0;
        }

        private void GenerateStockPDF(StockReportData stockData, string searchText)
        {
            // Create new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Stock_Report_{DateTime.Now:yyyyMMddHHmmss}";

            // Create empty page with LANDSCAPE orientation
            PdfPage page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Define fonts
            XFont fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
            XFont fontSubtitle = new XFont("Arial", 12, XFontStyle.Regular);
            XFont fontHeader = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 9);
            XFont fontBold = new XFont("Arial", 9, XFontStyle.Bold);
            XFont fontSmallBold = new XFont("Arial", 9, XFontStyle.Bold);

            // Current Y position for drawing
            double yPos = 30;
            double xPos = 20;
            double pageWidth = page.Width.Point;

            // Draw title
            gfx.DrawString("STOCK INVENTORY REPORT - AA TRADERS", fontTitle, XBrushes.Navy,
                          new XRect(0, yPos, pageWidth, page.Height), XStringFormats.TopCenter);
            yPos += 30;

            // Draw report info
            gfx.DrawString($"Report Date: {DateTime.Now:dd/MM/yyyy}", fontSubtitle, XBrushes.Black, xPos, yPos);

            // Draw search text if provided
            if (!string.IsNullOrEmpty(searchText))
            {
                gfx.DrawString($"Search: {searchText}", fontSubtitle, XBrushes.DarkBlue, xPos + 200, yPos);
            }

            gfx.DrawString($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm}", fontNormal, XBrushes.Gray, pageWidth - 200, yPos);
            yPos += 25;

            // Draw table headers
            DrawStockTableHeaders(gfx, xPos, ref yPos, pageWidth, fontHeader);

            // Draw items in grid format
            int srNo = 1;
            bool alternateRow = false;

            foreach (var item in stockData.StockItems)
            {
                // Alternate row background
                if (alternateRow)
                {
                    gfx.DrawRectangle(XBrushes.WhiteSmoke, xPos, yPos - 2, pageWidth - 40, 15);
                }
                alternateRow = !alternateRow;

                // Draw item details
                DrawStockItemRow(gfx, xPos, yPos, srNo, item, fontNormal);

                yPos += 15;
                srNo++;

                // Check if we need a new page
                if (yPos > page.Height - 120)
                {
                    // Draw page footer
                    DrawPageFooter(gfx, xPos, page.Height - 30, pageWidth, fontNormal);

                    // Add new page
                    page = document.AddPage();
                    page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = 30;

                    // Redraw headers on new page
                    DrawStockTableHeaders(gfx, xPos, ref yPos, pageWidth, fontHeader);
                    alternateRow = false;
                }
            }

            // Draw summary section
            DrawStockSummary(gfx, xPos, ref yPos, pageWidth, stockData, fontBold, fontHeader);

            // Draw final page footer
            DrawPageFooter(gfx, xPos, page.Height - 30, pageWidth, fontNormal);

            // Save PDF
            string fileName = $"Stock_Inventory_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fullPath = Path.Combine(desktopPath, fileName);

            document.Save(fullPath);
            document.Close();

            // Open PDF
            System.Threading.Thread.Sleep(500);
            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
        }

        private void DrawStockTableHeaders(XGraphics gfx, double xPos, ref double yPos,
                                         double pageWidth, XFont fontHeader)
        {
            // Draw header background
            gfx.DrawRectangle(XBrushes.LightGray, xPos, yPos - 2, pageWidth - 40, 20);

            // Draw column headers
            double colX = xPos + 5;
            gfx.DrawString("Sr.", fontHeader, XBrushes.Black, colX, yPos);
            colX += 30;
            gfx.DrawString("Party Name", fontHeader, XBrushes.Black, colX, yPos);
            colX += 120;
            gfx.DrawString("Product Name", fontHeader, XBrushes.Black, colX, yPos);
            colX += 120;
            gfx.DrawString("Brand", fontHeader, XBrushes.Black, colX, yPos);
            colX += 80;
            gfx.DrawString("Total", fontHeader, XBrushes.Black, colX, yPos);
            colX += 50;
            gfx.DrawString("Sold", fontHeader, XBrushes.Black, colX, yPos);
            colX += 50;
            gfx.DrawString("Available", fontHeader, XBrushes.Black, colX, yPos);
            colX += 60;
            gfx.DrawString("Purchased", fontHeader, XBrushes.Black, colX, yPos);
            colX += 70;
            gfx.DrawString("Sold Amt", fontHeader, XBrushes.Black, colX, yPos);
            colX += 70;
            gfx.DrawString("Balance", fontHeader, XBrushes.Black, colX, yPos);

            yPos += 20;
        }

        private void DrawStockItemRow(XGraphics gfx, double xPos, double yPos, int srNo,
                                     StockItem item, XFont fontNormal)
        {
            double colX = xPos + 5;

            // Serial Number
            gfx.DrawString(srNo.ToString(), fontNormal, XBrushes.Black, colX, yPos);
            colX += 30;

            // Party Name
            string partyName = item.PartyName.Length > 20 ? item.PartyName.Substring(0, 20) + "..." : item.PartyName;
            gfx.DrawString(partyName, fontNormal, XBrushes.Black, colX, yPos);
            colX += 120;

            // Product Name
            string productName = item.ProductName.Length > 20 ? item.ProductName.Substring(0, 20) + "..." : item.ProductName;
            gfx.DrawString(productName, fontNormal, XBrushes.Black, colX, yPos);
            colX += 120;

            // Brand
            string brand = item.Brand.Length > 15 ? item.Brand.Substring(0, 15) + "..." : item.Brand;
            gfx.DrawString(brand, fontNormal, XBrushes.Black, colX, yPos);
            colX += 80;

            // Total Bags
            gfx.DrawString(item.TotalBags.ToString("N0"), fontNormal, XBrushes.Black, colX, yPos);
            colX += 50;

            // Sold Bags
            gfx.DrawString(item.SoldBags.ToString("N0"), fontNormal, XBrushes.Black, colX, yPos);
            colX += 50;

            // Available Stock (with color coding)
            XBrush stockColor = XBrushes.Black;
            if (item.AvailableStock <= 0)
                stockColor = XBrushes.DarkRed;
            else if (item.AvailableStock < 10)
                stockColor = XBrushes.DarkOrange;
            else
                stockColor = XBrushes.DarkGreen;

            gfx.DrawString(item.AvailableStock.ToString("N0"), fontNormal, stockColor, colX, yPos);
            colX += 60;

            // Purchased Amount
            gfx.DrawString(item.PurchasedAmount.ToString("N2"), fontNormal, XBrushes.Black, colX, yPos);
            colX += 70;

            // Sold Amount
            gfx.DrawString(item.SoldAmount.ToString("N2"), fontNormal, XBrushes.Black, colX, yPos);
            colX += 70;

            // Balance (Profit/Loss)
            XBrush balanceColor = item.Balance >= 0 ? XBrushes.DarkGreen : XBrushes.DarkRed;
            gfx.DrawString(item.Balance.ToString("N2"), fontNormal, balanceColor, colX, yPos);
        }

        private void DrawStockSummary(XGraphics gfx, double xPos, ref double yPos,
                                     double pageWidth, StockReportData stockData,
                                     XFont fontBold, XFont fontHeader)
        {
            yPos += 15;
            gfx.DrawLine(XPens.Gray, xPos, yPos, pageWidth - 20, yPos);
            yPos += 15;

            // Summary header
            gfx.DrawString("STOCK SUMMARY", fontHeader, XBrushes.Navy, xPos, yPos);
            yPos += 20;

            // First row of summary
            gfx.DrawString($"Total Items: {stockData.StockItems.Count}", fontBold, XBrushes.Black, xPos, yPos);
            gfx.DrawString($"Total Bags: {stockData.TotalBags:N0}", fontBold, XBrushes.Black, xPos + 200, yPos);
            gfx.DrawString($"Sold Bags: {stockData.TotalSoldBags:N0}", fontBold, XBrushes.Black, xPos + 350, yPos);
            yPos += 15;

            // Second row of summary
            gfx.DrawString($"Available Stock: {stockData.TotalAvailableStock:N0}", fontBold, XBrushes.Black, xPos, yPos);

            // Color code for total available stock
            XBrush totalStockColor = XBrushes.Black;
            if (stockData.TotalAvailableStock <= 0)
                totalStockColor = XBrushes.DarkRed;
            else if (stockData.TotalAvailableStock < 50)
                totalStockColor = XBrushes.DarkOrange;
            else
                totalStockColor = XBrushes.DarkGreen;

            gfx.DrawString($"{stockData.TotalAvailableStock:N0}", fontBold, totalStockColor, xPos + 120, yPos);

            gfx.DrawString($"Stock Value: Rs {stockData.TotalPurchasedAmount:N2}", fontBold, XBrushes.DarkBlue, xPos + 350, yPos);
            yPos += 15;

            // Third row of summary
            gfx.DrawString($"Total Purchased: Rs {stockData.TotalPurchasedAmount:N2}", fontBold, XBrushes.DarkBlue, xPos, yPos);
            gfx.DrawString($"Total Sales: Rs {stockData.TotalSoldAmount:N2}", fontBold, XBrushes.DarkBlue, xPos + 200, yPos);
            yPos += 15;

            // Net Balance (Profit/Loss)
            XBrush netBalanceColor = stockData.TotalBalance >= 0 ? XBrushes.DarkGreen : XBrushes.DarkRed;
            string balanceStatus = stockData.TotalBalance >= 0 ? "PROFIT" : "LOSS";

            gfx.DrawString($"NET {balanceStatus}:", fontHeader, netBalanceColor, xPos, yPos);
            gfx.DrawString($"Rs {Math.Abs(stockData.TotalBalance):N2}", fontHeader, netBalanceColor, xPos + 100, yPos);

            gfx.DrawString($"Overall Balance: Rs {stockData.TotalBalance:N2}",
                          fontHeader, netBalanceColor, pageWidth - 200, yPos);
        }

        private void DrawPageFooter(XGraphics gfx, double xPos, double yPos,
                                   double pageWidth, XFont fontNormal)
        {
            gfx.DrawLine(XPens.LightGray, xPos, yPos, pageWidth - 20, yPos);
            yPos += 5;

            gfx.DrawString("AA Traders - Stock Inventory System", fontNormal, XBrushes.Gray, xPos, yPos);
            gfx.DrawString($"Page 1", fontNormal, XBrushes.Gray, pageWidth - 50, yPos);
        }
    }

    // Helper classes for structured data
    public class StockReportData
    {
        public DateTime ReportDate { get; set; }
        public List<StockItem> StockItems { get; set; } = new List<StockItem>();

        // Summary totals
        public int TotalBags { get; set; }
        public int TotalSoldBags { get; set; }
        public int TotalAvailableStock { get; set; }
        public decimal TotalPurchasedAmount { get; set; }
        public decimal TotalSoldAmount { get; set; }
        public decimal TotalBalance { get; set; }
    }

    public class StockItem
    {
        public string PartyName { get; set; }
        public string ProductName { get; set; }
        public string Brand { get; set; }
        public int TotalBags { get; set; }
        public int SoldBags { get; set; }
        public int AvailableStock { get; set; }
        public decimal PurchasedAmount { get; set; }
        public decimal SoldAmount { get; set; }
        public decimal Balance { get; set; }
    }
}