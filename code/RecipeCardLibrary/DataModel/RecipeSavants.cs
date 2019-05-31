using System.Collections.Generic;

namespace RecipeCardLibrary.DataModel
{
    public class RecipeSavants
    {
        public string UserName { get; set; }

        public string TagLine { get; set; }

        public Configuration Cfg { get; set; }

        public Cover Cover { get; set; }

        public Tips Tips { get; set; }

        public List<Recipe> Recipes { get; set; } = new List<Recipe>();

        public ShoppingList ShoppingList { get; set; }
    }
}