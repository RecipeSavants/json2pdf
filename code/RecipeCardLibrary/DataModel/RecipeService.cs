using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecipeCardLibrary.DataModel
{
    public class RecipeService
    {
        [JsonProperty("id")]
        public string RecipeID { get; set;}

        [JsonProperty("recipeName")]
        public string MealName { get; set; }

        [JsonProperty("synopsis")]
        public string Description { get; set; }

        [JsonProperty("ingredients")]
        public List<Ingredients> Ingredients { get; set; } = new List<Ingredients>();

        [JsonProperty("steps")]
        public List<Step> Steps { get; set; } = new List<Step>();

        [JsonProperty("uri")]
        public List<string> Image { get; set; } = new List<string>();

        [JsonProperty("prepNotes")]
        public string Tip { get; set; }

        public string TotalActiveCookTime { get; set; }

        public string Cusine { get; set; }

        public string Yield { get; set; }

        public string MealType { get; set; }        

        public List<string> Garnish { get; set; } = new List<string>();

        public string GetImage()
        {
            if(Image.Count > 0 )
            {
                return Image[0].Replace("_m.jpg", "_facebook.jpg");
            }
            return string.Empty;
        }

        public List<string> GetIngredients()
        {
            List<string> ingredients = new List<string>();
            
            foreach (Ingredients i in Ingredients.OrderBy(i => i.SortOrder))
            {
                string line = i.ToString();
                ingredients.Add(line);
            }

            return ingredients;
        }

        public List<string> GetSteps()
        {
            List<string> steps = new List<string>();

            foreach (Step i in Steps.OrderBy(i => i.SortOrder))
            {
                steps.Add(i.step);
            }

            return steps;
        }
    }

    public class Ingredients
    {
        public int SortOrder { get; set;}

        public string Quantity { get; set; }

        public string QuantityUnits { get; set; }

        public string Term { get; set; }

        public string PrepNotes { get; set; }

        public string QuantityMetric { get; set; }

        public string QuantityMetricUnits { get; set; }

        public override string ToString()
        {
            StringBuilder ingredients = new StringBuilder();
         
            ingredients.Append(Fractions.ToFractions(Double.Parse(Quantity), 4));

            if (!string.IsNullOrEmpty(QuantityUnits) && QuantityUnits.ToUpper() != "N/A")
            {
                ingredients.AppendFormat(" {0}", QuantityUnits);
            }
            if(Term.Contains("Salt &"))
            {
                string newLine = string.Empty;
                if(!string.IsNullOrEmpty(PrepNotes))
                {
                    newLine = string.Format(" ({0})\r{1} ({0})", PrepNotes, ingredients.ToString());
                }
                else
                {
                    newLine = string.Format("\r{0}", ingredients.ToString());
                }
                ingredients.Append(string.Format(" {0}", Term.Replace(" &", newLine)));

            }
            else if(Term.Contains("Salt and"))
            {
                string newLine = string.Empty;
                if (!string.IsNullOrEmpty(PrepNotes))
                {
                    newLine = string.Format(" ({0})\r{1} ({0})", PrepNotes, ingredients.ToString());
                }
                else
                {
                    newLine = string.Format("\r{0}", ingredients.ToString());
                }
                ingredients.Append(string.Format(" {0}", Term.Replace(" and", newLine)));
            }
            else
            {
                ingredients.AppendFormat(" {0}", Term);
                if(!string.IsNullOrEmpty(PrepNotes))
                {
                    ingredients.AppendFormat(" ({0})", PrepNotes);
                }
            }

            return ingredients.ToString();
        }

        private string FormatQuantity(double remainder)
        {
            int remainder1 = (int)(remainder * 1000);
            int remainder2 = (int)(remainder * 100);
            int gcd1 = gcd(remainder1, 1000);
            int gcd2 = gcd(remainder1, 99);
            if (gcd1 > gcd2)
            {
                int x1 = remainder1 / gcd1;
                int x2 = 1000 / gcd1;
                return $"{x1}/{x2}";
            }
            else
            {
                int x1 = remainder2 / gcd2;
                int x2 = 99 / gcd2;
                return $"{x1}/{x2}";
            }
        }

        private int gcd(int a, int b)
        {
            if (b == 0)
                return a;
            return gcd(b, a % b);
        }
    }

    public class Step
    {
        public int SortOrder { get; set; }

        public string step { get; set; }
    }
}
