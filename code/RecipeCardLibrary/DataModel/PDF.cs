using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeCardLibrary.DataModel
{
    public class PDF
    {
        public string Type { get; set; }
        public string Cover { get; set; }
        public string Tips { get; set; }
        public List<string> Recipes { get; set; }
        public string ShoppingList { get; set; }

        public PDF()
        {
            Recipes = new List<string>();
        }
    }
}
