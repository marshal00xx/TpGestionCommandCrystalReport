using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;

namespace tpCrystalReportGestionCommand {
    public partial class GestionClient : Form {
        public GestionClient() {
            InitializeComponent();
        }
        public static SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder() {
            DataSource = ".",
            InitialCatalog = "NorthWind",
            IntegratedSecurity = true
        };
        static SqlConnection connection = new SqlConnection(connectionString.ToString());
        static int OrderID = 0;
        SqlCommand command = new SqlCommand();
        DataTable table = new DataTable();
        private void GestionClient_Load(object sender, EventArgs e) {
            connection.Open();
            command.Connection = connection;
            try {
                command.CommandText = "SELECT * FROM orderIdsView";
                table.Load(command.ExecuteReader());
                // set the datasource of dgv to table 
                dgv.DataSource = table;
                OrderID = Convert.ToInt32(dgv.Rows[0].Cells["OrderID"].Value);
                // set the datasource of cb to a new filtered list
                HashSet<String> set = new HashSet<String>(
                    table.Columns[0].Table.AsEnumerable().Select(row => row.Field<String>("CustomerID"))
                );
                List<String> list = new List<String>(set);
                list.Insert(0, String.Empty);
                clientsCB.DataSource = list;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        private void clientsCB_SelectedValueChanged(object sender, EventArgs e) {
            if (!clientsCB.Text.Equals(String.Empty)) {
                try {
                    DataTable filteredTable = table.AsEnumerable()
                        .Where(row => row.Field<String>("CustomerID") == clientsCB.Text)
                        .CopyToDataTable();
                    dgv.DataSource = filteredTable;
                }
                catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void printButton_Click(object sender, EventArgs e) {
            try {
                loadReport(OrderID);
                report.PrintReport();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        // method to load data to d report
        private void loadReport(int orderID) {
            try {
                /*,*/
                SqlDataAdapter adapter = new SqlDataAdapter(
                    "SELECT orderDate FROM Orders WHERE OrderID = @OrderID " +
                                    "SELECT customerId, companyName, address, phone FROM Customers " +
                                    "SELECT productName FROM Products WHERE ProductID IN (SELECT ProductID FROM [Order Details] WHERE OrderID = @OrderID)" +
                                    "SELECT orderId, unitPrice, quantity, discount FROM [Order Details] WHERE OrderID = @OrderID"
                    , connection);

                adapter.SelectCommand.Parameters.AddWithValue("@OrderID", orderID);
                // filling the dataset
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);

                // linking the report with our dataset
                FactureX factureX = new FactureX();
                factureX.SetDataSource(dataSet);
                factureX.SetParameterValue("orderIdParameter", OrderID);
                report.ReportSource = factureX;
                report.Refresh();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        private void dgv_MouseClick(object sender, MouseEventArgs e) {
            OrderID = Convert.ToInt32(dgv.CurrentRow.Cells["OrderID"].Value);
        }
    }
}
