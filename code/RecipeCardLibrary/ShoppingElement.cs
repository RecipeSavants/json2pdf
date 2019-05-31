using ps = Photoshop;

namespace RecipeCardLibrary
{
    public enum Column
    {
        Left,
        Center,
        Right
    }

    public class ShoppingElement
    {
        public ps.ArtLayer TitleLayer { get; set; }

        public ps.ArtLayer TextLayer { get; set; }

        public double HeightText { get; set; }

        public double HeightTitle { get; set; }

        public double WidthText { get; set; }

        public double WidthTitle { get; set; }

        public Column Column { get; set; }
    }
}
