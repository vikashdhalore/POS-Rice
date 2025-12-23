using POS_Rice;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POS_Rice
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            //InitializeData();
            //AttachEvents();
        }

        //----------------------
        private void button1_Click(object sender, EventArgs e)
        {
            Party party = new Party();
            party.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Products products = new Products();
            products.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            InventoryForm inventoryform = new InventoryForm();
            inventoryform.Show();
        }

        private void btnSaleProduct_Click(object sender, EventArgs e)
        {
            saleProduct saleproduct = new saleProduct();
            saleproduct.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Stock stock = new Stock();
            stock.Show();
        }

        private void Main_Load(object sender, EventArgs e)
        {
          
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            frmLedger frmLedger = new frmLedger();
            frmLedger.Show();
            //Income income = new Income();
            //income.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            /*GeneralLedger grlLedger = new GeneralLedger();
            grlLedger.Show();*/

            GeneralLedgerForm gnrlLedgerForm = new GeneralLedgerForm();
            gnrlLedgerForm.Show();

        }

        private void button8_Click(object sender, EventArgs e)
        {
            frmSalesReport formSalesReport = new frmSalesReport();
            formSalesReport.Show();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            frmExpenseReport FormExpenseReport = new frmExpenseReport();
            FormExpenseReport.Show();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            frmSaleExpenseReport FormSaleExpenseReport = new frmSaleExpenseReport();
            FormSaleExpenseReport.Show();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            frmSaleExpenseReport FormSaleExpenseReport = new frmSaleExpenseReport();
            FormSaleExpenseReport.Show();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            frmExpenseReport FormExpenseReport = new frmExpenseReport();
            FormExpenseReport.Show();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            frmSalesReport formSalesReport = new frmSalesReport();
            formSalesReport.Show();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            frmPartyReport FormPartyReport = new frmPartyReport();
            FormPartyReport.Show();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            SignUpForm signUpForm = new SignUpForm();
            signUpForm.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ModernLoginForm mLoginForm = new ModernLoginForm();
            this.Close();
            mLoginForm.Show();

        }

        private void button14_Click(object sender, EventArgs e)
        {
            BackupRestoreForm backUpRestoreForm = new BackupRestoreForm();
            backUpRestoreForm.Show();
        }
        private void ShowChangePasswordForm()
        {
            ChangePasswordForm.ShowChangePassword();
        }
        private void button13_Click(object sender, EventArgs e)
        {
            ShowChangePasswordForm();
        }
    }
}

