using System;
using System.Drawing;
using System.Windows.Forms;

partial class frmLedger
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.DateTimePicker dateFrom;
    private System.Windows.Forms.DateTimePicker dateTo;
    private System.Windows.Forms.ComboBox comboParty;
    private System.Windows.Forms.ComboBox comboVoucherType;
    private System.Windows.Forms.TextBox textSearch;
    private System.Windows.Forms.DataGridView dataGridViewLedger;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Button btnExport;
    private System.Windows.Forms.Label lblTotalDebit;
    private System.Windows.Forms.Label lblTotalCredit;
    private System.Windows.Forms.Label lblOpeningBalance;
    private System.Windows.Forms.Label lblClosingBalance;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.dateFrom = new System.Windows.Forms.DateTimePicker();
            this.dateTo = new System.Windows.Forms.DateTimePicker();
            this.comboParty = new System.Windows.Forms.ComboBox();
            this.comboVoucherType = new System.Windows.Forms.ComboBox();
            this.textSearch = new System.Windows.Forms.TextBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.dataGridViewLedger = new System.Windows.Forms.DataGridView();
            this.lblOpeningBalance = new System.Windows.Forms.Label();
            this.lblTotalDebit = new System.Windows.Forms.Label();
            this.lblTotalCredit = new System.Windows.Forms.Label();
            this.lblClosingBalance = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lblCashBalance = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLedger)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(20, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "From:";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(180, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "To:";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(349, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Party:";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(20, 109);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 20);
            this.label5.TabIndex = 4;
            this.label5.Text = "Type:";
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(180, 109);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 20);
            this.label6.TabIndex = 5;
            this.label6.Text = "Search:";
            // 
            // dateFrom
            // 
            this.dateFrom.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateFrom.Location = new System.Drawing.Point(60, 77);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Size = new System.Drawing.Size(100, 25);
            this.dateFrom.TabIndex = 6;
            this.dateFrom.ValueChanged += new System.EventHandler(this.dateFrom_ValueChanged);
            // 
            // dateTo
            // 
            this.dateTo.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTo.Location = new System.Drawing.Point(210, 77);
            this.dateTo.Name = "dateTo";
            this.dateTo.Size = new System.Drawing.Size(100, 25);
            this.dateTo.TabIndex = 7;
            this.dateTo.ValueChanged += new System.EventHandler(this.dateTo_ValueChanged);
            // 
            // comboParty
            // 
            this.comboParty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboParty.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboParty.Location = new System.Drawing.Point(389, 77);
            this.comboParty.Name = "comboParty";
            this.comboParty.Size = new System.Drawing.Size(207, 25);
            this.comboParty.TabIndex = 8;
            this.comboParty.SelectedIndexChanged += new System.EventHandler(this.comboParty_SelectedIndexChanged);
            // 
            // comboVoucherType
            // 
            this.comboVoucherType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboVoucherType.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboVoucherType.Location = new System.Drawing.Point(60, 109);
            this.comboVoucherType.Name = "comboVoucherType";
            this.comboVoucherType.Size = new System.Drawing.Size(100, 25);
            this.comboVoucherType.TabIndex = 10;
            this.comboVoucherType.SelectedIndexChanged += new System.EventHandler(this.comboVoucherType_SelectedIndexChanged);
            // 
            // textSearch
            // 
            this.textSearch.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textSearch.Location = new System.Drawing.Point(230, 109);
            this.textSearch.Name = "textSearch";
            this.textSearch.Size = new System.Drawing.Size(200, 25);
            this.textSearch.TabIndex = 11;
            this.textSearch.TextChanged += new System.EventHandler(this.textSearch_TextChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(450, 109);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 25);
            this.btnRefresh.TabIndex = 12;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExport.ForeColor = System.Drawing.Color.White;
            this.btnExport.Location = new System.Drawing.Point(529, 109);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 25);
            this.btnExport.TabIndex = 13;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // dataGridViewLedger
            // 
            this.dataGridViewLedger.AllowUserToAddRows = false;
            this.dataGridViewLedger.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewLedger.BackgroundColor = System.Drawing.SystemColors.ActiveCaption;
            this.dataGridViewLedger.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewLedger.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewLedger.Location = new System.Drawing.Point(7, 139);
            this.dataGridViewLedger.Name = "dataGridViewLedger";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewLedger.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewLedger.RowHeadersVisible = false;
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.dataGridViewLedger.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewLedger.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewLedger.Size = new System.Drawing.Size(981, 513);
            this.dataGridViewLedger.TabIndex = 14;
            // 
            // lblOpeningBalance
            // 
            this.lblOpeningBalance.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblOpeningBalance.Location = new System.Drawing.Point(8, 656);
            this.lblOpeningBalance.Name = "lblOpeningBalance";
            this.lblOpeningBalance.Size = new System.Drawing.Size(222, 20);
            this.lblOpeningBalance.TabIndex = 15;
            this.lblOpeningBalance.Text = "Opening Balance: 0.00";
            // 
            // lblTotalDebit
            // 
            this.lblTotalDebit.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalDebit.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblTotalDebit.Location = new System.Drawing.Point(237, 656);
            this.lblTotalDebit.Name = "lblTotalDebit";
            this.lblTotalDebit.Size = new System.Drawing.Size(221, 20);
            this.lblTotalDebit.TabIndex = 16;
            this.lblTotalDebit.Text = "Total Debit: 0.00";
            // 
            // lblTotalCredit
            // 
            this.lblTotalCredit.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalCredit.ForeColor = System.Drawing.Color.DarkRed;
            this.lblTotalCredit.Location = new System.Drawing.Point(464, 656);
            this.lblTotalCredit.Name = "lblTotalCredit";
            this.lblTotalCredit.Size = new System.Drawing.Size(236, 20);
            this.lblTotalCredit.TabIndex = 17;
            this.lblTotalCredit.Text = "Total Credit: 0.00";
            // 
            // lblClosingBalance
            // 
            this.lblClosingBalance.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblClosingBalance.Location = new System.Drawing.Point(739, 656);
            this.lblClosingBalance.Name = "lblClosingBalance";
            this.lblClosingBalance.Size = new System.Drawing.Size(244, 20);
            this.lblClosingBalance.TabIndex = 18;
            this.lblClosingBalance.Text = "Closing Balance: 0.00";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.panel1.Controls.Add(this.label7);
            this.panel1.Location = new System.Drawing.Point(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(998, 60);
            this.panel1.TabIndex = 19;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(461, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(114, 37);
            this.label7.TabIndex = 0;
            this.label7.Text = "Finance";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.DarkTurquoise;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(608, 109);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 26);
            this.button1.TabIndex = 20;
            this.button1.Text = "Cashbook Entry";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(761, 86);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 20);
            this.label4.TabIndex = 21;
            this.label4.Text = "Cash Balance";
            // 
            // lblCashBalance
            // 
            this.lblCashBalance.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblCashBalance.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCashBalance.ForeColor = System.Drawing.Color.Goldenrod;
            this.lblCashBalance.Location = new System.Drawing.Point(854, 64);
            this.lblCashBalance.Name = "lblCashBalance";
            this.lblCashBalance.Size = new System.Drawing.Size(134, 68);
            this.lblCashBalance.TabIndex = 22;
            this.lblCashBalance.Text = "0.00";
            this.lblCashBalance.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmLedger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Controls.Add(this.lblCashBalance);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.dateFrom);
            this.Controls.Add(this.dateTo);
            this.Controls.Add(this.comboParty);
            this.Controls.Add(this.comboVoucherType);
            this.Controls.Add(this.textSearch);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.dataGridViewLedger);
            this.Controls.Add(this.lblOpeningBalance);
            this.Controls.Add(this.lblTotalDebit);
            this.Controls.Add(this.lblTotalCredit);
            this.Controls.Add(this.lblClosingBalance);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmLedger";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "General Ledger";
            this.Load += new System.EventHandler(this.frmLedger_Load_2);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewLedger)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    private Panel panel1;
    private Label label7;
    private Button button1;
    private Label label4;
    private Label lblCashBalance;
}