using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PictureToBinary
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Main Code

        // Public Vars
        public int ImagesTotal = 0;
        public string[] PhotosLoc = { "" };
        public string SaveLoc = "";

        // Get Images Folder
        private void Button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            OpenFileDialog x = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Jpg Files|*.jpg|Png Files|*.png",
                Title = "Select Photos To Convert"
            };
            x.ShowDialog();
            PhotosLoc = x.FileNames;

            int PhotoCount = 0;

            // Sort Textbox
            foreach (string s in x.FileNames)
            {
                if (textBox1.Text == "")
                {
                    // Get Dir Count
                    PhotoCount++;
                    textBox1.Text = s;
                }
                else
                {
                    // Get Dir Count
                    PhotoCount++;
                    textBox1.Text = s + ", " + textBox1.Text;
                }
            }

        }

        // Export File Location
        private void Button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Txt Files|*.txt",
                Title = "Converted Data Save Name"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SaveLoc = dialog.FileName;
                textBox2.Text = dialog.FileName;
            }
        }

        // Convert Images Button
        private void Button1_Click(object sender, EventArgs e)
        {

            // Check If Locations Are Populated
            if (textBox1.Text == "")
            {
                MessageBox.Show("ERROR: Please Add Some Photos To Convert!");
                return;
            }
            else if (textBox2.Text == "")
            {
                MessageBox.Show("ERROR: Please Add Some Photos To Convert!");
                return;
            }
            else
            {

                // Reset ProgressBar
                progressBar1.Value = 0;

                // Setup Progress Bar
                progressBar1.Step = 1;
                progressBar1.Maximum = textBox1.Text.Split(',').Length;

                // Convert Images
                ConvertToBinary(PhotosLoc);

                // Job Finished
                MessageBox.Show("Conversion Has Completed!");

            }

        }

        // Convert Images To Binary
        public void ConvertToBinary(string[] files)
        {
            try
            {

                // Delete Old Text File if Exists
                if (File.Exists(textBox2.Text))
                {
                    File.Delete(textBox2.Text);
                }

                string PhotoData = "";

                // For Each Image
                foreach (string image in files)
                {

                    // Clear String
                    PhotoData = "";

                    // Get Image's Color Top Left To Bottom Right
                    Bitmap img = new Bitmap(image);

                    // Rotate Image Counter Clockwise 90
                    // img.RotateFlip(RotateFlipType.Rotate180FlipX);

                    for (int j = 0; j < img.Height; j++)
                    {
                        for (int i = 0; i < img.Width; i++)
                        {

                            Color pixel = img.GetPixel(i, j);

                            // Get Color
                            if (pixel.Name == "ffffffff" || pixel.Name == "fffefefe" || pixel.Name == "fffdfdfd")
                            {
                                // White
                                PhotoData = PhotoData + "1";
                            }
                            else if (pixel.Name == "ff000000" || pixel.Name == "ff010101" || pixel.Name == "ff020202")
                            {
                                // Black
                                PhotoData = PhotoData + "0";
                            }
                            else
                            {
                                // Error Color
                                PhotoData = PhotoData + "2";
                                MessageBox.Show("ERROR: Color: " + pixel.Name + " | Location: " + i + "," + j + " | Image: " + image);
                            }
                        }
                    }

                    // Wright Data To File
                    using (System.IO.StreamWriter sw = System.IO.File.AppendText(textBox2.Text))
                    {
                        //write my text
                        sw.WriteLine(PhotoData);
                        sw.Close();
                    }

                    // Process Progressbar
                    progressBar1.PerformStep();

                }

            }
            catch (Exception)
            {
                return;
            }

        }

        #endregion

    }

    static class Helper
    {
        public static string GetUntilOrEmpty(this string text, string stopAt = "-")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }

    }

}
