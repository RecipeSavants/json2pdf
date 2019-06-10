using System.Collections.Generic;

namespace RecipeCardLibrary.DataModel
{
    public class Recipe
    {
        public string Image { get; set; }

        public string PhotoshopPSD { get; set; }

        public List<string> Steps { get; set; } = new List<string>();

        public string Description { get; set; }

        public string MealType { get; set; }

        public string MealName { get; set; }

        public string TotalActiveCookTime { get; set; }

        public string Cusine { get; set; }

        public string Yield { get; set; }

        public List<string> Ingredients { get; set; } = new List<string>();

        public List<string> Garnish { get; set; } = new List<string>();

        public string Tip { get; set; }

        public string RecipeID { get; set;}

        public bool IsModified { get; set; }
    }
}