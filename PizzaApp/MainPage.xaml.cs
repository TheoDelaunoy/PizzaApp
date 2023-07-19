using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using PizzaApp.Models;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace PizzaApp
{

    public partial class MainPage : ContentPage
    {
        List<Pizza> pizzas = new List<Pizza>();
        Pizza pizza = new Pizza();

        List<string> pizzasFav = new List<string>();

        enum e_tri
        {
            TRI_AUCUN,
            TRI_NOM,
            TRI_PRIX,
            TRI_FAV
        }

        string tmpFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp");
        string jsonFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pizzas.json");

        const string KEY_TRI = "tri";
        const string KEY_FAV = "fav";

        e_tri tri = e_tri.TRI_AUCUN;

        public MainPage()
        {
            InitializeComponent();

            LoadFavList();

            if (Application.Current.Properties.ContainsKey(KEY_TRI))
            {
                tri = (e_tri)Application.Current.Properties[KEY_TRI];
                sortButton.Source = GetImageSourceFromTri(tri);
            }

            myListView.RefreshCommand = new Command((obj) =>
            {
                DownloadData((pizzas) =>
                {
                    myListView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);
                    myListView.IsRefreshing = false;
                });
            });

            myListView.IsVisible = false;
            WaitActivity.IsVisible = true;

            if (File.Exists(jsonFileName))
            {
                string pizzasJson = File.ReadAllText(jsonFileName);
                if (!String.IsNullOrEmpty(pizzasJson)){
                    pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzasJson);
                    myListView.ItemsSource = GetPizzasFromTri(tri, pizzas);
                    myListView.IsVisible = true;
                    WaitActivity.IsVisible = false;
                }
                
            }


            DownloadData((pizzas) =>
            {
                if(pizzas != null)
                {
                    myListView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);
                    WaitActivity.IsVisible = false;
                    myListView.IsVisible = true;
                }
            });
        }

        private void DownloadData(Action<List<Pizza>> action)
        {
            const string URL = "https://drive.google.com/uc?export=download&id=1a5KEoDs6GzHIhcafDkwb2NUlE2JumiJB";

            using (var webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                {
                    Exception ex = e.Error;

                    if (ex == null)
                    {
                        File.Copy(tmpFileName, jsonFileName, true);

                        string pizzasJson = File.ReadAllText(jsonFileName);
                        pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzasJson);
                        Device.BeginInvokeOnMainThread(() => {
                            action.Invoke(pizzas);
                        });
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(async () =>{
                            await DisplayAlert("Erreur", "une erreur réseau s'est produite", ex.Message, "OK");
                            action.Invoke(null);
                        });
                    }
                };

                webClient.DownloadFileAsync(new Uri(URL), tmpFileName);
            }

        }

        private string GetImageSourceFromTri(e_tri t)
        {
            switch (t)
            {
                case e_tri.TRI_NOM:
                    return "sort_nom.png";
                case e_tri.TRI_PRIX:
                    return "sort_prix.png";
                case e_tri.TRI_AUCUN:
                    return "sort_none.png";
                case e_tri.TRI_FAV:
                    return "sort_fav.png";
            }
            return "sort_none.png";
        }

        private List<Pizza> GetPizzasFromTri(e_tri t, List<Pizza> l)
        {

            if (l == null)
            {
                return null;
            }
            else if (t == e_tri.TRI_PRIX)
            {
                List<Pizza> copie = new List<Pizza>(l);
                copie.Sort((p1, p2) => { return p1.prix.CompareTo(p2.prix); });
                return copie;
            }
            else if (t == e_tri.TRI_NOM)
            {
                List<Pizza> copie = new List<Pizza>(l);
                copie.Sort((p1, p2) => { return p1.Titre.CompareTo(p2.Titre); });
                return copie;
            }
            else if( t == e_tri.TRI_FAV)
            {
                List<Pizza> copie = new List<Pizza>(l);
                copie.Sort((p1, p2) => { return p1.Titre.CompareTo(p2.Titre); });
                return copie;
            }
            else
            {
                return l;
            }
        }

        private void OnFavPizzaChanged(PizzaCell pizzaCell)
        {
            bool isInFavList = pizzasFav.Contains(pizzaCell.pizza.nom);

            if(pizzaCell.isFavorite && !isInFavList)
            {
                pizzasFav.Add(pizzaCell.pizza.nom);
                SaveFavList();
            }
            else if (!pizzaCell.isFavorite && isInFavList)
            {
                pizzasFav.Remove(pizzaCell.pizza.nom);
                SaveFavList();
            }

        }

        private List<PizzaCell> GetPizzaCells(List<Pizza> p, List<string> f)
        {
            List<PizzaCell> ret = new List<PizzaCell>();
            if(p == null)
            {
                return ret;
            }
            foreach (Pizza pizza in p)
            {
                bool isFav = f.Contains(pizza.nom);

                if (tri == e_tri.TRI_FAV)
                {
                    if(isFav == true)
                    {
                        ret.Add(new PizzaCell { pizza = pizza, isFavorite = isFav, favChangedAction = OnFavPizzaChanged });
                    }
                }
                else
                {
                    ret.Add(new PizzaCell { pizza = pizza, isFavorite = isFav, favChangedAction = OnFavPizzaChanged });
                }
                
            }
            return ret;
        }

        void SortButton_Clicked(System.Object sender, System.EventArgs e)
        {
            if (tri == e_tri.TRI_AUCUN)
            {
                tri = e_tri.TRI_NOM;
            }
            else if (tri == e_tri.TRI_NOM)
            {
                tri = e_tri.TRI_PRIX;
            }
            else if (tri == e_tri.TRI_PRIX)
            {
                tri = e_tri.TRI_FAV;
            }
            else if (tri == e_tri.TRI_FAV)
            {
                tri = e_tri.TRI_AUCUN;
            }

            DownloadData((pizzas) =>
            {
                myListView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas), pizzasFav);
                myListView.IsRefreshing = false;
            });

            sortButton.Source = GetImageSourceFromTri(tri);
            myListView.ItemsSource = GetPizzasFromTri(tri, pizzas);

            Application.Current.Properties[KEY_TRI] = (int)tri;
            Application.Current.SavePropertiesAsync();
        }

        private void SaveFavList()
        {
            string favListStr = JsonConvert.SerializeObject(pizzasFav);
            Application.Current.Properties[KEY_FAV] = favListStr;
            Application.Current.SavePropertiesAsync();
        }

        private void LoadFavList()
        {
            if (Application.Current.Properties.ContainsKey(KEY_FAV))
            {
                string favListStr = Application.Current.Properties[KEY_FAV].ToString();
                pizzasFav = JsonConvert.DeserializeObject<List<string>>(favListStr);
            }
            
        }

    }
}

