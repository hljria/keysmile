using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Configuration;
using Microsoft.SqlServer;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
namespace keysmile
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string connstr = ConfigurationManager.ConnectionStrings["connstr"].ConnectionString;
            string selectcmdstr = "select 位置号, 项目编号, 条码, 份数 from 试剂信息表";
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                using (SqlCommand cmd = new SqlCommand(selectcmdstr, conn))
                {
                    try
                    {
                        conn.Open();//打开连接数据库
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            using (DataSet ds = new DataSet())
                            {
                                adapter.Fill(ds);
                                grid.ItemsSource = ds.Tables[0].DefaultView;
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        conn.Close();
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            string connstr = ConfigurationManager.ConnectionStrings["connstr"].ConnectionString;

            string selectcmdstr = string.Format("update 试剂信息表 set 份数='{0}' where 条码={1}", itemNum.Text,code.Text);
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                using (SqlCommand cmd = new SqlCommand(selectcmdstr, conn))
                {
                    try
                    {
                        conn.Open();//打开连接数据库
                        cmd.ExecuteNonQuery();

                    }
                    catch (SqlException ex)
                    {
                        conn.Close();
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
            Button_Click(sender, e);
        }

        private void grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedrow = grid.SelectedItem as DataRowView;
            if(selectedrow != null)
            {
                var data = selectedrow.Row;
                itemName.Text = data["位置号"].ToString();
                itemNum.Text = data["份数"].ToString();
                code.Text = data["条码"].ToString();
               // MessageBox.Show(data[5].ToString());
            }
        }

        private void itemNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length-1))
            { e.Handled = true; }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "查找剩余试剂探测文本文件";
            openFileDialog.Filter = "(*.txt)|*.txt|(*.*)|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = @"D:\";
            if(openFileDialog.ShowDialog() == true)
            {
                sysjLever.Text= openFileDialog.FileName;
                File.WriteAllText("记录.txt", sysjLever.Text,Encoding.UTF8);
                            
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string recordname = DateTime.Now.ToString("yyyy-MM-dd") + "试剂余量更新.txt";
            sysjLever.Text = File.ReadAllText("记录.txt")+recordname;
            //regestlefttxt.AppendText(File.ReadAllText(sysjLever.Text));
            readrecord();
        }
        
        private void readrecord()
        {
            StreamReader sr = new StreamReader(sysjLever.Text);
            string line="";
            string result="";
            string pattern = "\t";
            string pattern1 = @"\s\S*$";
            string pattern2 = @"\d*$";
            string pattern3 = @"-\d";

            int curline = 0;
            int lowest = 200;
            int curnum = 0;
            string replacement = " ";
            string replacement1 = "";
            line =sr.ReadLine();
            line=sr.ReadLine();            
            while (line != null)
            {                
                if (curline >= 3 ) 
                { 
                    curline = 0;
                    lowest = 200;
                    result=Regex.Replace(result, pattern3, replacement1);
                    regestlefttxt.AppendText(result + "\n");
                }
                curline++;
                if (line.Length > 30)
                {
                    line = line.Substring(9, 27);
                    //line = Regex.Replace(line, pattern, replacement);
                    line = Regex.Replace(line, pattern1, replacement1);
                    curnum = int.Parse(Regex.Match(line, pattern2).Value);
                    if (lowest > curnum) 
                    {
                        lowest = curnum;
                        result = line;
                    } 
                    
                }                
                line = sr.ReadLine();
            }
            regestlefttxt.AppendText(result + "\n");

        }
    }
}
