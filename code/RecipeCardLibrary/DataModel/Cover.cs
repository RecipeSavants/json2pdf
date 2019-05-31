using System.Collections.Generic;

namespace RecipeCardLibrary.DataModel
{
    public class Cover
    {
        public string PhotoshopPSD { get; set; }

        public string MealTips { get; set; }

        public string WeekOf { get; set; }

        public string Title { get; set; }

        public string Image { get; set; }

        public Dictionary<string, List<string>> Meals { get; set; } = new Dictionary<string, List<string>>();

        public string MainTitle { get; set; }
    }
}
