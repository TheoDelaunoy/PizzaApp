using System;
using System.ComponentModel;
using System.Windows.Input;
using PizzaApp.extensions;
using Xamarin.Forms;

namespace PizzaApp.Models
{
	public class PizzaCell: INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public Pizza pizza { get; set; }

		public bool isFavorite { get; set; }

		public string ImageSourceFav { get { return isFavorite ? "star2.png" : "star1.png"; } }

		public ICommand FavClickCommand { get; set; }

		public Action<PizzaCell> favChangedAction { get; set; }

        public PizzaCell()
		{
			FavClickCommand = new Command((obj) =>
			{
				isFavorite = !isFavorite;
				OnPropertyChanged("ImageSourceFav");
				favChangedAction.Invoke(this);
			});
		}

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}
    }
}

