using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace PictureSorter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog { SelectedPath = @"C:\Users\ajenk_000\OneDrive\SkyDrive camera roll" };
            var dr = fbd.ShowDialog();

            if (dr.Equals(DialogResult.OK))
            {
                // Get the path selected in the dialog box
                string path = fbd.SelectedPath;
                if (Directory.Exists(path))
                {
                    foreach (var fullyQualifiedFileName in Directory.EnumerateFiles(path))
                    {
                        if (FileIsPicture(fullyQualifiedFileName))
                        {

                            var dateTaken = GetDateTakenFromImage(fullyQualifiedFileName);

                            // Creaste directory name
                            // Files without any date data into their own folder as MinValue
                            var targetFolderName = path + @"\" + dateTaken.Year + "-" + (dateTaken.Month < 10 ? "0" + dateTaken.Month : dateTaken.Month.ToString());
                            var targetFileName = targetFolderName + GetFileName(fullyQualifiedFileName);

                            // Note CreateDirectory checks for existence of directory before creating
                            Directory.CreateDirectory(targetFolderName);
                            if (File.Exists(targetFileName))
                                textBox1.AppendText("File Exists! " + targetFileName + "\r\n");
                            else
                            {
                                // Move file to directory
                                File.Move(fullyQualifiedFileName, targetFileName);
                            }

                        }
                        else
                        {
                            // Log unknown file extension
                            textBox1.AppendText("Found extension: " +
                                (fullyQualifiedFileName.Contains(".") ?
                                fullyQualifiedFileName.Substring(
                                    fullyQualifiedFileName.LastIndexOf(".", StringComparison.Ordinal)
                                    ) + "\r\n"
                                : "None"));
                        }
                    }

                    textBox1.AppendText("Complete!\r\n");
                }
            }
        }

        private static string GetFileName(string fullyQualifiedFileName)
        {
            int finalSlash = fullyQualifiedFileName.LastIndexOf(@"\", StringComparison.Ordinal);
            return fullyQualifiedFileName.Substring(finalSlash);
        }

        public static bool FileIsPicture(string path)
        {
            path = path.ToLower();
            return path.EndsWith(".jpg") ||
                   path.EndsWith(".bmp") ||
                   path.EndsWith(".gif") ||
                   path.EndsWith(".png");
        }

        public static bool FileIsVideo(string path)
        {
            path = path.ToLower();
            return path.EndsWith(".mov") ||
                path.EndsWith(".mp4");
        }

        //we init this once so that if the function is repeatedly called
        //it isn't stressing the garbage man
        private static readonly Regex R = new Regex(":");

        //retrieves the datetime WITHOUT loading the whole image
        public static DateTime GetDateTakenFromImage(string path)
        {
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var myImage = Image.FromStream(fs))
                {
                    System.Drawing.Imaging.PropertyItem propItem = myImage.GetPropertyItem(36867);
                    string dateTaken = R.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                    return DateTime.Parse(dateTaken);
                }
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }
    }
}
