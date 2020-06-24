using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Hexagon
{
    public partial class Form1 : Form
    {
        private enum EditorState
        {
            Text,
            Hex,
            Binary,
            Octal
        }
        public string FilePath = "";
        public string FileName = "";
        private string TextHash = "";
        private EditorState EditorMode = EditorState.Text;

        public Form1()
        {
            InitializeComponent();
        }
        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form()
                {
                    Width = 500,
                    Height = 150,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };
                Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
                TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
                Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Editor.Font = new Font(FontFamily.GenericMonospace, Editor.Font.Size);
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                if (File.Exists(args[1]))
                {
                    FilePath = args[1];
                    string[] file = FilePath.Split('\\');
                    FileName = file[file.Length - 1];
                    string[] extArray = FileName.Split('.');
                    string ext = extArray[extArray.Length - 1].ToLower();
                    string[] bin = { "jpg", "png", "gif", "bmp", "tiff", "psd", "mp4", "mkv", "avi", "mov", "mpg", "vob", "mp3", "aac", "wav", "flac", "ogg", "mka", "wma", "pdf", "doc", "xls", "ppt", "docx", "odt", "zip", "rar", "7z", "tar", "iso", "mdb", "accde", "frm", "sqlite", "exe", "dll", "so", "class" };
                    if (bin.Contains(ext))
                    {
                        Editor.Text = BitConverter.ToString(File.ReadAllBytes(FilePath)).Replace("-", " ");
                        EditorMode = EditorState.Hex;
                    }
                    else
                    {
                        switch (EditorMode)
                        {
                            case EditorState.Text:
                                Editor.Text = File.ReadAllText(FilePath);
                                break;
                            case EditorState.Hex:
                                Editor.Text = BitConverter.ToString(File.ReadAllBytes(FilePath)).Replace("-", " ");
                                break;
                            case EditorState.Binary:
                                Editor.Text = string.Join(" ", File.ReadAllBytes(FilePath).Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));
                                break;
                        }
                    }
                    Output.Text = "[ File Loaded ]";
                    this.Text = "Hexagon - [ " + FileName + " ]";
                    TextHash = SHA256_Compute(Editor.Text);
                }
            }
        }
        private string SHA256_Compute(string str)
        {
            SHA256 mySHA256 = SHA256.Create();
            return BitConverter.ToString(mySHA256.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", "");
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (TextHash != SHA256_Compute(Editor.Text))
                {
                    this.Text = "Hexagon - [ " + FileName + " ] - Not Yet Saved!";
                }
                else
                {
                    ActiveForm.Text = "Hexagon - [ " + FileName + " ]";
                }
                switch (EditorMode)
                {
                    case EditorState.Text:
                        label1.Text = "Word count: " + Editor.Text.Split(' ').Length + ", Char count: " + Editor.Text.Length;
                        break;
                    case EditorState.Hex:
                        label1.Text = "Byte count: " + Editor.Text.Replace(" ", "").Length / 2;
                        break;
                    case EditorState.Binary:
                        int bytes = Editor.Text.Replace(" ", "").Length;
                        label1.Text = "Byte count: " + bytes / 8 + ", Bit count: " + bytes;
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    saveAsToolStripMenuItem_Click(sender, e);
                }
                else
                {
                    switch (EditorMode)
                    {
                        case EditorState.Text:
                            File.WriteAllText(FilePath, Editor.Text);
                            break;
                        case EditorState.Hex:
                            var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
                            string hex = Editor.Text.Replace(" ", "");
                            byte[] bytes = Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
                            fs.Write(bytes, 0, bytes.Length);
                            fs.Close();
                            break;
                        case EditorState.Binary:
                            string input = Editor.Text.Replace(" ", "");
                            int numOfBytes = input.Length / 8;
                            byte[] byteArray = new byte[numOfBytes];
                            for (int i = 0; i < numOfBytes; ++i)
                            {
                                byteArray[i] = Convert.ToByte(input.Substring(8 * i, 8), 2);
                            }
                            File.WriteAllBytes(FilePath, byteArray);
                            break;
                    }
                    Form1.ActiveForm.Text = "Hexagon - [ " + FileName + " ]";
                    Output.Text = "[ File Saved ]";
                }
            }
            catch (ArgumentException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error:");
            }
            TextHash = SHA256_Compute(Editor.Text);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilePath = "";
            FileName = "";
            Form1.ActiveForm.Text = "Hexagon";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FilePath = openFileDialog1.FileName;
                string[] file = FilePath.Split('\\');
                FileName = file[file.Length - 1];
                string[] extArray = FileName.Split('.');
                string ext = extArray[extArray.Length - 1].ToLower();
                string[] bin = { "jpg", "png", "gif", "bmp", "tiff", "psd", "mp4", "mkv", "avi", "mov", "mpg", "vob", "mp3", "aac", "wav", "flac", "ogg", "mka", "wma", "pdf", "doc", "xls", "ppt", "docx", "odt", "zip", "rar", "7z", "tar", "iso", "mdb", "accde", "frm", "sqlite", "exe", "dll", "so", "class" };
                if (bin.Contains(ext))
                {
                    Editor.Text = BitConverter.ToString(File.ReadAllBytes(FilePath)).Replace("-", " ");
                    EditorMode = EditorState.Hex;
                }
                else
                {
                    switch (EditorMode)
                    {
                        case EditorState.Text:
                            Editor.Text = File.ReadAllText(FilePath);
                            break;
                        case EditorState.Hex:
                            Editor.Text = BitConverter.ToString(File.ReadAllBytes(FilePath)).Replace("-", " ");
                            break;
                        case EditorState.Binary:
                            Editor.Text = string.Join(" ", File.ReadAllBytes(FilePath).Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));
                            break;
                    }
                }
                Output.Text = "[ File Loaded ]";
                Form1.ActiveForm.Text = "Hexagon - [ " + FileName + " ]";
                TextHash = SHA256_Compute(Editor.Text);
            }
            else
            {
                Output.Text = "[ File Not Loaded ]";
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Save File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                FilePath = saveFileDialog1.FileName;
                string[] file = FilePath.Split('\\');
                FileName = file[file.Length - 1];
                Form1.ActiveForm.Text = "Hexagon - [ " + FileName + " ]";
                switch (EditorMode)
                {
                    case EditorState.Text:
                        File.WriteAllText(FilePath, Editor.Text);
                        break;
                    case EditorState.Hex:
                        var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
                        string hex = Editor.Text.Replace(" ", "");
                        byte[] bytes = Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();
                        break;
                    case EditorState.Binary:
                        int numOfBytes = Editor.Text.Length / 8;
                        byte[] byteArray = new byte[numOfBytes];
                        for (int i = 0; i < numOfBytes; ++i)
                        {
                            byteArray[i] = Convert.ToByte(Editor.Text.Substring(8 * i, 8), 2);
                        }
                        File.WriteAllBytes(FilePath, byteArray);
                        break;

                }
                Output.Text = "[ File Saved ]";
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                File.Delete(FilePath);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Can not delete file.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error:");
            }
        }
        private void textToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    Editor.Text = File.ReadAllText(FilePath);
                }
                else
                {
                    switch (EditorMode)
                    {
                        case EditorState.Hex:
                            string hex = Editor.Text.Replace(" ", "");
                            Editor.Text = Encoding.ASCII.GetString(Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray());
                            break;
                        case EditorState.Binary:
                            string input = Editor.Text.Replace(" ", "");
                            int numOfBytes = input.Length / 8;
                            byte[] byteArray = new byte[numOfBytes];
                            for (int i = 0; i < numOfBytes; ++i)
                            {
                                byteArray[i] = Convert.ToByte(input.Substring(8 * i, 8), 2);
                            }
                            Editor.Text = Encoding.ASCII.GetString(byteArray);
                            break;
                    }
                }
                EditorMode = EditorState.Text;
            }
            catch (Exception)
            {

            }
        }

        private void hexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(FilePath))
            {
                Editor.Text = BitConverter.ToString(File.ReadAllBytes(FilePath)).Replace("-", " ");
            }
            else
            {
                switch (EditorMode)
                {
                    case EditorState.Text:
                        Editor.Text = BitConverter.ToString(Encoding.ASCII.GetBytes(Editor.Text)).Replace("-", " ");
                        break;
                    case EditorState.Binary:
                        string input = Editor.Text.Replace(" ", "");
                        int numOfBytes = input.Length / 8;
                        byte[] byteArray = new byte[numOfBytes];
                        for (int i = 0; i < numOfBytes; ++i)
                        {
                            byteArray[i] = Convert.ToByte(input.Substring(8 * i, 8), 2);
                        }
                        Editor.Text = BitConverter.ToString(byteArray).Replace("-", " ");
                        break;
                }
            }
            EditorMode = EditorState.Hex;
        }

        private void binaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    Editor.Text = string.Join(" ", File.ReadAllBytes(FilePath).Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));
                }
                else
                {
                    switch (EditorMode)
                    {
                        case EditorState.Text:
                            Editor.Text = string.Join(" ", Encoding.ASCII.GetBytes(Editor.Text).Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));
                            break;
                        case EditorState.Hex:
                            string hex = Editor.Text.Replace(" ", "");
                            byte[] bytes = Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
                            Editor.Text = string.Join(" ", bytes.Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));
                            break;
                    }
                }
                EditorMode = EditorState.Binary;
            }
            catch (Exception)
            {
            }
        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Title = "Create File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                FilePath = saveFileDialog1.FileName;
                string[] file = FilePath.Split('\\');
                FileName = file[file.Length - 1];
                File.Create(saveFileDialog1.FileName);
                Output.Text = "[ File Saved ]";
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.SelectAll();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Undo();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Paste();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Copy();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Cut();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            int a = Editor.SelectionLength;
            Editor.Text = Editor.Text.Remove(Editor.SelectionStart, a);
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string findText = Prompt.ShowDialog("Find Text", "");
            int index = Editor.Find(findText, RichTextBoxFinds.MatchCase);
            if (index > 0)
            {
                Editor.Select(index, findText.Length);
            }
            else
            {
                index = Editor.Find(findText);
                if (index > 0)
                {
                    Editor.Select(index, findText.Length);
                }
                else
                {
                    MessageBox.Show("Could not find that text", "Find Text");
                }
            }
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text = Prompt.ShowDialog("Replace Text", "");
            string replace = Prompt.ShowDialog("Replace With", "");
            Editor.Text = Editor.Text.Replace(text, replace);
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                switch (EditorMode)
                {
                    case EditorState.Text:
                        Editor.Text = File.ReadAllText(FilePath);
                        break;
                    case EditorState.Hex:
                        Editor.Text = BitConverter.ToString(File.ReadAllBytes(FilePath)).Replace("-", " ");
                        break;
                    case EditorState.Binary:
                        Editor.Text = string.Join(" ", File.ReadAllBytes(FilePath).Select(x => Convert.ToString(x, 2).PadLeft(8, '0')));
                        break;
                }
            }
            catch
            {

            }
        }

        private void greenOnBlackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.BackColor = Color.Black;
            Editor.ForeColor = Color.Lime;
        }

        private void whiteOnBlackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.BackColor = Color.White;
            Editor.ForeColor = Color.Black;
        }

        private void redOnBlackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.BackColor = Color.Black;
            Editor.ForeColor = Color.Red;
        }

        private void yellowOnBlackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.BackColor = Color.Black;
            Editor.ForeColor = Color.Yellow;
        }

        private void orangeOnBlackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.BackColor = Color.Black;
            Editor.ForeColor = Color.Orange;
        }

        private void invertThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Color temp = Editor.BackColor;
            Editor.BackColor = Editor.ForeColor;
            Editor.ForeColor = temp;
        }
    }
}