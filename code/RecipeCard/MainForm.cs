using CreatePdfLibrary;
using RecipeCardLibrary;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RecipeCard
{
    public partial class MainForm : Form
    {
        private static string _fileFolder = Environment.CurrentDirectory;

        public static MainForm Instance { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            Instance = this;
            txtJSON.iTalkRTB.ScrollBars = RichTextBoxScrollBars.None;
            txtCfg.iTalkRTB.ScrollBars = RichTextBoxScrollBars.None;
            //txtID.iTalkRTB.ScrollBars = RichTextBoxScrollBars.None;
            txtJSONPDF.iTalkRTB.ScrollBars = RichTextBoxScrollBars.None;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Focus();
            txtCfg.Text = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Application.ExecutablePath)).Parent.Parent.FullName, "cfg.json"); ;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                btnStart.Enabled = false;
                ClearLog();

                string JSONFile = txtJSON.Text;
                string id = txtID.Text;
                string cfgFile = txtCfg.Text;

                var controller = new RecipeSavantsController();
                controller.Message += Log;

                Task.Factory.StartNew(() =>
                {
                    controller.Run(JSONFile, id, cfgFile);
                }).ContinueWith((task) =>
                {
                    btnStart.Enabled = true;
                    controller.Message -= Log;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), true);
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Log (object sender, MessageEventArgs e)
        {
            Log(e.Msg, e.IsError, e.EndLine);
        }

        public void Log(object sender, MessagePdfEventArgs e)
        {
            Log(e.Msg, e.IsError, e.EndLine);
        }

        public void Log(string message, bool isError = false, bool endLine = true)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string, bool, bool>(Log), new object[] { message, isError, endLine });
                return;
            }

            Instance.txtOutput.SelectionColor = isError ? Color.Red : Color.Black;
            Instance.txtOutput.AppendText(message);
            if (endLine && !message.EndsWith(Environment.NewLine))
            {
                Instance.txtOutput.AppendText(Environment.NewLine);
            }
        }

        public void ClearLog()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(ClearLog));
                return;
            }

            Instance.txtOutput.Text = string.Empty;
        }

        private static string GetFilename(string filter)
        {
            var dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Filter = filter;
            dlg.InitialDirectory = _fileFolder;

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return null;
            }

            _fileFolder = Path.GetDirectoryName(dlg.FileName);
            return dlg.FileName;
        }

        private void btnJSON_Click(object sender, EventArgs e)
        {
            var filename = GetFilename("JSON file (*.*)|*.*");
            if (filename != null)
            {
                txtJSON.Text = filename;
            }
        }

        private void btnId_Click(object sender, EventArgs e)
        {
            txtID.Text = string.Empty;
        }

        private void btnJSONPDF_Click(object sender, EventArgs e)
        {
            var filename = GetFilename("JSON file (*.*)|*.*");
            if (filename != null)
            {
                txtJSONPDF.Text = filename;
            }
        }

        private void btnCfg_Click(object sender, EventArgs e)
        {
            var filename = GetFilename("JSON file (*.*)|*.*");
            if (filename != null)
            {
                txtCfg.Text = filename;
            }
        }

        private void btnStartPDF_Click(object sender, EventArgs e)
        {
            try
            {
                groupBoxPDF.Enabled = false;
                ClearLog();
                string json = txtJSONPDF.Text;

                var controller = new CreatePDFController();
                controller.Message += Log;
                Task.Factory.StartNew(() =>
                {                    
                    controller.Run(json);
                }).ContinueWith((task) =>
                {
                    controller.Message -= Log;
                    groupBoxPDF.Enabled = true;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), true);
            }
        }
    }
}
