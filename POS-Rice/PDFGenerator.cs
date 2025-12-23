using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace POS_Rice
{
    public class PDFGenerator
    {
        public static void GenerateLedgerPDF(DataGridView dataGridView, string accountName,
                                            DateTime fromDate, DateTime toDate,
                                            decimal currentBalance, decimal totalDebit, decimal totalCredit)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = $"{CleanFileName(accountName)}_Ledger_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                    DefaultExt = "pdf",
                    Title = "Save PDF File"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    CreatePDFDocument(saveFileDialog.FileName, dataGridView, accountName,
                                     fromDate, toDate, currentBalance, totalDebit, totalCredit);

                    // Optionally open the PDF after creation
                    if (MessageBox.Show("PDF created successfully! Would you like to open it now?",
                        "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CreatePDFDocument(string filePath, DataGridView dataGridView,
                                             string accountName, DateTime fromDate, DateTime toDate,
                                             decimal currentBalance, decimal totalDebit, decimal totalCredit)
        {
            try 
            {
                // Create new PDF document
                PdfDocument document = new PdfDocument();
                document.Info.Title = $"{accountName} - General Ledger";
                document.Info.Author = "AA Traders";
                document.Info.CreationDate = DateTime.Now;

                // Create A4 page in landscape
                PdfPage page = document.AddPage();
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                XGraphics gfx = XGraphics.FromPdfPage(page);

                XFont titleFont = new XFont("Arial", 16, XFontStyle.Bold);
                XFont headerFont = new XFont("Arial", 12, XFontStyle.Bold);
                XFont subHeaderFont = new XFont("Arial", 10, XFontStyle.Bold);
                XFont normalFont = new XFont("Arial", 9);
                XFont boldFont = new XFont("Arial", 9, XFontStyle.Bold);
                XFont amountFont = new XFont("Arial", 9);
                XFont footerFont = new XFont("Arial", 8);

                // Margins
                double leftMargin = 40;
                double topMargin = 40;
                double rightMargin = 40;
                double bottomMargin = 40;
                double usableWidth = page.Width - leftMargin - rightMargin;

                double yPos = topMargin;

                // Draw company header
                gfx.DrawString("PROJECT RICE MANAGEMENT SYSTEM", titleFont, XBrushes.Black,
                              new XRect(leftMargin, yPos, usableWidth, 0), XStringFormats.TopCenter);
                yPos += 25;

                // Draw ledger title
                gfx.DrawString($"{accountName.ToUpper()} - GENERAL LEDGER", headerFont, XBrushes.Black,
                              new XRect(leftMargin, yPos, usableWidth, 0), XStringFormats.TopCenter);
                yPos += 20;

                // Draw date range
                gfx.DrawString($"Period: {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}", subHeaderFont, XBrushes.Black,
                              new XRect(leftMargin, yPos, usableWidth, 0), XStringFormats.TopCenter);
                yPos += 15;

                // Draw print date
                gfx.DrawString($"Printed on: {DateTime.Now:dd/MM/yyyy hh:mm tt}", normalFont, XBrushes.Black,
                              new XRect(leftMargin, yPos, usableWidth, 0), XStringFormats.TopCenter);
                yPos += 20;

                // Draw summary section
                DrawSummarySection(gfx, leftMargin, ref yPos, currentBalance, totalDebit, totalCredit,
                                  boldFont, usableWidth);
                yPos += 20;

                // Draw table headers
                double columnWidth = usableWidth / 7;
                string[] headers = { "Date", "Voucher No", "Description", "Debit", "Credit", "Balance", "Source" };
                double[] columnWidths = { columnWidth * 0.8, columnWidth * 1.0, columnWidth * 2.0,
                                     columnWidth * 0.8, columnWidth * 0.8, columnWidth * 0.8, columnWidth * 0.8 };

                DrawTableHeader(gfx, leftMargin, yPos, headers, columnWidths, boldFont);
                yPos += 20;

                // Draw table rows
                DrawTableRows(gfx, leftMargin, ref yPos, dataGridView, normalFont, amountFont,
                             columnWidths, page.Height - bottomMargin);

                // Draw totals row at bottom
                DrawTotalsRow(gfx, leftMargin, page.Height - bottomMargin - 30,
                             totalDebit, totalCredit, columnWidths, boldFont);

                // Draw footer
                DrawFooter(gfx, page, leftMargin, usableWidth, bottomMargin, footerFont);

                // Save the document
                document.Save(filePath);
                document.Dispose();

            }

            catch (Exception ex) 
            {
                throw new Exception($"Error creating PDF document: {ex.Message}", ex);
            }

            /*  // Create new PDF document
              PdfDocument document = new PdfDocument();
              document.Info.Title = $"{accountName} - General Ledger";
              document.Info.Author = "Project Rice Management System";
              document.Info.CreationDate = DateTime.Now;

              // Define page size (A4 landscape)
              PdfSharpCore.PageSize pageSize = PdfSharpCore.PageSize.A4;
              double pageWidth = pageSize.Width.Point;   // 841.89 points
              double pageHeight = pageSize.Height.Point; // 595.28 points

              // Swap width/height for landscape orientation
              double landscapeWidth = pageHeight;   // 595.28
              double landscapeHeight = pageWidth;   // 841.89
            */
            // Add a page with landscape orientation





            // Set up fonts


        }

        private static void DrawSummarySection(XGraphics gfx, double leftMargin, ref double yPos,
                                              decimal currentBalance, decimal totalDebit, decimal totalCredit,
                                              XFont font, double usableWidth)
        {
            // Create summary background
            XRect summaryRect = new XRect(leftMargin, yPos, usableWidth, 25);
            gfx.DrawRectangle(XBrushes.LightGray, summaryRect);
            gfx.DrawRectangle(XPens.Black, summaryRect);

            double columnWidth = usableWidth / 3;

            // Draw summary headers
            string[] summaryHeaders = { "Current Balance", "Total Debit", "Total Credit" };
            for (int i = 0; i < 3; i++)
            {
                XRect rect = new XRect(leftMargin + (columnWidth * i), yPos, columnWidth, 25);
                gfx.DrawString(summaryHeaders[i], font, XBrushes.Black, rect, XStringFormats.Center);
            }

            yPos += 25;

            // Draw summary values
            XRect balanceRect = new XRect(leftMargin, yPos, columnWidth, 25);
            XRect debitRect = new XRect(leftMargin + columnWidth, yPos, columnWidth, 25);
            XRect creditRect = new XRect(leftMargin + (columnWidth * 2), yPos, columnWidth, 25);

            // Balance - green for positive, red for negative
            XBrush balanceBrush = currentBalance >= 0 ? XBrushes.DarkGreen : XBrushes.DarkRed;
            gfx.DrawString($"RS {currentBalance:N2}", font, balanceBrush, balanceRect, XStringFormats.Center);

            // Debit - red
            gfx.DrawString($"RS {totalDebit:N2}", font, XBrushes.DarkRed, debitRect, XStringFormats.Center);

            // Credit - green
            gfx.DrawString($"RS {totalCredit:N2}", font, XBrushes.DarkGreen, creditRect, XStringFormats.Center);

            yPos += 25;
        }

        private static void DrawTableHeader(XGraphics gfx, double leftMargin, double yPos,
                                           string[] headers, double[] columnWidths, XFont font)
        {
            double xPos = leftMargin;

            for (int i = 0; i < headers.Length; i++)
            {
                XRect cellRect = new XRect(xPos, yPos, columnWidths[i], 20);

                // Draw cell background
                gfx.DrawRectangle(XBrushes.LightGray, cellRect);
                gfx.DrawRectangle(XPens.Black, cellRect);

                // Draw header text
                XStringFormat format = new XStringFormat();
                if (i >= 3 && i <= 5) // Amount columns (Debit, Credit, Balance)
                {
                    format.Alignment = XStringAlignment.Far;
                    format.LineAlignment = XLineAlignment.Near;
                }
                else
                {
                    format.Alignment = XStringAlignment.Near;
                    format.LineAlignment = XLineAlignment.Near;
                }

                gfx.DrawString(headers[i], font, XBrushes.Black, cellRect, format);

                xPos += columnWidths[i];
            }
        }

        private static void DrawTableRows(XGraphics gfx, double leftMargin, ref double yPos,
                                 DataGridView dataGridView, XFont normalFont, XFont amountFont,
                                 double[] columnWidths, double maxHeight)
        {
            // Check if DataGridView has any rows
            if (dataGridView == null || dataGridView.Rows.Count == 0)
            {
                gfx.DrawString("No data available", normalFont, XBrushes.Black,
                              new XRect(leftMargin, yPos, 200, 20), XStringFormats.CenterLeft);
                return;
            }

            bool isAlternate = false;

            for (int rowIndex = 0; rowIndex < dataGridView.Rows.Count; rowIndex++)
            {
                DataGridViewRow row = dataGridView.Rows[rowIndex];

                // Skip new rows and null rows
                if (row == null || row.IsNewRow) continue;

                // Check if we need a new page
                if (yPos > maxHeight - 50)
                {
                    gfx.DrawString("--- Continued on next page ---", normalFont, XBrushes.Black,
                                  new XRect(leftMargin, yPos, 500, 20), XStringFormats.CenterLeft);
                    break;
                }

                double xPos = leftMargin;

                // Get cell values with null checks
                string transDate = GetCellValue(row, "TransactionDate") ??
                                  GetCellValue(row, "Date") ??
                                  GetCellValue(row, "TransDate") ?? "";
                string voucherNo = GetCellValue(row, "VoucherNo") ??
                                  GetCellValue(row, "Voucher") ??
                                  GetCellValue(row, "VoucherNumber") ?? "";
                string description = GetCellValue(row, "Description") ?? "";
                string debitStr = GetCellValue(row, "DebitAmount") ??
                                 GetCellValue(row, "Debit") ?? "0";
                string creditStr = GetCellValue(row, "CreditAmount") ??
                                  GetCellValue(row, "Credit") ?? "0";
                string balanceStr = GetCellValue(row, "Balance") ?? "0";
                string source = GetCellValue(row, "Source") ?? "";

                // Parse amounts with error handling
                if (!decimal.TryParse(debitStr, out decimal debitAmount))
                    debitAmount = 0;

                if (!decimal.TryParse(creditStr, out decimal creditAmount))
                    creditAmount = 0;

                if (!decimal.TryParse(balanceStr, out decimal balanceAmount))
                    balanceAmount = 0;

                // Draw row background for alternating colors
                if (isAlternate)
                {
                    XRect rowRect = new XRect(leftMargin, yPos, GetTotalWidth(columnWidths), 20);
                    gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(240, 240, 240)), rowRect);
                }

                // Draw cell borders and content
                for (int colIndex = 0; colIndex < columnWidths.Length; colIndex++)
                {
                    XRect cellRect = new XRect(xPos, yPos, columnWidths[colIndex] - 1, 20);
                    gfx.DrawRectangle(XPens.LightGray, cellRect);

                    string cellText = "";
                    XFont cellFont = normalFont;
                    XBrush cellBrush = XBrushes.Black;
                    XStringFormat format = new XStringFormat();

                    try
                    {
                        switch (colIndex)
                        {
                            case 0: // Date
                                cellText = FormatDate(transDate);
                                format.Alignment = XStringAlignment.Near;
                                break;
                            case 1: // Voucher No
                                cellText = voucherNo;
                                format.Alignment = XStringAlignment.Near;
                                break;
                            case 2: // Description
                                cellText = description.Length > 50 ? description.Substring(0, 47) + "..." : description;
                                format.Alignment = XStringAlignment.Near;
                                break;
                            case 3: // Debit
                                cellText = debitAmount > 0 ? debitAmount.ToString("N2") : "";
                                cellFont = amountFont;
                                cellBrush = debitAmount > 0 ? XBrushes.DarkRed : XBrushes.Black;
                                format.Alignment = XStringAlignment.Far;
                                break;
                            case 4: // Credit
                                cellText = creditAmount > 0 ? creditAmount.ToString("N2") : "";
                                cellFont = amountFont;
                                cellBrush = creditAmount > 0 ? XBrushes.DarkGreen : XBrushes.Black;
                                format.Alignment = XStringAlignment.Far;
                                break;
                            case 5: // Balance
                                cellText = balanceAmount.ToString("N2");
                                cellFont = new XFont("Arial", 9, XFontStyle.Bold);
                                cellBrush = balanceAmount >= 0 ? XBrushes.DarkGreen : XBrushes.DarkRed;
                                format.Alignment = XStringAlignment.Far;
                                break;
                            case 6: // Source
                                cellText = source;
                                format.Alignment = XStringAlignment.Near;
                                break;
                        }

                        format.LineAlignment = XLineAlignment.Near;

                        if (!string.IsNullOrEmpty(cellText))
                        {
                            gfx.DrawString(cellText, cellFont, cellBrush, cellRect, format);
                        }
                    }
                    catch (Exception)
                    {
                        // Log error or draw error indicator
                        gfx.DrawString("Error", normalFont, XBrushes.Red, cellRect, format);
                    }

                    xPos += columnWidths[colIndex];
                }

                yPos += 20;
                isAlternate = !isAlternate;
            }
        }
        private static void DrawTotalsRow(XGraphics gfx, double leftMargin, double yPos,
                                         decimal totalDebit, decimal totalCredit,
                                         double[] columnWidths, XFont font)
        {
            double xPos = leftMargin;

            // Draw totals background
            XRect totalsRect = new XRect(leftMargin, yPos, GetTotalWidth(columnWidths), 25);
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(220, 220, 220)), totalsRect);
            gfx.DrawRectangle(XPens.Black, totalsRect);

            // "TOTALS" text spanning first 3 columns
            double firstThreeCols = columnWidths[0] + columnWidths[1] + columnWidths[2];
            XRect totalsLabelRect = new XRect(xPos, yPos, firstThreeCols, 25);
            gfx.DrawString("TOTALS:", font, XBrushes.Black, totalsLabelRect,
                          new XStringFormat { Alignment = XStringAlignment.Far });

            xPos += firstThreeCols;

            // Debit total
            XRect debitTotalRect = new XRect(xPos, yPos, columnWidths[3], 25);
            gfx.DrawString($"RS {totalDebit:N2}", font, XBrushes.DarkRed, debitTotalRect,
                          new XStringFormat { Alignment = XStringAlignment.Far });
            xPos += columnWidths[3];

            // Credit total
            XRect creditTotalRect = new XRect(xPos, yPos, columnWidths[4], 25);
            gfx.DrawString($"RS {totalCredit:N2}", font, XBrushes.DarkGreen, creditTotalRect,
                          new XStringFormat { Alignment = XStringAlignment.Far });
        }

        private static void DrawFooter(XGraphics gfx, PdfPage page, double leftMargin,
                                      double usableWidth, double bottomMargin, XFont font)
        {
            // Draw page number
            string footerText = $"Page 1 of 1 | Generated by Project Rice System";
            gfx.DrawString(footerText, font, XBrushes.Black,
                          new XRect(leftMargin, page.Height - bottomMargin, usableWidth, 20),
                          XStringFormats.Center);

            // Draw end of report indicator
            gfx.DrawString("*** End of Report ***", font, XBrushes.Black,
                          new XRect(leftMargin, page.Height - bottomMargin + 15, usableWidth, 20),
                          XStringFormats.Center);
        }

        #region Helper Methods

        private static double GetTotalWidth(double[] columnWidths)
        {
            double total = 0;
            foreach (double width in columnWidths)
            {
                total += width;
            }
            return total;
        }

        private static string GetCellValue(DataGridViewRow row, string columnName)
        {
            try
            {
                // First, try to get by column name
              //  if (row.Cells.Contains(columnName) && row.Cells[columnName]?.Value != null)
                //    return row.Cells[columnName].Value.ToString();

                // If not found, try to find by index or header text
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null && cell.OwningColumn.HeaderText == columnName)
                        return cell.Value.ToString();
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        private static string FormatDate(string dateStr)
        {
            if (DateTime.TryParse(dateStr, out DateTime date))
                return date.ToString("dd/MM/yyyy");
            return dateStr;
        }

        private static string CleanFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        #endregion
    }
}