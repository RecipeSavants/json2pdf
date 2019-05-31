using System.Collections.Generic;

namespace RecipeCardLibrary.DataModel
{
    public class List
    {
        public List<string> Produce { get; set; } = new List<string>();

        public List<string> Meat { get; set; } = new List<string>();

        public List<string> Dairy { get; set; } = new List<string>();

        public List<string> Baking { get; set; } = new List<string>();

        public List<string> Canned { get; set; } = new List<string>();

        public List<string> Beverages { get; set; } = new List<string>();

        public List<string> Herbs { get; set; } = new List<string>();

        public List<string> Pantry { get; set; } = new List<string>();
    }
}
