using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CreatePdfLibrary
{
    public class CreatePDFController
    {
        private List<RecipeSavantsPDF> recipeSavantsPDFList = new List<RecipeSavantsPDF>();
        
        private CloudStorageAccount storageAccount;

        private CloudBlobClient blobClient;

        public EventHandler<MessagePdfEventArgs> Message;

        #region Log

        public void LogError(string message, params object[] args)
        {
            string erroMsg = FormatMessage(message, args);
            //MainForm.Instance.Log(erroMsg, true);
            //MessageBox.Show(erroMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            OnThresholdReached(new MessagePdfEventArgs { Msg = erroMsg, IsError = true });
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            var sb = new StringBuilder(FormatMessage(message, args));
            sb.AppendLine(ex.ToString());

            //MainForm.Instance.Log(sb.ToString(), true);
            //MessageBox.Show(sb.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            OnThresholdReached(new MessagePdfEventArgs { Msg = sb.ToString(), IsError = true });
        }

        public void LogInformation(string message, params object[] args)
        {
            //MainForm.Instance.Log(FormatMessage(message, args));
            OnThresholdReached(new MessagePdfEventArgs { Msg = FormatMessage(message, args), EndLine = true });
        }

        public void LogInformationBegin(string message, params object[] args)
        {
            //MainForm.Instance.Log(FormatMessage(message, args), false, false);
            OnThresholdReached(new MessagePdfEventArgs { Msg = FormatMessage(message, args), IsError = false, EndLine = false });
        }

        private static string FormatMessage(string message, params object[] args)
        {
            args = args == null ? null : args.Where(a => a != null).ToArray();
            return string.Format(message, args);
        }

        protected virtual void OnThresholdReached(MessagePdfEventArgs e)
        {
            EventHandler<MessagePdfEventArgs> handler = Message;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion //Log

        public void Run(string json, string outputFolder = null)
        {
            try
            {
                LogInformation("[{0}] - Start PDF", DateTime.Now);
                string storageConnectionString = ConfigurationManager.AppSettings["storageconnectionstring"];
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                blobClient = storageAccount.CreateCloudBlobClient();

                if (!CheckParameters(json))
                {
                    return;
                }

                foreach (RecipeSavantsPDF pdf in recipeSavantsPDFList)
                {
                    string filePdf = pdf.Type != "General" ? $"{pdf.Type}-{pdf.Cover}.pdf": $"{pdf.Cover}.pdf";

                    string localFileName = string.Empty;
                    do
                    {
                        localFileName = Path.Combine(string.IsNullOrEmpty(outputFolder) ? Path.GetTempPath() : outputFolder, string.Format("{0}-{1}", Guid.NewGuid(), filePdf));
                    }
                    while (File.Exists(localFileName));

                    using (FileStream fs = new FileStream(localFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        Document doc = null;
                        Image imgCover = ReadImage(pdf.Cover, "Cover", pdf.Type);
                        if (imgCover != null)
                        {
                            Rectangle pagesize = new Rectangle(imgCover.ScaledWidth, imgCover.ScaledHeight);
                            doc = new Document(pagesize);
                            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                            doc.Open();
                            doc.Add(imgCover);
                        }

                        Image imgTips = ReadImage(pdf.Tips, "Tips", pdf.Type);
                        if (imgTips != null)
                        {
                            doc.NewPage();
                            doc.Add(imgTips);
                        }

                        foreach (string recipe in pdf.Recipes)
                        {
                            Image imgRecipe = ReadImageRecipe(recipe, pdf.Type);
                            if (imgRecipe == null) continue;
                            doc.NewPage();
                            doc.Add(imgRecipe);
                        }

                        Image imgShoppintList = ReadImage(pdf.ShoppingList, "ShoppingList", pdf.Type);
                        if (imgShoppintList != null)
                        {
                            doc.NewPage();
                            doc.Add(imgShoppintList);
                        }

                        doc.Close();
                    }
                    AzureUploadFromFile(localFileName, filePdf);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "The process ended with an error.");
            }
            finally
            {
                LogInformation("[{0}] - End PDF", DateTime.Now);
            }
        }

        private bool CheckParameters(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                LogError("JSON file is not specified.");
                return false;
            }

            //if (!File.Exists(json))
            //{
            //    LogError("JSON file doesn't exist ({0}).", json);
            //    return false;
            //}

            recipeSavantsPDFList = GetRecipeSavantsList(json);
            if (recipeSavantsPDFList != null)
            {
                return true;
            }
            return false;
        }

        public List<RecipeSavantsPDF> GetRecipeSavantsList(string filename)
        {
            try
            {
                List<RecipeSavantsPDF> result = JsonConvert.DeserializeObject<List<RecipeSavantsPDF>>(filename);
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error during parsing \"{0}\".", filename);
                return null;
            }
        }

        private Image ReadImage(string card, string cardType, string user)
        {
            if (!string.IsNullOrEmpty(card))
            {
                string nameCover = string.Empty;
                if (user != "General")
                {
                    nameCover = string.Format("{0}.jpg", card);
                }
                else
                {
                    nameCover = string.Format("{0}.jpg", card);
                }

                string fileCover = AzureDownloadToFile(nameCover, "recipecards");
                try
                {
                    Image img = Image.GetInstance(fileCover);
                    img.SetAbsolutePosition(0, 0);
                    return img;
                }
                catch (Exception ex)
                {
                    LogError(ex, "");
                    return null;
                }
            }
            else
            {
                LogError("Type {0}, {1} is empty.", user, cardType);
                return null;
            }
        }

        private Image ReadImageRecipe(string card, string user)
        {
            if (!string.IsNullOrEmpty(card))
            {
                string nameCover = string.Empty;
                //if (user != "General")
                //{
                //    nameCover = string.Format("{0}-{1}.jpg", user, card);
                //}
                //else
                //{
                    nameCover = string.Format("{0}.jpg", card);
                //}
                string fileCover = AzureDownloadToFile(nameCover, "recipecards"); ;
                try
                {
                    Image img = Image.GetInstance(fileCover);
                    img.SetAbsolutePosition(0, 0);
                    return img;
                }
                catch (Exception ex)
                {
                    LogError(ex, "");
                    return null;
                }
            }
            else
            {
                LogError("Type {0}, recipe is empty.", user);
                return null;
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

        private bool AzureUploadFromFile(string localFileName, string fileName)
        {
            try
            {
                string blobStore = "mealplans";

                CloudBlobContainer container = blobClient.GetContainerReference(blobStore);
                CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
                blob.UploadFromFile(localFileName, FileMode.OpenOrCreate);
                try
                {
                    //File.Delete(localFileName);
                }
                catch(Exception ex)
                {
                    LogError(ex, "Error delete temporary file \"{0}\".", localFileName);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error upload file \"{0}\".", fileName);
                return false;
            }
        }
    }
}