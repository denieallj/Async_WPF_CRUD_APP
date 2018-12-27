using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CRUD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            load_list();
        }

        #region Methods

        public Task<DataTable> get_all_cars()
        {
            // This is a separate thread so it wont blocked the main thread
            // The ui window can move without freezing

            return Task.Run(() =>
            {
                // Get the connection string after adding data connection and data sources (The left side tab)
                string conn_string = Properties.Settings.Default.carsdbConnectionString;

                // Create and open the connection to sql server
                SqlConnection conn = new SqlConnection(conn_string);

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                string sql = "SELECT * FROM tbl_cars";

                // Create an empty table
                DataTable tbl = new DataTable();

                // The result is kept in the adapter and the adapter will fill the data table
                SqlDataAdapter adapter = new SqlDataAdapter(sql, conn_string);

                // Fill the table
                adapter.Fill(tbl);

                // Close the connection
                conn.Close();

                return tbl;
            });
        }

        public async void load_list(string operation = "default", int id = 0, string name = "")
        {
            DataTable tbl;

            if (operation == "default")
            {
                tbl = await get_all_cars();
            }
            else
            {
                tbl = await exec_stored_procedure(id, name);
            }

            // Display data into list box
            car_list.DisplayMemberPath = "cars";
            car_list.SelectedValue = "id";
            car_list.ItemsSource = tbl.DefaultView;
            
        }

        private async Task add_Entry_To_DB(String entry)
        {
            // Get the connection string after adding data connection and data sources (The left side tab)
            string conn_string = Properties.Settings.Default.carsdbConnectionString;

            // Create and open the connection to sql server
            SqlConnection conn = new SqlConnection(conn_string);

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            // Inserting data to db async way
            String sql_insert = "INSERT INTO tbl_cars (cars) VALUES ('"+ entry +"')";

            SqlCommand cmd = new SqlCommand(sql_insert, conn);

            await cmd.ExecuteNonQueryAsync();

            conn.Close();
        }

        private async Task delete_Entry(String id)
        {
            // Get the connection string after adding data connection and data sources (The left side tab)
            string conn_string = Properties.Settings.Default.carsdbConnectionString;

            // Create and open the connection to sql server
            SqlConnection conn = new SqlConnection(conn_string);

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            // Deleting data in db async way
            String sql_delete = "DELETE FROM tbl_cars WHERE id = " + id;

            SqlCommand cmd = new SqlCommand(sql_delete, conn);

            await cmd.ExecuteNonQueryAsync();

            // Close DB connection
            conn.Close();
        }

        private async Task update_Entry(String id, String car)
        {
            // Get the connection string after adding data connection and data sources (The left side tab)
            string conn_string = Properties.Settings.Default.carsdbConnectionString;

            // Create and open the connection to sql server
            SqlConnection conn = new SqlConnection(conn_string);

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            // Deleting data in db async way
            String sql_delete = "UPDATE tbl_cars SET cars = '" + car + "' WHERE id = " + id;

            SqlCommand cmd = new SqlCommand(sql_delete, conn);

            await cmd.ExecuteNonQueryAsync();

            // Close DB connection
            conn.Close();
        }

        private Task<DataTable> exec_stored_procedure(int id, String name)
        {
            return Task.Run(() => {
                // Get the connection string after adding data connection and data sources (The left side tab)
                string conn_string = Properties.Settings.Default.carsdbConnectionString;

                // Create and open the connection to sql server
                SqlConnection conn = new SqlConnection(conn_string);

                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                string sql = "EXEC dbo.GetHondaCar @id = " + id + ", @cars = '" + name + "';";

                // Create an empty table
                DataTable tbl = new DataTable();

                // The result is kept in the adapter and the adapter will fill the data table
                SqlDataAdapter adapter = new SqlDataAdapter(sql, conn_string);

                // Fill the table
                adapter.Fill(tbl);

                // Close the connection
                conn.Close();

                return tbl;
            });
        }  

        #endregion /Methods

        #region Buttons
        private async void Btn_add_Click(object sender, RoutedEventArgs e)
        {
            String entry = txt_entry.Text;

            if (entry.Trim() != "")
            {
                // Add the data to db
                await add_Entry_To_DB(entry);

                // Load old and new data
                load_list();

                // Clear the textbox
                txt_entry.Text = "";
            }

        }

        private async void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            // Get the current id and car name in the list box
            DataRowView row = car_list.SelectedItem as DataRowView;
            
            if (row != null)
            {
                string carID = row["id"].ToString();
                // string carName = row["cars"].ToString();

                await delete_Entry(carID);
                load_list();
            }
            else
            {
                MessageBox.Show("No data selected!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private async void Btn_update_Click(object sender, RoutedEventArgs e)
        {
            // Get the current id and car name in the list box
            DataRowView row = car_list.SelectedItem as DataRowView;

            if (row != null)
            {

                string carID = row["id"].ToString();

                String entry = txt_entry.Text;

                if (entry.Trim() != "")
                {
                    await update_Entry(carID, entry);
                    load_list();

                    txt_entry.Text = "";
                }
                else
                {
                    MessageBox.Show("Textbox is empty!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            else
            {
                MessageBox.Show("No data selected!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void Btn_exec_Click(object sender, RoutedEventArgs e)
        {
            load_list("stored procedure", 2, "Honda");
        }

        private void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            txt_entry.Text = "";
            load_list();
        }

        #endregion /Buttons

        private void Car_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView row = car_list.SelectedItem as DataRowView;

            if (row != null)
            {
                txt_entry.Text = row["cars"].ToString();
            }
            
        }
    }
}
