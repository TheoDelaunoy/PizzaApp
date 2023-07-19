using System;
namespace PizzaApp.extensions
{
	public static class StringExtensions
	{ 
		public static string Formattage(this string str)
		{
			if (String.IsNullOrEmpty(str))
			{
				return str;
			}

			string formattedStr;
			formattedStr = str.ToLower();
			formattedStr = formattedStr.Substring(0, 1).ToUpper() + formattedStr.Substring(1, formattedStr.Length-1);
			return formattedStr;
        }
	}
}

