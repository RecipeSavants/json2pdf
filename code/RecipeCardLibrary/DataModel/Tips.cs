using System.Collections.Generic;

namespace RecipeCardLibrary.DataModel
{
    public class Tips
    {
        public string Image { get; set; }

        public Dictionary<string, string> Tip { get; set; } = new Dictionary<string, string>();

        public string PhotoshopPSD { get; set;}
    }
}
