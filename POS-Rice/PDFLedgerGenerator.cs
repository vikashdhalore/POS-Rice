using System;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Drawing;

namespace POS_Rice
{
    public class PDFLedgerGenerator
    {
        public void GenerateLedgerPDF(DataGridView dataGridView, DateTime fromDate, DateTime toDate,
                                    decimal openingBalance, decimal totalDebit, decimal totalCredit, decimal closingBalance)
        {
            try
            {
                // Create new PDF document
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Ledger Report";

                // Create empty page
                PdfPage page = document.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Define fonts (slightly smaller to fit more content)
                XFont fontTitle = new XFont("Arial", 14, XFontStyle.Bold);
                XFont fontHeader = new XFont("Arial", 8, XFontStyle.Bold);
                XFont fontNormal = new XFont("Arial", 7);
                XFont fontBold = new XFont("Arial", 8, XFontStyle.Bold);
                XFont fontSmall = new XFont("Arial", 7);

                // Current Y position for drawing
                double yPos = 30;
                double leftMargin = 20;
                double rightMargin = page.Width - 20;
                double pageWidth = page.Width - 40;

                // Draw title and header
                gfx.DrawString("LEDGER REPORT", fontTitle, XBrushes.Navy,
                              new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopCenter);
                yPos += 25;

                // Draw date range
                gfx.DrawString($"Period: {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}", fontBold,
                              XBrushes.Black, leftMargin, yPos);
                yPos += 15;

                // Draw summary information in a compact way
                gfx.DrawString($"Opening: {openingBalance:N2}", fontBold, XBrushes.Black, leftMargin, yPos);
                gfx.DrawString($"Debit: {totalDebit:N2}", fontBold, XBrushes.DarkGreen, leftMargin + 120, yPos);
                gfx.DrawString($"Credit: {totalCredit:N2}", fontBold, XBrushes.DarkRed, leftMargin + 240, yPos);
                gfx.DrawString($"Closing: {closingBalance:N2}", fontBold,
                              closingBalance >= 0 ? XBrushes.DarkGreen : XBrushes.DarkRed,
                              leftMargin + 360, yPos);
                yPos += 20;

                // Draw table headers
                DrawTableHeaders(gfx, fontHeader, leftMargin, ref yPos, pageWidth);

                // Draw data rows
                int rowNumber = 1;
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.IsNewRow && row.Visible)
                    {
                        if (yPos > page.Height - 50) // Check if we need new page (more strict)
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPos = 30;
                            DrawTableHeaders(gfx, fontHeader, leftMargin, ref yPos, pageWidth);
                        }

                        DrawDataRow(gfx, fontNormal, leftMargin, ref yPos, row, rowNumber++, pageWidth);
                    }
                }

                // Draw footer
                yPos += 15;
                gfx.DrawLine(XPens.Gray, leftMargin, yPos, rightMargin, yPos);
                yPos += 8;
                gfx.DrawString($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSmall,
                              XBrushes.Gray, leftMargin, yPos);
                gfx.DrawString($"Page 1 of 1", fontSmall, XBrushes.Gray,
                              new XRect(0, yPos, page.Width - leftMargin, page.Height),
                              XStringFormats.TopRight);

                // Save PDF
                string fileName = $"Ledger_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fullPath = Path.Combine(desktopPath, fileName);

                document.Save(fullPath);
                document.Close();

                // Open the PDF
                MessageBox.Show($"PDF generated successfully!\n\nSaved to: {fullPath}", "Success",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawTableHeaders(XGraphics gfx, XFont font, double leftMargin, ref double yPos, double pageWidth)
        {
            // Draw header background
            gfx.DrawRectangle(XBrushes.LightGray, leftMargin, yPos - 3, pageWidth, 15);

            // Define column widths (optimized for A4 page - removed PartyID and Source)
            double[] columnWidths = { 25, 55, 75, 70, 200, 70, 70 }; // Total: 565 (fits A4)
            string[] headers = { "Sr.", "Date", "Voucher Type", "Voucher No", "Description", "Debit", "Credit" };

            double xPos = leftMargin;
            for (int i = 0; i < headers.Length; i++)
            {
                XStringFormat format = (i >= 5) ? XStringFormats.TopCenter : XStringFormats.TopLeft;

                gfx.DrawString(headers[i], font, XBrushes.Black,
                              new XRect(xPos, yPos, columnWidths[i], 15), format);
                xPos += columnWidths[i];
            }

            yPos += 15;
        }

        private void DrawDataRow(XGraphics gfx, XFont font, double leftMargin, ref double yPos,
                               DataGridViewRow row, int rowNumber, double pageWidth)
        {
            // Alternate row background
            if (rowNumber % 2 == 1)
            {
                gfx.DrawRectangle(XBrushes.White, leftMargin, yPos - 1, pageWidth, 12);
            }
            else
            {
                gfx.DrawRectangle(XBrushes.WhiteSmoke, leftMargin, yPos - 1, pageWidth, 12);
            }

            // Define column widths (same as headers - removed PartyID and Source)
            double[] columnWidths = { 25, 55, 75, 70, 200, 70, 70 };

            double xPos = leftMargin;

            // Sr. No. (Center aligned)
            gfx.DrawString(rowNumber.ToString(), font, XBrushes.Black,
                          new XRect(xPos, yPos, columnWidths[0], 12), XStringFormats.TopCenter);
            xPos += columnWidths[0];

            // Date
            string date = GetSafeString(row.Cells["TransactionDate"].Value);
            if (DateTime.TryParse(date, out DateTime transDate))
            {
                gfx.DrawString(transDate.ToString("dd/MM/yy"), font, XBrushes.Black,
                              new XRect(xPos, yPos, columnWidths[1], 12), XStringFormats.TopLeft);
            }
            xPos += columnWidths[1];

            // Voucher Type
            string voucherType = GetSafeString(row.Cells["VoucherType"].Value);
            gfx.DrawString(TruncateString(voucherType, 12), font, XBrushes.Black,
                          new XRect(xPos, yPos, columnWidths[2], 12), XStringFormats.TopLeft);
            xPos += columnWidths[2];

            // Voucher No
            string voucherNo = GetSafeString(row.Cells["VoucherNo"].Value);
            gfx.DrawString(TruncateString(voucherNo, 10), font, XBrushes.Black,
                          new XRect(xPos, yPos, columnWidths[3], 12), XStringFormats.TopLeft);
            xPos += columnWidths[3];

            // Description (maximum space allocated)
            string description = GetSafeString(row.Cells["Description"].Value);
            gfx.DrawString(TruncateString(description, 40), font, XBrushes.Black,
                          new XRect(xPos, yPos, columnWidths[4], 12), XStringFormats.TopLeft);
            xPos += columnWidths[4];

            // Debit Amount (Right aligned)
            string debitStr = GetSafeString(row.Cells["DebitAmount"].Value);
            if (decimal.TryParse(debitStr, out decimal debit))
            {
                if (debit != 0)
                {
                    gfx.DrawString(debit.ToString("N2"), font, XBrushes.DarkGreen,
                                  new XRect(xPos, yPos, columnWidths[5], 12), XStringFormats.TopRight);
                }
                else
                {
                    gfx.DrawString("-", font, XBrushes.Gray,
                                  new XRect(xPos, yPos, columnWidths[5], 12), XStringFormats.TopCenter);
                }
            }
            xPos += columnWidths[5];

            // Credit Amount (Right aligned)
            string creditStr = GetSafeString(row.Cells["CreditAmount"].Value);
            if (decimal.TryParse(creditStr, out decimal credit))
            {
                if (credit != 0)
                {
                    gfx.DrawString(credit.ToString("N2"), font, XBrushes.DarkRed,
                                  new XRect(xPos, yPos, columnWidths[6], 12), XStringFormats.TopRight);
                }
                else
                {
                    gfx.DrawString("-", font, XBrushes.Gray,
                                  new XRect(xPos, yPos, columnWidths[6], 12), XStringFormats.TopCenter);
                }
            }

            yPos += 12;
        }

        private string GetSafeString(object value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;
            return value.ToString();
        }

        private string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}