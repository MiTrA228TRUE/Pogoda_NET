using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Microsoft.Data.Sqlite;
using HtmlAgilityPack;
using System.Security.Policy;
using System.Net;
using System.Net.Http;
using System.Diagnostics;

namespace Pogoda_NET
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<City> cityList;
        public ObservableCollection<City> CityList
        {
            get { return cityList; }
            set
            {
                cityList = value;
                OnPropertyChanged("CityList");
            }
        }

        HtmlWeb web = new HtmlWeb();
        
        List<Label> Days = new List<Label>();
        List<Label> Max_temp = new List<Label>();
        List<Label> Min_temp = new List<Label>();
        List<TextBlock> Pogod = new List<TextBlock>();

        private City selectedCity;
        public City SelectedCity
        {
            get { return selectedCity; }
            set
            {
                selectedCity = value;
                OnPropertyChanged("SelectedCity");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadCitiesFromDatabase();
            Days.AddRange(new List<Label> { lb_day_1, lb_day_2, lb_day_3, lb_day_4, lb_day_5, lb_day_6, lb_day_7 });
            Max_temp.AddRange(new List<Label> { lb_max_1, lb_max_2, lb_max_3, lb_max_4, lb_max_5, lb_max_6, lb_max_7 });
            Min_temp.AddRange(new List<Label> { lb_min_1, lb_min_2, lb_min_3, lb_min_4, lb_min_5, lb_min_6, lb_min_7 });
            Pogod.AddRange(new List<TextBlock> { lb_pog_1, lb_pog_2, lb_pog_3, lb_pog_4, lb_pog_5, lb_pog_6, lb_pog_7, });
        }


        private void LoadCitiesFromDatabase()
        {
            using (var connection = new SqliteConnection("Data Source=usersdata.db"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Users";
                using (var reader = command.ExecuteReader())
                {
                    CityList = new ObservableCollection<City>();
                    while (reader.Read())
                    {
                        City city = new City
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            District = reader.GetString(2),
                            Region = reader.GetString(3),
                            Link = reader.GetString(4)
                        };
                        CityList.Add(city);
                    }
                }
            }
        }

        /*private void textBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = textBoxSearch.Text.ToLower();

            if (!string.IsNullOrEmpty(searchText))
            {
                comboBoxCities.ItemsSource = CityList.Where(city => city.Name.ToLower().Contains(searchText));
                comboBoxCities.IsDropDownOpen = true;
            }
            else
            {
                comboBoxCities.ItemsSource = CityList;
                comboBoxCities.IsDropDownOpen = false;
            }
        }*/

        private void comboBoxCities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
                SelectedCity = comboBoxCities.SelectedItem as City;
                if (SelectedCity != null)
                {
                    if (!string.IsNullOrEmpty(SelectedCity.Link))
                    {
                        // Открываем ссылку во внешнем браузере или выводим ее в lb_temp
                        //System.Diagnostics.Process.Start(new ProcessStartInfo(SelectedCity.Link) { UseShellExecute = true });
                        Run_Temp(SelectedCity.Link);
                        lb_name.Content = SelectedCity.Name;
                    }
                }

                //textBoxSearch.Text = "";
                comboBoxCities.IsDropDownOpen = false;
            
        }

        private void Run_Temp(string Link) 
        {
            var web = new HtmlWeb();
            var doc = web.Load(Link);
            var doc10 = web.Load(Link + "10-days/");
            var temp = doc.DocumentNode.SelectNodes("/html/body/section[2]/div[1]/section[2]/div/a[1]/div/div[1]/div[3]/div[1]/span[1]");
            var data = doc10.DocumentNode.SelectNodes("/html/body/section[2]/div[1]/section[2]/div[1]/div/div/div[1]/a[position() >= 1 and position() <= 10]/div[2]");
            var max_temp = doc10.DocumentNode.SelectNodes("/html/body/section[2]/div[1]/section[2]/div[1]/div/div/div[3]/div/div/div[position() >= 1 and position() <= 10]/div[1]/span[1]");
            var min_temp = doc10.DocumentNode.SelectNodes("/html/body/section[2]/div[1]/section[2]/div[1]/div/div/div[3]/div/div/div[position() >= 1 and position() <= 10]/div[2]/span[1]");
            var pogod = doc10.DocumentNode.SelectNodes("/html/body/section[2]/div[1]/section[2]/div[1]/div/div/div[2]/div[position() >= 1 and position() <= 10]/div");

            //var min_temp;
            //var pogod;

            if (temp[0].InnerText.Contains("&minus;"))
                lb_temp.Content = temp[0].InnerText.Replace("&minus;", "-");
            else
                lb_temp.Content = temp[0].InnerText;
            
            for (int i = 0; i < Days.Count && i < data.Count; i++)
            {
   
                Days[i].Content = data[i].InnerHtml;

                Pogod[i].Text = pogod[i].GetAttributeValue("data-text", "");

                if (max_temp[i].InnerText.Contains("&minus;"))
                    Max_temp[i].Content = max_temp[i].InnerText.Replace("&minus;", "-");
                else
                    Max_temp[i].Content = max_temp[i].InnerText;

                if (min_temp[i].InnerText.Contains("&minus;"))
                    Min_temp[i].Content = min_temp[i].InnerText.Replace("&minus;", "-");
                else
                    Min_temp[i].Content = min_temp[i].InnerText;

            }



        }

       

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
