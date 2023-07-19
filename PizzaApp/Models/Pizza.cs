using System;
using PizzaApp.extensions;
namespace PizzaApp.Models
{
	public class Pizza
	{
		public string nom;

		public int prix;

		public string[] ingredients;

        public string imageUrl { get; set; }


        public Pizza()
		{
		}

        public string Titre { get { return nom.Formattage(); } }

        public string PrixEuros { get { return this.prix + "€"; } }

        public string IngredientsStr { get { return String.Join(", ", ingredients); } }
    }
}

