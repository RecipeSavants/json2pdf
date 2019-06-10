using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RecipeCardLibrary.DataModel
{
    public enum SocialType
    {
        Passport,
        Complete,
        Normal,
        PassportNoSocial,
        SocialNoRecipecardNormal,
        SocialRecipecardComplete,
        No
    }

    public class Configuration
    {
        public bool OutputASPSD { get; set; }

        public bool OutputASJpeg { get; set; }

        public int Quality { get; set; }

        public string CoverFile { get; set; }

        public string TipsFile { get; set; }

        public string RecipeFile { get; set; }

        public string ShoppingFile { get; set; }

        public string InstagramFile { get; set; }

        public string PinterestFile { get; set; }

        public string PTMealsFile { get; set; }

        public string PTPassportFile { get; set; }

        public string PSDInput { get; set; }

        public string RecipeCardsOutput { get; set; }

        public string SocialCardsOutput { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SocialType SocialType { get; set; }

        public bool ProcessPDF { get; set; }
    }
}
