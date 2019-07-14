using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using RecipeCardLibrary.DataModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ps = Photoshop;
using Kraken; using Kraken.Http;
using System.Net.Http;
using System.Drawing;
using ImageMagick;
using System.Drawing.Imaging;
using System.Globalization;
using CreatePdfLibrary;

namespace RecipeCardLibrary
{
    public class RecipeSavantsController
    {
        private DataModel.Configuration cfg;

        private DataModel.Configuration cfgDefault;

        private List<RecipeSavants> recipeSavantsList = new List<RecipeSavants>();

        private static string weekOfFileName = string.Empty;

        private static double lenSpaceWeekOfToTitle = 44; //template Cover    

        private CloudStorageAccount storageAccount;

        private CloudBlobClient blobClient;

        public EventHandler<MessageEventArgs> Message;

        #region Log

        public void LogError(string message, params object[] args)
        {
            string erroMsg = FormatMessage(message, args);
            //MainForm.Instance.Log(erroMsg, true);
            //MessageBox.Show(erroMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            OnThresholdReached(new MessageEventArgs { Msg = erroMsg, IsError = true});
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            var sb = new StringBuilder(FormatMessage(message, args));
            sb.AppendLine(ex.ToString());

            //MainForm.Instance.Log(sb.ToString(), true);
            //MessageBox.Show(sb.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            OnThresholdReached(new MessageEventArgs { Msg = sb.ToString(), IsError = true });
        }

        public void LogInformation(string message, params object[] args)
        {
            //MainForm.Instance.Log(FormatMessage(message, args));
            OnThresholdReached(new MessageEventArgs { Msg = FormatMessage(message, args), EndLine = true });
        }

        public void LogInformationBegin(string message, params object[] args)
        {
            //MainForm.Instance.Log(FormatMessage(message, args), false, false);
            OnThresholdReached(new MessageEventArgs { Msg = FormatMessage(message, args), IsError = false, EndLine = false });
        }

        private static string FormatMessage(string message, params object[] args)
        {
            args = args == null ? null : args.Where(a => a != null).ToArray();
            return string.Format(message, args);
        }

        protected virtual void OnThresholdReached(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = Message;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion //Log

        public void Run(string jsonFile, string id, string cfgFile)
        {
            List<PDF> ss = new List<PDF>();
            LogInformationBegin("Launch Photoshop...");
            var app = Activator.CreateInstance(Type.GetTypeFromProgID("Photoshop.Application")) as ps.Application;
            try
            {
                LogInformation("[{0}] - Start", DateTime.Now);

                string storageConnectionString = ConfigurationManager.AppSettings["storageconnectionstring"];
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                blobClient = storageAccount.CreateCloudBlobClient();

                if (!CheckParameters(jsonFile, id, cfgFile))
                {
                    return;
                }


                app.DisplayDialogs = ps.PsDialogModes.psDisplayNoDialogs;
                LogInformation("OK");

                foreach (RecipeSavants recipeSavants in recipeSavantsList)
                {
                    var s = new PDF();
                    try
                    {
                        s.Type = recipeSavants.UserName ?? "General";
                        if(string.IsNullOrEmpty(s.Type))
                        {
                            s.Type = "General";
                        }
                        cfg = recipeSavants.Cfg;

                        if (recipeSavants.Cover != null)
                        {
                            s.Cover = (GenerateCover(app, recipeSavants.Cover, recipeSavants.UserName, recipeSavants.TagLine));

                            if (recipeSavants.Tips != null)
                            {
                                s.Tips = (GenerateTips(app, recipeSavants.Tips, recipeSavants.UserName, recipeSavants.TagLine));
                            }
                            if (recipeSavants.ShoppingList != null)
                            {
                                s.ShoppingList = (GenerateShopping(app, recipeSavants.ShoppingList, recipeSavants.UserName, recipeSavants.TagLine));
                            }
                        }
                        foreach (Recipe recipe in recipeSavants.Recipes)
                        {
                            if (cfg.SocialType != SocialType.SocialNoRecipecardNormal && cfg.SocialType != SocialType.SocialRecipecardComplete)
                                s.Recipes.Add(GenerateRecipe(app, recipe, recipeSavants.UserName, recipeSavants.TagLine, cfg.SocialType.ToString()));

                            if (cfg.SocialType != SocialType.No && cfg.SocialType != SocialType.PassportNoSocial)
                            {
                                GeneratePinterest(app, recipe, recipeSavants.UserName, "");
                                GenerateInstagram(app, recipe, recipeSavants.UserName, "");
                            }
                            try
                            {
                                File.Delete(recipe.Image);
                            }
                            catch
                            { }
                        }
                        ss.Add(s);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "The process ended with an error.");
                    }
                }
                //process the pdf
                if (cfg.ProcessPDF)
                {
                    CreatePDFController pdf = new CreatePDFController();
                    pdf.Run(JsonConvert.SerializeObject(ss), cfg.RecipeCardsOutput);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "The process ended with an error.");
            }
            finally
            {
                app.Application.Quit();
                LogInformation("[{0}] - End", DateTime.Now);
            }
        }

        private bool CheckParameters(string jsonFile, string id, string cfgFile)
        {
            if (string.IsNullOrEmpty(jsonFile) & string.IsNullOrEmpty(id))
            {
                LogError("ID empty and JSON files is not specified.");
                return false;
            }

            if (!string.IsNullOrEmpty(jsonFile))
            {
                if (!File.Exists(jsonFile))
                {
                    LogError("JSON file doesn't exist ({0}).", jsonFile);
                    return false;
                }
                if(!LoadRecipeSavantsList(jsonFile))
                {
                    return false;
                }
            }

            if (string.IsNullOrEmpty(cfgFile))
            {
                LogError("Cfg file is not specified.");
                return false;
            }
            if (!string.IsNullOrEmpty(cfgFile))
            {
                if (!File.Exists(cfgFile))
                {
                    LogError("Cfg file doesn't exist ({0}).", cfgFile);
                    return false;
                }
                if (!LoadCfg(cfgFile))
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(cfgDefault.PSDInput) && !Directory.Exists(cfgDefault.PSDInput))
                {
                    LogError("Input folder doesn't exist ({0}).", cfgDefault.PSDInput);
                    return false;
                }
                if (!string.IsNullOrEmpty(cfgDefault.SocialCardsOutput) && !Directory.Exists(cfgDefault.SocialCardsOutput))
                {
                    LogError("Social output folder doesn't exist ({0}).", cfgDefault.SocialCardsOutput);
                    return false;
                }
                if (!string.IsNullOrEmpty(cfgDefault.RecipeCardsOutput) && !Directory.Exists(cfgDefault.RecipeCardsOutput))
                {
                    LogError("Recipe output folder doesn't exist ({0}).", cfgDefault.RecipeCardsOutput);
                    return false;
                }
                string newDir = DateTime.Now.Date.ToString("dd-MM-yyyy");
                string newSocialDir = Path.Combine(cfgDefault.SocialCardsOutput, newDir);
                string newRecipeDir = Path.Combine(cfgDefault.RecipeCardsOutput, newDir);
                if (!Directory.Exists(newSocialDir))
                {
                    Directory.CreateDirectory(newSocialDir);
                }
                if (!Directory.Exists(newRecipeDir))
                {
                    Directory.CreateDirectory(newRecipeDir);
                }
                cfgDefault.SocialCardsOutput = newSocialDir;
                cfgDefault.RecipeCardsOutput = newRecipeDir;
            }

            if (!string.IsNullOrEmpty(id))
            {
                List<RecipeService> recipeServiceList = GetRecipeServiceList(id);
                foreach (RecipeService recipeService in recipeServiceList)
                {
                    RecipeSavants rs = new RecipeSavants()
                    {
                        Recipes = new List<Recipe>
                        {
                            new Recipe()
                            {
                                RecipeID = recipeService.RecipeID,
                                Description = recipeService.Description,
                                Image = recipeService.GetImage(),
                                Ingredients = recipeService.GetIngredients(),
                                MealName = recipeService.MealName,
                                Steps = recipeService.GetSteps(),
                                Tip = recipeService.Tip,
                                TotalActiveCookTime = recipeService.TotalActiveCookTime,
                                Cusine = recipeService.Cusine,
                                Yield = recipeService.Yield
                            }
                        }
                    };

                    recipeSavantsList.Add(rs);
                }
            }

            foreach (var recipeSavants in recipeSavantsList)
            {
                
                if (recipeSavants.Cfg == null)
                {
                    recipeSavants.Cfg = cfgDefault;
                    cfg = recipeSavants.Cfg;
                }
                else
                {
                    cfg = recipeSavants.Cfg;
                    if (string.IsNullOrEmpty(cfg.RecipeCardsOutput))
                    {
                        cfg.RecipeCardsOutput = cfgDefault.RecipeCardsOutput;
                    }
                    else
                    {
                        if (!Directory.Exists(cfg.RecipeCardsOutput))
                        {
                            LogError("Recipe output folder doesn't exist ({0}).", cfg.RecipeCardsOutput);
                            return false;
                        }
                    }
                    if (string.IsNullOrEmpty(cfg.SocialCardsOutput))
                    {
                        cfg.SocialCardsOutput = cfgDefault.SocialCardsOutput;
                    }
                    else
                    {
                        if (!Directory.Exists(cfg.SocialCardsOutput))
                        {
                            LogError("Social output folder doesn't exist ({0}).", cfg.SocialCardsOutput);
                            return false;
                        }
                    }
                    if (string.IsNullOrEmpty(cfg.PSDInput))
                    {
                        cfg.PSDInput = cfgDefault.PSDInput;
                    }
                    else
                    {
                        if (!Directory.Exists(cfg.PSDInput))
                        {
                            LogError("Social output folder doesn't exist ({0}).", cfg.PSDInput);
                            return false;
                        }
                    }
                }

                #region Check Cover

                if (recipeSavants.Cover != null)
                {
                    if (string.IsNullOrEmpty(recipeSavants.Cover.PhotoshopPSD))
                    {
                        if (string.IsNullOrWhiteSpace(cfg.CoverFile))
                        {
                            LogError("Cover card file is not specified.");
                            return false;
                        }
                        if (!File.Exists(Path.Combine(cfg.PSDInput, cfg.CoverFile)))
                        {
                            LogError("Cover card file doesn't exist ({0}).", cfg.CoverFile);
                            return false;
                        }
                    }
                    else
                    {                        
                        if (!ExistBlob("psdinput", recipeSavants.Cover.PhotoshopPSD))
                        {
                            LogError("Cover PhotoshopPSD file doesn't exist ({0}).", recipeSavants.Cover.PhotoshopPSD);
                            return false;
                        }
                    }

                    #region Check Tips

                    if (recipeSavants.Tips != null)
                    {
                        if (string.IsNullOrEmpty(recipeSavants.Tips.PhotoshopPSD))
                        {
                            if (string.IsNullOrWhiteSpace(cfg.TipsFile))
                            {
                                LogError("Tips card file is not specified.");
                                return false;
                            }
                            if (!File.Exists(Path.Combine(cfg.PSDInput, cfg.TipsFile)))
                            {
                                LogError("Tips card file doesn't exist ({0}).", cfg.TipsFile);
                                return false;
                            }
                        }
                        else
                        {
                            if (!ExistBlob("psdinput", recipeSavants.Tips.PhotoshopPSD))
                            {
                                LogError("Tips PhotoshopPSD file doesn't exist ({0}).", recipeSavants.Tips.PhotoshopPSD);
                                return false;
                            }
                        }
                    }

                    #endregion

                    #region ShoppingList

                    if (recipeSavants.ShoppingList != null)
                    {
                        if (string.IsNullOrWhiteSpace(cfg.ShoppingFile))
                        {
                            LogError("Shopping card file is not specified.");
                            return false;
                        }
                        if (!File.Exists(Path.Combine(cfg.PSDInput, cfg.ShoppingFile)))
                        {
                            LogError("Shopping card file doesn't exist ({0}).", cfg.ShoppingFile);
                            return false;
                        }
                    }

                    #endregion

                }

                #endregion

                #region Check Recipes

                if (recipeSavants.Recipes != null)
                {
                    if (cfg.SocialType != SocialType.SocialNoRecipecardNormal && cfg.SocialType != SocialType.SocialRecipecardComplete)
                    {
                        foreach (var recipe in recipeSavants.Recipes)
                        {
                            if (string.IsNullOrEmpty(recipe.PhotoshopPSD))
                            {
                                if (string.IsNullOrWhiteSpace(cfg.RecipeFile))
                                {
                                    LogError("Recipe card {0} file is not specified.", recipe.RecipeID);
                                    return false;
                                }
                                if (!File.Exists(Path.Combine(cfg.PSDInput, cfg.RecipeFile)))
                                {
                                    LogError("Recipe card {0} file doesn't exist ({1}).", recipe.RecipeID, cfg.RecipeFile);
                                    return false;
                                }
                            }
                            else
                            {
                                if (!ExistBlob("psdinput", recipe.PhotoshopPSD))
                                {
                                    LogError("Recipe {0} PhotoshopPSD file doesn't exist ({1}).", recipe.RecipeID, recipe.PhotoshopPSD);
                                    return false;
                                }
                            }
                        }
                    }

                    #region Instagram
                    if (cfg.SocialType != SocialType.PassportNoSocial)
                    {
                        if (string.IsNullOrWhiteSpace(cfg.InstagramFile))
                        {
                            LogError("Instagram card file is not specified.");
                            return false;
                        }
                        if (!File.Exists(Path.Combine(cfg.PSDInput, cfg.InstagramFile)))
                        {
                            LogError("Instagram card file doesn't exist ({0}).", cfg.InstagramFile);
                            return false;
                        }
                    }

                    #endregion

                    #region Pinterest
                    if (cfg.SocialType == SocialType.Normal || cfg.SocialType == SocialType.SocialNoRecipecardNormal || cfg.SocialType == SocialType.SocialRecipecardComplete)
                    {
                        if (string.IsNullOrWhiteSpace(cfg.PinterestFile))
                        {
                            LogError("Pinterest card file is not specified.");
                            return false;
                        }
                        if (!File.Exists(Path.Combine(cfg.PSDInput, cfg.PinterestFile)))
                        {
                            LogError("Pinterest card file doesn't exist ({0}).", cfg.PinterestFile);
                            return false;
                        }
                    }
                    #endregion

                    #region CompleteMeal
                    if (cfg.SocialType == SocialType.Complete || cfg.SocialType == SocialType.SocialRecipecardComplete)
                    {
                        if (string.IsNullOrWhiteSpace(cfg.PTMealsFile))
                        {
                            LogError("CompleteMeal card file is not specified.");
                            return false;
                        }
                        if (!File.Exists(Path.Combine(cfg.PSDInput, cfg.PTMealsFile)))
                        {
                            LogError("CompleteMeal card file doesn't exist ({0}).", cfg.PTMealsFile);
                            return false;
                        }
                    }

                    #endregion

                    #region PassportMeal
                    if (cfg.SocialType == SocialType.Passport || cfg.SocialType == SocialType.SocialRecipecardComplete)
                    {
                        if (string.IsNullOrWhiteSpace(cfg.PTPassportFile))
                        {
                            LogError("PassportMeal card file is not specified.");
                            return false;
                        }
                        if (!File.Exists(Path.Combine(cfg.PSDInput, cfg.PTPassportFile)))
                        {
                            LogError("PassportMeal card file doesn't exist ({0}).", cfg.PTPassportFile);
                            return false;
                        }
                    }

                    #endregion
                }

                #endregion
            }
            return true;
        }

        #region Cover

        private string GenerateCover(ps.Application app, Cover cover, string userName, string tagLine)
        {
            bool photoshopPSD = !string.IsNullOrEmpty(cover.PhotoshopPSD);
            string psdFile = string.Empty;
            if (photoshopPSD)
            {
                psdFile = AzureDownloadToFile(cover.PhotoshopPSD, "psdinput");
                if(!string.IsNullOrEmpty(psdFile))
                {
                    app.Load(psdFile);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                string coverFile = Path.Combine(cfg.PSDInput, cfg.CoverFile);
                app.Load(coverFile);
            }
            var doc = app.ActiveDocument;

            var headerLayerSet = doc.LayerSets.Cast<ps.LayerSet>().FirstOrDefault(l => l.Name == "Header");
            if (string.IsNullOrEmpty(cover.WeekOf))
            {
                weekOfFileName = headerLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("WeekOf")).TextItem.Contents.Replace("\r", "");
            }
            else
            {
                weekOfFileName = cover.WeekOf;
            }

            string fileName = string.IsNullOrEmpty(userName) ? $"{weekOfFileName}-Cover" : $"{userName}-{weekOfFileName}-Cover";

            LogInformationBegin($"File \"{fileName}.psd\"...");

            if (string.IsNullOrEmpty(cover.PhotoshopPSD))
            {
                if (!string.IsNullOrEmpty(cover.MainTitle) || !string.IsNullOrEmpty(cover.WeekOf) || !string.IsNullOrEmpty(cover.Title))
                {
                    UpdateCoverHeader(headerLayerSet, cover);
                }
                if (!string.IsNullOrEmpty(cover.Image))
                {
                    string imageName = UpdateImage(cover.Image, "Cover", doc);
                    File.Delete(imageName);
                }
                if (!string.IsNullOrEmpty(tagLine))
                {
                    var photoLayer = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Image");
                    double offset = photoLayer.Bounds[2] - photoLayer.Bounds[1];
                    UpdateTagLine(doc, tagLine, offset);
                }
            }

            if (cover.Meals.Count > 0)
            {
                UpdateCoverMeals(doc, cover);
            }
            if (!string.IsNullOrEmpty(cover.MealTips))
            {
                UpdateCoverMealTips(doc, cover);
            }

            Save(doc, fileName, photoshopPSD, psdFile);

            LogInformation("OK");
            doc.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            return fileName;
        }

        private static void UpdateCoverHeader(ps.LayerSet header, Cover cover)
        {
            bool left = false;
            bool setSpace = false;

            var mainTitleLayer = header.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("MainTitle"));
            var weekOfLayer = header.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("WeekOf"));
            var titleLayer = header.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("Title"));

            if (!string.IsNullOrEmpty(cover.MainTitle))
            {
                left = true;
                mainTitleLayer.TextItem.Contents = cover.MainTitle;
            }
            if (!string.IsNullOrEmpty(cover.WeekOf))
            {
                left = true;
                setSpace = true;
                FideLengthSpace(weekOfLayer, titleLayer);
                weekOfLayer.TextItem.Contents = cover.WeekOf;
            }
            if (!string.IsNullOrEmpty(cover.Title))
            {
                if (!setSpace)
                {
                    FideLengthSpace(weekOfLayer, titleLayer);
                    setSpace = true;
                }
                titleLayer.TextItem.Contents = cover.Title;
            }
            if (left)
            {
                LefMainTitleAndWeekOf(mainTitleLayer, weekOfLayer);
            }
            if (setSpace)
            {
                SetSpaceWeekOfToTitle(weekOfLayer, titleLayer);
            }
        }

        private static void FideLengthSpace(ps.ArtLayer weekOfLayer, ps.ArtLayer titleLayer)
        {
            double xWeekOfLayer = weekOfLayer.Bounds[2];
            double xTitleLayer = titleLayer.Bounds[0];
            lenSpaceWeekOfToTitle = xTitleLayer - xWeekOfLayer;
        }

        private static void LefMainTitleAndWeekOf(ps.ArtLayer mainTitleLayer, ps.ArtLayer weekOfLayer)
        {
            double x = mainTitleLayer.TextItem.Position[0];
            double y = weekOfLayer.TextItem.Position[1];
            weekOfLayer.TextItem.Position = new object[] { x, y };
        }

        private static void SetSpaceWeekOfToTitle(ps.ArtLayer weekOfLayer, ps.ArtLayer titleLayer)
        {
            titleLayer.TextItem.Position = new object[] { weekOfLayer.Bounds[2] + lenSpaceWeekOfToTitle, titleLayer.TextItem.Position[1] };
        }

        private void UpdateCoverMeals(ps.Document doc, Cover cover)
        {
            int mealToText = 35; // px
            int textToMeal = 50; // px

            double posY = 0;
            double posXHeader = 0;
            double posXText = 0;
            double heightHeader = 0;
            double heightText = 0;

            var mealsLayer = doc.LayerSets.Cast<ps.LayerSet>().FirstOrDefault(l => l.Name == "Meals");
            var previousElement = mealsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "1");

            for (int i = 1; i < 5; i++)
            {
                string mealName = string.Empty;
                if ((cfg.SocialType == SocialType.Complete || cfg.SocialType == SocialType.SocialRecipecardComplete) && i == 1)
                {
                    mealName = "Meal Highlights";
                }
                else
                {
                    mealName = string.Format("Meal{0}", i);
                }
                //string mealName = string.Format("Meal{0}", i);
                if (i <= cover.Meals.Count)
                {
                    var mealText = mealsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == mealName);
                    if(mealText == null) continue;
                    if (i == 1)
                    {
                        posXHeader = previousElement.TextItem.Position[0];
                        posY = previousElement.TextItem.Position[1];
                        posXText = mealText.TextItem.Position[0];
                        heightHeader = previousElement.Bounds[3] - previousElement.Bounds[1];
                    }

                    List<string> textList;
                    cover.Meals.TryGetValue(i.ToString(), out textList);
                    mealText.TextItem.Contents = string.Join("\r", textList.ToArray());
                    posY += mealToText;
                    mealText.TextItem.Position = new object[] { posXText, posY };

                    heightText = mealText.Bounds[3] - mealText.Bounds[1]; // posY += heightText * textList.Count + textToMeal + heightHeader;

                    if (i < cover.Meals.Count)
                    {
                        posY += heightText + textToMeal + heightHeader;
                        previousElement = mealsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == (i + 1).ToString());
                        previousElement.TextItem.Position = new object[] { posXHeader, posY };
                    }
                }
                else //visible = false
                {
                    var header = mealsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == i.ToString());
                    var mealText = mealsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == mealName);
                    header.Visible = false;
                    mealText.Visible = false;
                }
            }
        }

        private static void UpdateCoverMealTips(ps.Document doc, Cover cover)
        {
            int mealToText = 35; // px
            int textToMeal = 50; // px

            var tipsLayerSet = doc.LayerSets.Cast<ps.LayerSet>().FirstOrDefault(l => l.Name == "Tips");
            var mealTips = tipsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "MealTips");
            mealTips.TextItem.Contents = cover.MealTips;

            var mealPrepTips = tipsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Meal Prep Tips");

            double posY = mealPrepTips.TextItem.Position[1];
            double posXHeader = mealPrepTips.TextItem.Position[0];
            double posXText = mealTips.TextItem.Position[0];
            double heightHeader = mealPrepTips.Bounds[3] - mealPrepTips.Bounds[1];
            double heightText = mealTips.Bounds[3] - mealTips.Bounds[1];

            posY += mealToText;
            mealTips.TextItem.Position = new object[] { posXText, posY };

            posY += heightText + textToMeal + heightHeader;
            var reachOutLayer = tipsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("Reach Out"));
            reachOutLayer.TextItem.Position = new object[] { posXHeader, posY };

            posY += mealToText;
            var asYouLayer = tipsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("As you are"));
            asYouLayer.TextItem.Position = new object[] { posXText, posY };

            heightText = asYouLayer.Bounds[3] - asYouLayer.Bounds[1];
            posY += heightText + textToMeal + heightHeader;
            var happyCookungLayer = tipsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("Happy Cooking!"));
            happyCookungLayer.TextItem.Position = new object[] { posXHeader, posY };
        }

        #endregion //Cover

        #region Tips

        private string GenerateTips(ps.Application app, Tips tips, string userName, string tagLine)
        {
            bool photoshopPSD = !string.IsNullOrEmpty(tips.PhotoshopPSD);
            string psdFile = string.Empty;
            if (photoshopPSD)
            {
                psdFile = AzureDownloadToFile(tips.PhotoshopPSD, "psdinput");
                if (!string.IsNullOrEmpty(psdFile))
                {
                    app.Load(psdFile);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                string tipsFile = Path.Combine(cfg.PSDInput, cfg.TipsFile);
                app.Load(tipsFile);
            }
            var doc = app.ActiveDocument;

            string fileName = string.IsNullOrEmpty(userName) ? $"{weekOfFileName}-Tips" : $"{userName}-{weekOfFileName}-Tips";

            LogInformationBegin("File \"{0}.psd\"...", fileName);

            if (string.IsNullOrEmpty(tips.PhotoshopPSD))
            {
                if (!string.IsNullOrEmpty(tips.Image))
                {
                    string imageName = UpdateImage(tips.Image, "Tips", doc);
                    File.Delete(imageName);
                }
            }
            var photoLayer = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Image");
            double offset = photoLayer.Bounds[2] - photoLayer.Bounds[1];
            UpdateTagLine(doc, tagLine, 350);
            if (tips.Tip.Count > 0)
            {
                UpdateTips(doc, tips);
            }

            Save(doc, fileName, photoshopPSD, psdFile);

            LogInformation("OK");
            
            doc.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            return fileName;
        }

        private void UpdateTips(ps.Document doc, Tips tips)
        {
            int mealToText = 35; // px
            int textToMeal = 50; // px

            double posY = 0;
            double posXHeader = 0;
            double posXText = 0;
            double heightHeader = 0;
            double heightText = 0;

            var tipsLayer = doc.LayerSets.Cast<ps.LayerSet>().FirstOrDefault(l => l.Name == "Tips");
            var previousElement = tipsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "1");

            int numberTip = 1;
            foreach (var tip in tips.Tip)
            {
                if (numberTip < 4)
                {
                    string tipName = string.Format("Tip{0}", numberTip);
                    var tipText = tipsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == tipName);
                    if (numberTip == 1)
                    {
                        posXHeader = previousElement.TextItem.Position[0];
                        posY = previousElement.TextItem.Position[1];
                        posXText = tipText.TextItem.Position[0];
                        heightHeader = previousElement.Bounds[3] - previousElement.Bounds[1];
                    }

                    previousElement.TextItem.Contents = tip.Key;
                    tipText.TextItem.Contents = tip.Value;
                    posY += mealToText;
                    tipText.TextItem.Position = new object[] { posXText, posY };
                    heightText = tipText.Bounds[3] - tipText.Bounds[1];
                    numberTip++;

                    if (numberTip < 4)
                    {
                        posY += heightText + textToMeal + heightHeader;
                        previousElement = tipsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == numberTip.ToString());
                        previousElement.TextItem.Position = new object[] { posXHeader, posY };
                    }
                }
                else
                {
                    break;
                }
            }

            for (int i = numberTip; i < 4; i++)
            {
                string tipName = string.Format("Tip{0}", numberTip);
                var header = tipsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == i.ToString());
                var mealText = tipsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == tipName);
                header.Visible = false;
                mealText.Visible = false;
            }
            UpdateTagLine(doc, "", 0);
        }

        #endregion //Back

        #region Recipe

        private string GenerateRecipe(ps.Application app, Recipe recipe, string userName, string tagLine, string type)
        {
            string fileName;
            if(recipe.IsModified)
            {
                fileName = $"{userName}-{type}-{recipe.RecipeID}";
            }
            else
            {
                fileName = $"{type}-{recipe.RecipeID}";
            }

           // if(!ExistBlob("recipecards", $"{fileName}-Page1.jpg"))
            {
                bool photoshopPSD = !string.IsNullOrEmpty(recipe.PhotoshopPSD);
                string psdFile = string.Empty;
                if (photoshopPSD)
                {
                    psdFile = AzureDownloadToFile(recipe.PhotoshopPSD, "psdinput");
                    if (!string.IsNullOrEmpty(psdFile))
                    {
                        app.Load(psdFile);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    string recipeFile = Path.Combine(cfg.PSDInput, cfg.RecipeFile);
                    app.Load(recipeFile);
                }

                var doc = app.ActiveDocument;

                LogInformationBegin("File \"{0}.psd\"...", fileName);

                if (recipe.Ingredients.Count > 0 || recipe.Garnish.Count > 0 || !string.IsNullOrEmpty(recipe.Tip))
                {
                    var IngredientsLayerSet = doc.LayerSets.Cast<ps.LayerSet>().FirstOrDefault(l => l.Name == "Ingredients");
                    //Modify the ingredients to be sentence case
                    List<string> s = new List<string>();
                    List<string> s1 = new List<string>();
                    foreach (var item in recipe.Ingredients)
                    {
                        s.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.ToLower()));
                    }
                    foreach(var item in recipe.Garnish)
                    {
                        s1.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.ToLower()));
                    }
                    recipe.Ingredients = s;
                    recipe.Garnish = s1;
                    UpdateRecipeIngredients(IngredientsLayerSet, recipe);
                }

                if (string.IsNullOrEmpty(recipe.PhotoshopPSD))
                {
                    var headerLayerSet = doc.LayerSets.Cast<ps.LayerSet>().FirstOrDefault(l => l.Name == "Header");
                    if (!string.IsNullOrEmpty(recipe.Description))
                    {
                        var descriptionLayer = headerLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("Description"));
                        UpdateRecipeHeader(descriptionLayer, recipe.Description);
                    }
                    if (!string.IsNullOrEmpty(recipe.MealType))
                    {
                        var mealTypeLayer = headerLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("MealType"));
                        UpdateRecipeHeader(mealTypeLayer, recipe.MealType);
                    }
                    if (!string.IsNullOrEmpty(recipe.MealName))
                    {
                        var mealNameLayer = headerLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("MealName"));
                        UpdateRecipeHeader(mealNameLayer, recipe.MealName);
                    }
                    if (!string.IsNullOrEmpty(recipe.TotalActiveCookTime))
                    {
                        var mealNameLayer = headerLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("TotalActiveCookTime"));
                        UpdateRecipeHeader(mealNameLayer, CookTimeToString(recipe.TotalActiveCookTime));
                    }
                    if (!string.IsNullOrEmpty(recipe.Cusine))
                    {
                        var mealNameLayer = headerLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("Cusine"));
                        UpdateRecipeHeader(mealNameLayer, recipe.Cusine);
                    }
                    if (!string.IsNullOrEmpty(recipe.Yield))
                    {
                        var mealNameLayer = headerLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name.StartsWith("Yield"));
                        UpdateRecipeHeader(mealNameLayer, recipe.Yield);
                    }
                    if (!string.IsNullOrEmpty(recipe.Image))
                    {
                        recipe.Image = UpdateImage(recipe.Image, "Recipe", doc);
                    }

                    var photoLayer = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Image");
                    double offset = photoLayer.Bounds[2] - photoLayer.Bounds[1];
                    UpdateTagLine(doc, "", offset);

                    if (recipe.Steps.Count > 0)
                    {
                        UpdareRecipeInstruction(doc, recipe, ref fileName, photoshopPSD);
                    }
                }

                if (string.IsNullOrEmpty(recipe.Image))
                {
                    recipe.Image = SaveImage(doc);
                }

                Save(doc, fileName, photoshopPSD, psdFile);

                LogInformation("OK");

                doc.Close(ps.PsSaveOptions.psDoNotSaveChanges);
                return fileName;
            }
            return fileName;
        }

        private string SaveImage(ps.Document doc)
        {
            var photoLayer = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Image");
            doc.ActiveLayer = photoLayer;
            var app = doc.Application;

            var placedLayerEditContents = app.StringIDToTypeID("placedLayerEditContents");
            var desc1 = new ps.ActionDescriptor();
            app.ExecuteAction(placedLayerEditContents, desc1, ps.PsDialogModes.psDisplayNoDialogs);

            var smartObjectContents = doc.Application.ActiveDocument;

            var idnewPlacedLayer = app.StringIDToTypeID("newPlacedLayer");
            app.ExecuteAction(idnewPlacedLayer, null, ps.PsDialogModes.psDisplayNoDialogs);

            var idplacedLayerEditContents = app.StringIDToTypeID("placedLayerEditContents");
            var desc2 = new ps.ActionDescriptor();
            app.ExecuteAction(idplacedLayerEditContents, desc2, ps.PsDialogModes.psDisplayNoDialogs);

            var imajeObjectContents = doc.Application.ActiveDocument;

            string fileName = Path.Combine(Path.GetTempPath(), string.Format("{0}{1}", Guid.NewGuid(), ".jpg"));
            SavePsdToJpeg1(fileName, imajeObjectContents);

            imajeObjectContents.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            smartObjectContents.Close(ps.PsSaveOptions.psDoNotSaveChanges);

            return fileName;
        }

        private void UpdateRecipeHeader(ps.ArtLayer artLayer, string text)
        {
            artLayer.TextItem.Contents = text;
        }

        private void UpdateRecipeIngredients(ps.LayerSet ingredientsLayerSet, Recipe recipe)
        {
            int headerToText = 35; // px
            int textToHeader = 50; // px

            double posY = 0;
            double posX = 0;
            double heightHeader = 0;
            double heightText = 0;

            var ingredientsHeader = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "IngredientsLabel");
            var ingredientsText = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Ingredients");

            posX = ingredientsHeader.TextItem.Position[0];
            posY = ingredientsHeader.TextItem.Position[1];
            heightHeader = ingredientsHeader.Bounds[3] - ingredientsHeader.Bounds[1];

            if (recipe.Ingredients.Count > 0)
            {
                ingredientsText.TextItem.Contents = string.Join("\r", recipe.Ingredients);
            }
            posY += headerToText;
            ingredientsText.TextItem.Position = new object[] { posX, posY };
            heightText = ingredientsText.Bounds[3] - ingredientsText.Bounds[1];

            var garnishHeader = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "GarnishLabel");
            var garnishText = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Garnish");

            if (recipe.Garnish.Count > 0)
            {
                posY += heightText + textToHeader + heightHeader;
                garnishHeader.TextItem.Position = new object[] { posX, posY };
                garnishText.TextItem.Contents = string.Join("\r", recipe.Garnish);
                posY += headerToText;
                garnishText.TextItem.Position = new object[] { posX, posY };
                heightText = garnishText.Bounds[3] - garnishText.Bounds[1]; //
            }
            else
            {
                garnishHeader.Visible = false;
                garnishText.Visible = false;
            }

            try
            {
                var tipHeader = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "TipLabel");
                var tipText = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Tip");

                posY += heightText + textToHeader + heightHeader;
                tipHeader.TextItem.Position = new object[] { posX, posY };

                posY += headerToText;
                tipText.TextItem.Position = new object[] { posX, posY };

                if (string.IsNullOrEmpty(recipe.PhotoshopPSD))
                {
                    if (!string.IsNullOrEmpty(recipe.Tip))
                    {
                        tipText.TextItem.Contents = recipe.Tip;
                    }
                    else
                    {
                        tipHeader.Visible = false;
                        tipText.Visible = false;
                    }
                }
            }
            catch
            { }
        }

        private void UpdareRecipeInstruction(ps.Document doc, Recipe recipe, ref string fileName, bool photoshopPSD)
        {
            int stepToStep = 35; // px
            int countPage = 1;
            double posYFirst = 0;
            double posY = 0;
            double stepHeight = 0;
            double posXHeader = 0;
            double posXText = 0;
            var tagline = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Tagline");
            double docHeight = tagline.Bounds[1] - stepToStep;

            var directionsLayer = doc.LayerSets.Cast<ps.LayerSet>().FirstOrDefault(l => l.Name == "Directions");

            var instructionsElement = directionsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Instructions");
            posXHeader = instructionsElement.Bounds[0];
            posYFirst = instructionsElement.Bounds[3];
            posY = posYFirst;

            int count = 1;
            int countFirst = count;
            foreach (string s in recipe.Steps)
            {
                var stepElement = directionsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == string.Format("Step{0}", count));
                var stepDetailsElement = directionsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == string.Format("StepDetails{0}", count));

                if (count == 1)
                {
                    posXText = stepDetailsElement.Bounds[0];
                    stepHeight = stepElement.Bounds[3] - stepElement.Bounds[1];
                }

                stepElement.TextItem.Contents = count.ToString();
                stepDetailsElement.TextItem.Contents = s;

                posY += stepToStep;
                stepElement.TextItem.Position = new object[] { posXHeader, posY + stepHeight };
                stepDetailsElement.TextItem.Position = new object[] { posXText, posY };
                posY += stepDetailsElement.Bounds[3] - stepDetailsElement.Bounds[1];

                if (posY >= docHeight)
                {
                    stepElement.Visible = false;
                    stepDetailsElement.Visible = false;
                    string fileNamePage = string.Format("{0}-Page{1}.psd", fileName, countPage);

                    Save(doc, fileNamePage, photoshopPSD, "");

                    for (int i = countFirst; i < count; i++)
                    {
                        directionsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == string.Format("Step{0}", i)).Delete();
                        directionsLayer.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == string.Format("StepDetails{0}", i)).Delete();
                    }

                    stepElement.Visible = true;
                    stepDetailsElement.Visible = true;
                    posY = posYFirst;
                    posY += stepToStep;
                    stepElement.TextItem.Position = new object[] { posXHeader, posY + stepHeight };
                    stepDetailsElement.TextItem.Position = new object[] { posXText, posY };
                    countPage++;
                    countFirst = count;
                }

                count++;
                if (recipe.Steps.Count >= count)
                {
                    var newStepElement = stepElement.Duplicate();
                    newStepElement.Name = string.Format("Step{0}", count);
                    var newStepDetailsElement = stepDetailsElement.Duplicate();
                    newStepDetailsElement.Name = string.Format("StepDetails{0}", count);
                }
            }
            if (countPage > 0)
            {
                fileName = string.Format("{0}-Page{1}", fileName, countPage);
            }
        }

        #endregion

        #region InstagramPinterest

        private void GeneratePinterest(ps.Application app, Recipe recipe, string userName, string tagLine)
        {
            string type = string.Empty;
            if (string.IsNullOrEmpty(tagLine))
            {
                string pinterestFile = string.Empty;
                switch(cfg.SocialType)
                {
                    case SocialType.Normal:
                    case SocialType.SocialNoRecipecardNormal:
                    case SocialType.SocialRecipecardComplete:
                        type = cfg.SocialType.ToString();
                        pinterestFile = Path.Combine(cfg.PSDInput, cfg.PinterestFile);
                        break;

                    case SocialType.Passport:
                        type = "Passport";
                        pinterestFile = Path.Combine(cfg.PSDInput, cfg.PTPassportFile);
                        break;

                    case SocialType.Complete:
                        type = "Complete";
                        pinterestFile = Path.Combine(cfg.PSDInput, cfg.PTMealsFile);
                        break;
                }
                app.Load(pinterestFile);

                if(cfg.SocialType == SocialType.SocialRecipecardComplete)
                {
                    app.Load(Path.Combine(cfg.PSDInput, cfg.PTPassportFile));
                    app.Load(Path.Combine(cfg.PSDInput, cfg.PTMealsFile));
                }
            }
            else
            {
                if (tagLine.ToLower().StartsWith("complete"))
                {
                    type = "Complete";
                    app.Load(Path.Combine(cfg.PSDInput, cfg.PTMealsFile));
                }
                else if (tagLine.ToLower().StartsWith("passport"))
                {
                    type = "Passport";
                    app.Load(Path.Combine(cfg.PSDInput, cfg.PTPassportFile));
                }
                else
                {
                    type = "Pinterest";
                    app.Load(Path.Combine(cfg.PSDInput, cfg.PinterestFile));
                }
            }

            var doc = app.ActiveDocument;

            string fileName = string.IsNullOrEmpty(userName) ? $"{recipe.RecipeID}-{type}-Pinterest": $"{userName}-{recipe.RecipeID}-{type}-Pinterest";

            LogInformationBegin("File \"{0}.psd\"...", Path.GetFileName(fileName));

            if (!string.IsNullOrEmpty(recipe.MealName))
            {
                UpdateTitle12(doc, recipe);
            }

            if (!string.IsNullOrEmpty(recipe.Image))
            {
                UpdateImage1200(recipe.Image, type, doc);
            }

            if (cfg.OutputASJpeg)
            {
                AzureUploadFromFileAsync(doc, $"{fileName}.jpg", "socialcards", true);
            }
            if (cfg.OutputASPSD)
            {
                string filePSD = Path.Combine(cfg.SocialCardsOutput, $"{fileName}.psd");
                //if (!File.Exists(filePSD))
                //{
                    doc.SaveAs(filePSD);
                //}
                //else
                //{
                //    LogError($"File \"{filePSD}\" already exists - skip.");
                //}
            }

            LogInformation("OK");
            
            doc.Close(ps.PsSaveOptions.psDoNotSaveChanges);
        }

        private void UpdateTitle12(ps.Document doc, Recipe recipe)
        {
            bool title2Visible = true;
            int titleToTitle = 25; // px
            var title1Element = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Title1");
            var title2Element = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Title2");
            var rsOrangeElement = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "RSOrange");
            title1Element.TextItem.Contents = recipe.MealName;
            
            double widthTitle1 = title1Element.Bounds[2] - title1Element.Bounds[0];
            if (widthTitle1 < (doc.Width - 100)) // indent left 50px and indent right 50px
            {
                title2Element.Visible = false;
                title2Visible = false;
            }
            else
            {
                string[] titles = recipe.MealName.Split(' ');
                title1Element.TextItem.Contents = titles[0];
                if (titles.Length > 1)
                {
                    for (int i = 1; i < titles.Length; i++)
                    {
                        title1Element.TextItem.Contents += string.Format(" {0}", titles[i]);
                        widthTitle1 = title1Element.Bounds[2] - title1Element.Bounds[0];
                        if (widthTitle1 > (doc.Width - 100)) // indent left 50px and indent right 50px
                        {
                            title1Element.TextItem.Contents = string.Join(" ", titles, 0, i);
                            title2Element.TextItem.Contents = string.Join(" ", titles, i, titles.Length - i);
                            break;
                        }
                    }
                }
                else
                {
                    LogError("Title exceeds allowed width. Splitting is not possible. Title: {0}.", recipe.MealName);
                }
            }

            //double posY = titleToTitle;
            double posY = rsOrangeElement.Bounds[1];
            posY -= titleToTitle;

            if (title2Visible)
            {
                title2Element.TextItem.Position = new object[] { title2Element.TextItem.Position[0], posY };
                posY = title2Element.Bounds[1];
                posY -= titleToTitle;
            }
            title1Element.TextItem.Position = new object[] { title1Element.TextItem.Position[0], posY };
        }

        private void GenerateInstagram(ps.Application app, Recipe recipe, string userName, string tagLine)
        {            
            app.Load(Path.Combine(cfg.PSDInput, cfg.InstagramFile));

            var doc = app.ActiveDocument;

            string fileName = string.IsNullOrEmpty(userName) ? $"{recipe.RecipeID}-Instagram": $"{userName}-{recipe.RecipeID}-Instagram";

            LogInformationBegin("File \"{0}.psd\"...", Path.GetFileName(fileName));

            if (!string.IsNullOrEmpty(recipe.Image))
            {
                UpdateImage1200(recipe.Image, "Instagram", doc);

                if (cfg.OutputASJpeg)
                {
                    AzureUploadFromFileAsync(doc, $"{fileName}.jpg", "socialcards", true);
                }
                if (cfg.OutputASPSD)
                {
                    string filePSD = Path.Combine(cfg.SocialCardsOutput, $"{fileName}.psd");
                    //if (!File.Exists(filePSD))
                    //{
                        doc.SaveAs(filePSD);
                    //}
                    //else
                    //{
                    //    LogError($"File \"{filePSD}\" already exists - skip.");
                    //}
                }
            }
            else
            {
                LogError("Instagram image empty.");
            }

                LogInformation("OK");
            
            doc.Close(ps.PsSaveOptions.psDoNotSaveChanges);
        }

        #endregion

        #region Shopping

        private string GenerateShopping(ps.Application app, ShoppingList shopping, string userName, string tagLine)
        {
            bool photoshopPSD = !string.IsNullOrEmpty(shopping.PhotoshopPSD);
            string psdFile = string.Empty;
            if (photoshopPSD)
            {
                psdFile = AzureDownloadToFile(shopping.PhotoshopPSD, "psdinput");
                if (!string.IsNullOrEmpty(psdFile))
                {
                    app.Load(psdFile);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                string cshoppingFile = Path.Combine(cfg.PSDInput, cfg.ShoppingFile);
                app.Load(cshoppingFile);
            }
            var doc = app.ActiveDocument;

            string fileName = string.IsNullOrEmpty(userName) ? $"{weekOfFileName}-ShoppingList" : $"{userName}-{weekOfFileName}-ShoppingList";

            LogInformationBegin("File \"{0}.psd\"...", Path.GetFileName(fileName));

            if (string.IsNullOrEmpty(shopping.PhotoshopPSD))
            {
                var ingredientsLayerSet = doc.LayerSets.Cast<ps.LayerSet>().FirstOrDefault(l => l.Name == "Ingredients");

                var leftArtLayer = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Produce");
                var centerArtLayer = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Meat");
                var rightArtLayer = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Herbs");

                double y = leftArtLayer.TextItem.Position[1];
                double xLeft = leftArtLayer.TextItem.Position[0];
                double xCenter = centerArtLayer.TextItem.Position[0];
                double xRight = rightArtLayer.TextItem.Position[0];

                Dictionary<string, ShoppingElement> shoppingElements = new Dictionary<string, ShoppingElement>();
                shoppingElements.Add("Produce", UpdateShoppingElement(ingredientsLayerSet, "Produce", shopping.List.Produce));
                shoppingElements.Add("Baking", UpdateShoppingElement(ingredientsLayerSet, "Baking", shopping.List.Baking));
                shoppingElements.Add("Canned", UpdateShoppingElement(ingredientsLayerSet, "Canned", shopping.List.Canned));
                shoppingElements.Add("Meat", UpdateShoppingElement(ingredientsLayerSet, "Meat", shopping.List.Meat));
                shoppingElements.Add("Dairy", UpdateShoppingElement(ingredientsLayerSet, "Dairy", shopping.List.Dairy));
                shoppingElements.Add("Beverages", UpdateShoppingElement(ingredientsLayerSet, "Beverages", shopping.List.Beverages));
                shoppingElements.Add("Herbs", UpdateShoppingElement(ingredientsLayerSet, "Herbs", shopping.List.Herbs));
                shoppingElements.Add("Pantry", UpdateShoppingElement(ingredientsLayerSet, "Pantry", shopping.List.Pantry));

                shoppingElements = shoppingElements.Where(e => e.Value.HeightText > 0).OrderByDescending(e => e.Value.HeightText).ToDictionary(e => e.Key, e => e.Value);

                double leftHeight = 0;
                double centrHeight = 0;
                double rightHeight = 0;

                foreach (var e in shoppingElements)
                {
                    switch (FindMin(leftHeight, centrHeight, rightHeight))
                    {
                        case Column.Left:
                            e.Value.Column = Column.Left;
                            leftHeight += (e.Value.HeightText + 107); // widthTitle=57 and textToTitle= 50px
                            break;

                        case Column.Center:
                            e.Value.Column = Column.Center;
                            centrHeight += (e.Value.HeightText + 107);
                            break;

                        case Column.Right:
                            e.Value.Column = Column.Right;
                            rightHeight += (e.Value.HeightText + 107);
                            break;
                    }
                }

                var leftElements = shoppingElements.Where(e => e.Value.Column == Column.Left).OrderByDescending(e => e.Value.HeightText).ToDictionary(e => e.Key, e => e.Value);
                var CenterElements = shoppingElements.Where(e => e.Value.Column == Column.Center).OrderByDescending(e => e.Value.HeightText).ToDictionary(e => e.Key, e => e.Value);
                var rightElements = shoppingElements.Where(e => e.Value.Column == Column.Right).OrderByDescending(e => e.Value.HeightText).ToDictionary(e => e.Key, e => e.Value);

                AlignmentShoppingElements(leftElements, 0, y, doc.Width / 3);
                AlignmentShoppingElements(CenterElements, doc.Width / 3, y, doc.Width / 3 * 2);
                AlignmentShoppingElements(rightElements, doc.Width / 3 * 2, y, doc.Width);
                UpdateTagLine(doc, "", 0);
            }

            Save(doc, fileName, photoshopPSD, psdFile);

            LogInformation("OK");
            
            doc.Close(ps.PsSaveOptions.psDoNotSaveChanges);
            return fileName;
        }

        private static ShoppingElement UpdateShoppingElement(ps.LayerSet ingredientsLayerSet, string name, List<string> value)
        {
            var titleArtLayer = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == name);
            var textArtLayer = ingredientsLayerSet.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == string.Format("{0}#", name));
            if (value.Count > 0)
            {
                textArtLayer.TextItem.Contents = string.Join("\r", value);
                ShoppingElement se = new ShoppingElement()
                {
                    TitleLayer = titleArtLayer,
                    TextLayer = textArtLayer,
                    HeightText = textArtLayer.Bounds[3] - textArtLayer.Bounds[1],
                    HeightTitle = titleArtLayer.Bounds[3] - titleArtLayer.Bounds[1],
                    WidthText = textArtLayer.Bounds[2] - textArtLayer.Bounds[0],
                    WidthTitle = titleArtLayer.Bounds[2] - titleArtLayer.Bounds[0]
                };
                return se;
            }
            else
            {
                titleArtLayer.Visible = false;
                textArtLayer.Visible = false;
                ShoppingElement se = new ShoppingElement()
                {
                    TitleLayer = titleArtLayer,
                    TextLayer = textArtLayer
                };
                return se;
            }
        }

        private Column FindMin(double leftHeight, double centrHeight, double rightHeight)
        {
            if (leftHeight <= centrHeight && leftHeight <= rightHeight)
            {
                return Column.Left;
            }
            if (centrHeight <= rightHeight)
            {
                return Column.Center;
            }
            return Column.Right;
        }

        private void AlignmentShoppingElements(Dictionary<string, ShoppingElement> columnElements, double x, double y, double width)
        {
            if (columnElements.Count > 0)
            {
                int titleToText = 35; // px
                int textToTitle = 50; // px

                double maxWidth = 0;
                double maxWidthText = columnElements.Max(e => e.Value.WidthText);
                double maxWidthTitle = columnElements.Max(e => e.Value.WidthTitle);

                if (maxWidthText > maxWidthTitle)
                {
                    maxWidth = maxWidthText;
                }
                else
                {
                    maxWidth = maxWidthTitle;
                }

                x = x + (width - x - maxWidth) / 2;
                if (x <= 0)
                {
                    x = 50;
                }

                foreach (var e in columnElements)
                {
                    e.Value.TitleLayer.TextItem.Position = new object[] { x, y };
                    y += titleToText;
                    e.Value.TextLayer.TextItem.Position = new object[] { x, y };
                    y += e.Value.HeightText + textToTitle + e.Value.HeightTitle;
                }
            }
        }

        #endregion

        public bool LoadRecipeSavantsList(string filename)
        {
            try
            {
                var json = File.ReadAllText(filename);
                recipeSavantsList = JsonConvert.DeserializeObject<List<RecipeSavants>>(json);
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error load json file: \"{0}\".", filename);
                return false;
            }
        }

        public bool LoadCfg(string filename)
        {
            try
            {
                var json = File.ReadAllText(filename);
                cfgDefault = JsonConvert.DeserializeObject<DataModel.Configuration>(json);
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error load json cfg file: \"{0}\".", filename);
                return false;
            }
        }

        private List<RecipeService> GetRecipeServiceList(string idString)
        {
            try
            {
                List<RecipeService> rS = new List<RecipeService>();
                string[] ids = idString.Split(',');
                foreach (string id in ids)
                {
                    string json = DownloadRecipeJson(id);
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            RecipeService result = JsonConvert.DeserializeObject<RecipeService>(json);
                            rS.Add(result);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, "Error during parsing recipe. Id: \"{0}\".", id);
                        }
                    }
                }
                return rS;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error during parsing \"{0}\".", idString);
                return new List<RecipeService>();
            }
        }

        private string DownloadRecipeJson(string id)
        {
            string json = string.Empty;
            string url = string.Format("https://rest.recipesavants.com/api/recipe/{0}", id);
            try
            {
                using (WebClient client = new WebClient())
                {
                    json = client.DownloadString(url);
                }
            }
            catch (Exception ex)
            {
                LogError("{0} file not loaded: {1}", ex.Message, url);
                return string.Empty;
            }
            return json;
        }

        private string UpdateImage(string imagePath, string card, ps.Document doc)
        {
            //imagePath = Path.Combine(fi.Directory.FullName, imagePath);
            string localFileName = DownloadImage(imagePath);
            if (!File.Exists(localFileName))
            {
                LogError("Card \"{0}\" - file not found ({1}).", card, localFileName);
            }
            else
            {
                doc.Application.Load(Path.Combine(Environment.CurrentDirectory, localFileName));
                var photoDoc = doc.Application.ActiveDocument;
                doc.Application.Preferences.RulerUnits = ps.PsUnits.psPixels;

                if (photoDoc.Height != 2600)
                {
                    photoDoc.ResizeImage(null, 2600, 300, ps.PsResampleMethod.psBicubicSharper);
                }
                photoDoc.Selection.SelectAll();
                photoDoc.Selection.Copy();
                photoDoc.Close(ps.PsSaveOptions.psDoNotSaveChanges);

                doc.Application.ActiveDocument = doc;
                var photoLayer = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Image");
                doc.ActiveLayer = photoLayer;
                var app = doc.Application;

                var placedLayerEditContents = app.StringIDToTypeID("placedLayerEditContents");
                var desc1 = new ps.ActionDescriptor();
                app.ExecuteAction(placedLayerEditContents, desc1, ps.PsDialogModes.psDisplayNoDialogs);

                var smartObjectContents = doc.Application.ActiveDocument;
                smartObjectContents.Selection.SelectAll();
                var newLayer = smartObjectContents.Paste();
                newLayer.Move(smartObjectContents, ps.PsElementPlacement.psPlaceAtBeginning);
                smartObjectContents.Close(ps.PsSaveOptions.psSaveChanges);
            }
            return localFileName;
        }

        private void UpdateImage1200(string imagePath, string card, ps.Document doc)
        {
            string localFileName = DownloadImage(imagePath); 
            
            if (!File.Exists(localFileName))
            {
                LogError("Card \"{0}\" - file not found ({1}).", card, localFileName);
            }
            else
            {
                doc.Application.Load(Path.Combine(Environment.CurrentDirectory, localFileName));
                var photoDoc = doc.Application.ActiveDocument;
                doc.Application.Preferences.RulerUnits = ps.PsUnits.psPixels;

                if (photoDoc.Height != 1200)
                {
                    photoDoc.ResizeImage(null, 1200, 300, ps.PsResampleMethod.psBicubicSharper);
                }
                photoDoc.Selection.SelectAll();
                photoDoc.Selection.Copy();
                photoDoc.Close(ps.PsSaveOptions.psDoNotSaveChanges);

                doc.Application.ActiveDocument = doc;
                var photoLayer = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Image");
                doc.ActiveLayer = photoLayer;

                doc.Selection.SelectAll();
                var imageLayer = doc.Paste(true);
                imageLayer.BlendMode = photoLayer.BlendMode;

                // delete mask layer
                var app = doc.Application;
                var idDlt = app.CharIDToTypeID("Dlt ");
                var desc2 = new ps.ActionDescriptor();
                var idnull = app.CharIDToTypeID("null");
                var ref1 = new ps.ActionReference();
                var idChnl = app.CharIDToTypeID("Chnl");
                var idMsk = app.CharIDToTypeID("Msk ");
                ref1.PutEnumerated(idChnl, idChnl, idMsk);
                desc2.PutReference(idnull, ref1);
                app.ExecuteAction(idDlt, desc2, ps.PsDialogModes.psDisplayNoDialogs);

                imageLayer.Move(photoLayer, ps.PsElementPlacement.psPlaceBefore);
                photoLayer.Visible = false;
            }
        }

        private string DownloadImage(string url)
        {
            string localFileName = string.Empty;
            string ext = Path.GetExtension(url);
            do
            {
                localFileName = Path.Combine(Path.GetTempPath(), string.Format("{0}{1}", Guid.NewGuid(), ext));
            }
            while (File.Exists(localFileName));

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, localFileName);
                }
            }
            catch (Exception ex)
            {
                LogError("{0} file not loaded: {1}", ex.Message, url);
                return string.Empty;
            }
            return localFileName;
        }

        private void SavePsdToJpeg1(string fileName, ps.Document doc)
        {
            var jpgSaveOptions = new ps.JPEGSaveOptions();
            jpgSaveOptions.EmbedColorProfile = true;
            jpgSaveOptions.FormatOptions = ps.PsFormatOptionsType.psStandardBaseline;
            jpgSaveOptions.Matte = ps.PsMatteType.psNoMatte;
            jpgSaveOptions.Quality = cfg.Quality;
            string fileNameJpeg = fileName.Replace(".psd", ".jpeg");
            doc.SaveAs(fileNameJpeg, jpgSaveOptions, true);
        }

        public void UpdateTagLine(ps.Document doc, string tagLine, double offset)
        {
            var tagLineLayer = doc.ArtLayers.Cast<ps.ArtLayer>().FirstOrDefault(l => l.Name == "Tagline");

            switch(cfg.SocialType)
            {
                case SocialType.Normal:
                    tagLine = "All Rights Reserved | RecipeSavants.com";
                    break;

                case SocialType.Passport:
                case SocialType.PassportNoSocial:
                    tagLine = "Passport Meal Plans | All Rights Reserved | RecipeSavants.com";
                    break;

                case SocialType.Complete:
                    tagLine = "3 Course Menu Plans | All Rights Reserved | RecipeSavants.com";
                    break;
            }

            tagLineLayer.TextItem.Contents = tagLine;
            double length = tagLineLayer.Bounds[2] - tagLineLayer.Bounds[0];
            double x = (doc.Width - offset - length) / 2 + offset;
            double y = tagLineLayer.TextItem.Position[1];
            tagLineLayer.TextItem.Position = new object[] { x, y };
        }

        private void Save(ps.Document doc, string fileName, bool photoshopPSD, string psdFile)
        {
            if (cfg.OutputASJpeg)
            {
                AzureUploadFromFileAsync(doc, $"{fileName}.jpg", "recipecards", true);
            }
            if (cfg.OutputASPSD)
            {
                if (photoshopPSD)
                {
                    AzureUploadFromFileAsync(doc, $"{fileName}.psd", "psdoutput", false);
                    try
                    {
                        File.Delete(psdFile);
                    }
                    catch { }
                }
                else
                {
                    string filePSD = Path.Combine(cfg.RecipeCardsOutput, $"{fileName}.psd");
                    //if (!File.Exists(filePSD))
                    //{
                        doc.SaveAs(filePSD);
                    //}
                    //else
                    //{
                    //    LogError($"File \"{filePSD}\" already exists - skip.");
                    //}
                }
            }
        }

        private string AzureDownloadToFile(string fileName, string blobStore)
        {
            try
            {
                CloudBlobContainer container = blobClient.GetContainerReference(blobStore);
                bool exist = blobClient.GetContainerReference(blobStore).GetBlockBlobReference(fileName).Exists();
                if (exist)
                {
                    string localFileName = string.Empty;
                    string ext = Path.GetExtension(fileName);
                    do
                    {
                        localFileName = Path.Combine(Path.GetTempPath(), string.Format("{0}{1}", Guid.NewGuid(), ext));
                    }
                    while (File.Exists(localFileName));
                    CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
                    blob.DownloadToFile(localFileName, FileMode.OpenOrCreate);
                    return localFileName;
                }
                else
                {
                    LogError($"File \"{fileName}\" does not exists in the Blob \"{blobStore}\".");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error download file \"{fileName}\" from \"{blobStore}\".");
                return string.Empty;
            }
        }

        private async System.Threading.Tasks.Task<bool> AzureUploadFromFileAsync(ps.Document doc, string fileName, string blobStore, bool saveJpeg)
        {
            try
            {
                //bool exist = ExistBlob(blobStore, fileName);

                string localFileName = string.Empty;
                string ext = Path.GetExtension(fileName);
                do
                {
                    localFileName = Path.Combine(cfg.RecipeCardsOutput, string.Format("{0}{1}", Guid.NewGuid(), ext));
                }
                while (File.Exists(localFileName));

                if (saveJpeg)
                {
                    var jpgSaveOptions = new ps.JPEGSaveOptions();
                    jpgSaveOptions.EmbedColorProfile = true;
                    jpgSaveOptions.FormatOptions = ps.PsFormatOptionsType.psStandardBaseline;
                    jpgSaveOptions.Matte = ps.PsMatteType.psNoMatte;
                    jpgSaveOptions.Quality = 1;
                    doc.SaveAs(localFileName, jpgSaveOptions, false);
                }
                else
                {
                    doc.SaveAs(localFileName);
                }

                CloudBlobContainer container = blobClient.GetContainerReference(blobStore);
                CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
                blob.UploadFromFile(localFileName, FileMode.OpenOrCreate);
                File.Delete(localFileName);
                return true;
            }
            catch(Exception ex)
            {
                LogError(ex, "Error upload file \"{0}\".", fileName);
                return false;
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private bool ExistBlob(string blobStore, string fileName)
        {
            bool exist = blobClient.GetContainerReference(blobStore).GetBlockBlobReference(fileName).Exists();
            return exist;
        }

        private string CookTimeToString(string minutes)
        {
            if (string.IsNullOrEmpty(minutes)) return null;

            TimeSpan ts = TimeSpan.FromMinutes(Convert.ToDouble(minutes));
            if (ts.Hours == 0)
            {
                if (ts.Minutes == 0)
                    return null;
                else if (ts.Minutes == 1)
                    return $"1 Min";
                else
                    return $"{ts.Minutes} Minutes";
            }
            else 
            {
                if (ts.Minutes == 0)
                    return $"{ts.Hours} Hours";
                else if (ts.Minutes == 1)
                    return $"{ts.Hours} Hours 1 Minutes";
                else 
                    return $"{ts.Hours} Hour {ts.Minutes} Minutes";
            }
        }
    }
}