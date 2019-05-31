using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CreatePdfLibrary
{
    public class RecipeSavantsPDF
    {
        public string Type { get; set; }

        public string Cover { get; set; }

        public string Tips { get; set; }

        public List<string> Recipes { get; set; } = new List<string>();

        public string ShoppingList { get; set; }
    }
}