using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace POS_Rice
{
    internal class PDFInvoiceGenerator
    {
        private string connectionString = @"Server=.\SQLEXPRESS;Database=Rice;Integrated Security=true;";

        public void GeneratePDFInvoiceFromOrder(TextBox textOrderID, TextBox textCustName, DateTimePicker dateTimeDate,
                                              TextBox textCash, TextBox textCredit, TextBox textDebit, TextBox textBalance)
        {
            try
            {
                // Validate Order ID
                if (string.IsNullOrEmpty(textOrderID.Text))
                {
                    MessageBox.Show("Please select an order first!");
                    return;
                }

                int orderID = Convert.ToInt32(textOrderID.Text);

                // Get order data with all items
                OrderInvoiceData orderData = GetOrderInvoiceData(orderID);

                if (orderData == null || orderData.Items.Count == 0)
                {
                    MessageBox.Show("No order data found for the given Order ID!");
                    return;
                }

                // Generate PDF with complete order information
                GeneratePDF(orderData);

                MessageBox.Show("PDF invoice generated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}");
            }
        }

        private OrderInvoiceData GetOrderInvoiceData(int orderID)
        {
            OrderInvoiceData orderData = new OrderInvoiceData();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // First get the main order details
                string orderQuery = @"
                SELECT 
                    OrderID,
                    Customer_Name,
                    OrderDate,
                    Cash,
                    Credit,
                    Debit,
                    Balance
                FROM Orders 
                WHERE OrderID = @OrderID";

                using (SqlCommand orderCmd = new SqlCommand(orderQuery, conn))
                {
                    orderCmd.Parameters.AddWithValue("@OrderID", orderID);
                    conn.Open();

                    using (SqlDataReader orderReader = orderCmd.ExecuteReader())
                    {
                        if (orderReader.Read())
                        {
                            orderData.OrderID = orderID;
                            orderData.InvoiceNo = orderID; // OrderID as InvoiceNo
                            orderData.CustomerName = orderReader["Customer_Name"] != DBNull.Value ?
                                                   orderReader["Customer_Name"].ToString() : "N/A";
                            orderData.OrderDate = orderReader["OrderDate"] != DBNull.Value ?
                                                 Convert.ToDateTime(orderReader["OrderDate"]) : DateTime.Now;
                            orderData.Cash = orderReader["Cash"] != DBNull.Value ?
                                           Convert.ToDecimal(orderReader["Cash"]) : 0;
                            orderData.Credit = orderReader["Credit"] != DBNull.Value ?
                                             Convert.ToDecimal(orderReader["Credit"]) : 0;
                            orderData.Debit = orderReader["Debit"] != DBNull.Value ?
                                            Convert.ToDecimal(orderReader["Debit"]) : 0;
                            orderData.Balance = orderReader["Balance"] != DBNull.Value ?
                                              Convert.ToDecimal(orderReader["Balance"]) : 0;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                // Keep the Party join in query for internal use, but don't display it in PDF
                string itemsQuery = @"
                SELECT 
                    sp.SP_ID,
                    sp.QtyBag,
                    sp.TotalQtyBags,
                    sp.RemBags,
                    sp.Weight,
                    sp.Rate,
                    sp.Amount,
                    p.ProductName,
                    p.Brand,
                    pt.PartyName
                FROM SaleProduct sp
                INNER JOIN Product p ON sp.ProductID = p.ProductID
                INNER JOIN Party pt ON sp.PartyID = pt.PartyID
                WHERE sp.OrderID = @OrderID";

                using (SqlCommand itemsCmd = new SqlCommand(itemsQuery, conn))
                {
                    itemsCmd.Parameters.AddWithValue("@OrderID", orderID);

                    using (SqlDataReader itemsReader = itemsCmd.ExecuteReader())
                    {
                        while (itemsReader.Read())
                        {
                            InvoiceItem item = new InvoiceItem
                            {
                                SP_ID = itemsReader["SP_ID"] != DBNull.Value ?
                                       Convert.ToInt32(itemsReader["SP_ID"]) : 0,
                                ProductName = itemsReader["ProductName"] != DBNull.Value ?
                                             itemsReader["ProductName"].ToString() : "N/A",
                                Brand = itemsReader["Brand"] != DBNull.Value ?
                                       itemsReader["Brand"].ToString() : "N/A",
                                PartyName = itemsReader["PartyName"] != DBNull.Value ?
                                           itemsReader["PartyName"].ToString() : "N/A", // Keep internally
                                QtyBags = itemsReader["QtyBag"] != DBNull.Value ?
                                         Convert.ToInt32(itemsReader["QtyBag"]) : 0,
                                TotalQtyBags = itemsReader["TotalQtyBags"] != DBNull.Value ?
                                              Convert.ToInt32(itemsReader["TotalQtyBags"]) : 0,
                                RemBags = itemsReader["RemBags"] != DBNull.Value ?
                                         Convert.ToInt32(itemsReader["RemBags"]) : 0,
                                Weight = itemsReader["Weight"] != DBNull.Value ?
                                        Convert.ToDecimal(itemsReader["Weight"]) : 0,
                                Rate = itemsReader["Rate"] != DBNull.Value ?
                                      Convert.ToDecimal(itemsReader["Rate"]) : 0,
                                Amount = itemsReader["Amount"] != DBNull.Value ?
                                       Convert.ToDecimal(itemsReader["Amount"]) : 0
                            };

                            orderData.Items.Add(item);
                        }
                    }
                }
            }

            return orderData;
        }

        private void GeneratePDF(OrderInvoiceData orderData)
        {
            // Create new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = $"Invoice_{orderData.InvoiceNo}";

            // Create empty page
            PdfPage page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Define fonts
            XFont fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
            XFont fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
            XFont fontNormal = new XFont("Arial", 10);
            XFont fontBold = new XFont("Arial", 10, XFontStyle.Bold);
            XFont fontSmall = new XFont("Arial", 8);

            // Current Y position for drawing
            double yPos = 40;

            // Draw title
            gfx.DrawString(" 'AA' TRADERS INVOICE", fontTitle, XBrushes.Navy,
                          new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopCenter);
            yPos += 40;

            // Draw invoice details
            gfx.DrawString($"Invoice No: {orderData.InvoiceNo}", fontBold, XBrushes.Black, 50, yPos);
            gfx.DrawString($"Date: {orderData.OrderDate:dd/MM/yyyy}", fontBold, XBrushes.Black, 250, yPos);
            gfx.DrawString($"Time: {orderData.OrderDate:HH:mm}", fontBold, XBrushes.Black, 400, yPos);
            yPos += 25;

            // Draw customer information
            gfx.DrawString($"Customer Name: {orderData.CustomerName}", fontHeader, XBrushes.Black, 50, yPos);
            yPos += 25;

            yPos += 10;

            // Draw table headers with background - REMOVED PARTY COLUMN
            gfx.DrawRectangle(XBrushes.LightGray, 40, yPos - 5, 520, 20);
            gfx.DrawString("Sr. No.", fontBold, XBrushes.Black, 45, yPos);
            gfx.DrawString("Product", fontBold, XBrushes.Black, 90, yPos);
            gfx.DrawString("Brand", fontBold, XBrushes.Black, 200, yPos);  // More space for Brand
            gfx.DrawString("Qty Bags", fontBold, XBrushes.Black, 300, yPos);
            gfx.DrawString("Weight", fontBold, XBrushes.Black, 370, yPos);
            gfx.DrawString("Rate", fontBold, XBrushes.Black, 430, yPos);
            gfx.DrawString("Amount", fontBold, XBrushes.Black, 490, yPos);

            yPos += 20;

            // Draw items in grid format
            int srNo = 1;
            decimal totalAmount = 0;
            int totalBags = 0;
            decimal totalWeight = 0;

            foreach (var item in orderData.Items)
            {
                // Alternate row background for better readability
                if (srNo % 2 == 1)
                {
                    gfx.DrawRectangle(XBrushes.White, 40, yPos - 2, 520, 15);
                }
                else
                {
                    gfx.DrawRectangle(XBrushes.WhiteSmoke, 40, yPos - 2, 520, 15);
                }

                // Draw item details - DON'T SHOW PARTY NAME
                gfx.DrawString(srNo.ToString(), fontNormal, XBrushes.Black, 45, yPos);

                // Product Name (truncate if too long) - More space now that Party is removed
                string productName = item.ProductName.Length > 20 ? item.ProductName.Substring(0, 20) + "..." : item.ProductName;
                gfx.DrawString(productName, fontNormal, XBrushes.Black, 90, yPos);

                // Brand (truncate if too long) - More space now that Party is removed
                string brand = item.Brand.Length > 15 ? item.Brand.Substring(0, 15) + "..." : item.Brand;
                gfx.DrawString(brand, fontNormal, XBrushes.Black, 200, yPos);

                gfx.DrawString(item.QtyBags.ToString(), fontNormal, XBrushes.Black, 300, yPos);
                gfx.DrawString(item.Weight.ToString("N2"), fontNormal, XBrushes.Black, 370, yPos);
                gfx.DrawString(item.Rate.ToString("N2"), fontNormal, XBrushes.Black, 430, yPos);
                gfx.DrawString(item.Amount.ToString("N2"), fontNormal, XBrushes.Black, 490, yPos);

                // Accumulate totals
                totalBags += item.QtyBags;
                totalWeight += item.Weight;
                totalAmount += item.Amount;

                yPos += 15;
                srNo++;

                // Check if we need a new page
                if (yPos > page.Height - 150)
                {
                    // Add new page and reset position
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPos = 40;

                    // Redraw headers on new page - WITHOUT PARTY COLUMN
                    gfx.DrawRectangle(XBrushes.LightGray, 40, yPos - 5, 520, 20);
                    gfx.DrawString("Sr. No.", fontBold, XBrushes.Black, 45, yPos);
                    gfx.DrawString("Product", fontBold, XBrushes.Black, 90, yPos);
                    gfx.DrawString("Brand", fontBold, XBrushes.Black, 200, yPos);
                    gfx.DrawString("Qty Bags", fontBold, XBrushes.Black, 300, yPos);
                    gfx.DrawString("Weight", fontBold, XBrushes.Black, 370, yPos);
                    gfx.DrawString("Rate", fontBold, XBrushes.Black, 430, yPos);
                    gfx.DrawString("Amount", fontBold, XBrushes.Black, 490, yPos);
                    yPos += 20;
                }
            }

            // Draw summary section
            yPos += 10;
            gfx.DrawLine(XPens.Gray, 40, yPos, 560, yPos);
            yPos += 15;

            // Draw totals
            gfx.DrawString("Items Summary:", fontHeader, XBrushes.Black, 40, yPos);
            yPos += 20;

            gfx.DrawString($"Total Items: {orderData.Items.Count}", fontBold, XBrushes.Black, 40, yPos);
            gfx.DrawString($"Total Bags: {totalBags}", fontBold, XBrushes.Black, 200, yPos);
            yPos += 15;

            gfx.DrawString($"Total Weight: {totalWeight:N2} kg", fontBold, XBrushes.Black, 40, yPos);
            yPos += 15;

            // Draw total amount
            gfx.DrawString($"Total Amount:", fontHeader, XBrushes.Black, 350, yPos);
            gfx.DrawString($"Rs {totalAmount:N2}", fontHeader, XBrushes.Black, 470, yPos);
            yPos += 25;

            // Draw payment information
            gfx.DrawString("Payment Details:", fontHeader, XBrushes.Black, 40, yPos);
            yPos += 20;

            gfx.DrawString($"Cash: Rs {orderData.Cash:N2}", fontBold, XBrushes.Black, 40, yPos);
            gfx.DrawString($"Credit: Rs {orderData.Credit:N2}", fontBold, XBrushes.Black, 200, yPos);
            yPos += 15;

            gfx.DrawString($"Debit: Rs {orderData.Debit:N2}", fontBold, XBrushes.Black, 40, yPos);
            gfx.DrawString($"Balance: Rs {orderData.Balance:N2}", fontBold,
                          orderData.Balance >= 0 ? XBrushes.DarkGreen : XBrushes.DarkRed, 200, yPos);
            yPos += 25;

            // Draw footer
            gfx.DrawLine(XPens.Black, 40, yPos, 560, yPos);
            yPos += 15;

            gfx.DrawString("Thank you for your business!", fontNormal, XBrushes.Black, 40, yPos);
            yPos += 15;
            gfx.DrawString("Terms & Conditions: Goods once sold will not be taken back.", fontNormal, XBrushes.Gray, 40, yPos);
            yPos += 20;

            gfx.DrawString("Authorized Signature", fontNormal, XBrushes.Black, 450, yPos);
            gfx.DrawLine(XPens.Black, 450, yPos + 5, 550, yPos + 5);

            // Save PDF
            string fileName = $"Invoice_{orderData.InvoiceNo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            // Save and ensure file is closed before opening
            document.Save(fullPath);
            document.Close();

            // Wait a moment and then open
            System.Threading.Thread.Sleep(500);
            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
        }
    }

    // Helper classes for structured data - Keep PartyName property for internal use
    public class OrderInvoiceData
    {
        public int OrderID { get; set; }
        public int InvoiceNo { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Cash { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Balance { get; set; }
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        public int SP_ID { get; set; }
        public string ProductName { get; set; }
        public string Brand { get; set; }
        public string PartyName { get; set; } // Keep for internal use, but don't display in PDF
        public int QtyBags { get; set; }
        public int TotalQtyBags { get; set; }
        public int RemBags { get; set; }
        public decimal Weight { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}