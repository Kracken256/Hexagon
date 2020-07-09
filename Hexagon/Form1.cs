using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Hexagon
{
    public partial class Form1 : Form
    {
        private enum EditorState
        {
            Text,
            Hex,
            Binary
        }
        private enum SyntaxType
        {
            Disabled,
            C,
            XML,
            CSS,
            JavaScript,
            VB
        }
        private class SyntaxClass
        {
            public MatchCollection Match;
            public Color Color;
            public SyntaxClass(MatchCollection match, Color color)
            {
                Match = match;
                Color = color;
            }
        }
        public string FilePath = "";
        public string FileName = "";
        private SyntaxType syntaxType = SyntaxType.Disabled;
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
                    string[] exts = FileName.Split('.');
                    string ext = exts[exts.Length - 1].ToLower();
                    string[] bin = { "jpg", "png", "gif", "bmp", "tiff", "psd", "mp4", "mkv", "avi", "mov", "mpg", "vob", "mp3", "aac", "wav", "flac", "ogg", "mka", "wma", "pdf", "doc", "xls", "ppt", "docx", "odt", "zip", "rar", "7z", "tar", "iso", "mdb", "accde", "frm", "sqlite", "exe", "dll", "so", "class" };
                    if (bin.Contains(ext))
                    {
                        Editor.Text = BitConverter.ToString(File.ReadAllBytes(FilePath)).Replace("-", " ");
                        EditorMode = EditorState.Hex;
                    }
                    switch (ext)
                    {
                        case "cs":
                        case "cpp":
                        case "c":
                            syntaxType = SyntaxType.C;
                            break;
                        case "vb":
                            syntaxType = SyntaxType.VB;
                            break;
                        case "js":
                            syntaxType = SyntaxType.JavaScript;
                            break;
                        case "xml":
                        case "html":
                            syntaxType = SyntaxType.XML;
                            break;
                    }
                    this.Text = "Hexagon - [ " + FileName + " ]";
                }
            }
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            List<SyntaxClass> matches = new List<SyntaxClass>();
            switch (syntaxType)
            {
                case SyntaxType.C:
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"(\s?([A-Z][A-Z 0-9 a-z]*)\s\b)"), Color.FromArgb(58, 214, 136)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\b(Console|null|void|char|int|string|byte|long|float|double|decimal|unsigned|signed|object|Exception)\b"), Color.Cyan));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"[\+\-\*\/\%\=\>\<\&\|\!^]"), Color.Gray));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"([a-z0-9A-Z]*)\."), Color.FromArgb(33, 173, 98)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\b([a-z0-9_A-Z]*)\(.*\)"), Color.FromArgb(197, 209, 61)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"([a-z0-9A-Z]*)\."), Color.FromArgb(33, 173, 98)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\b(var|protected|override|this|enum|public|private|partial|static|namespace|class|using|foreach|in|for|while|new|else|return|if|struct|internal|get|set|switch|case|include)\b"), Color.FromArgb(87, 107, 230)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"(\/\/.+?$|\/\*.+?\*\/)", RegexOptions.Multiline), Color.Green));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, "\".+?\""), Color.Brown));
                    break;

                case SyntaxType.JavaScript:
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\b(let|var|const|break|case|class|catch|continue|debugger|default|delete|do|else|finally|for|function|if|in|instanceof|new|return|switch|this|throw|try|typeof|var|void|while|with)\b"), Color.FromArgb(87, 107, 230)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\b(undefined, null)\b"), Color.Cyan));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"[\+\-\*\/\%\=\>\<\&\|\!^]"), Color.Gray));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"([a-z0-9A-Z]*)\."), Color.FromArgb(33, 173, 98)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\b([a-z0-9_A-Z]*)\(.*\)"), Color.FromArgb(197, 209, 61)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"(\/\/.+?$|\/\*.+?\*\/)", RegexOptions.Multiline), Color.Green));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, "\".+?\""), Color.Brown));
                    break;

                case SyntaxType.CSS:
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"([a-z0-9A-Z_ @./#&+:()-]*)(\s*)\{"), Color.FromArgb(247, 201, 92)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"(\s*)([a-z0-9A-Z_@./#&+-]*)\:"), Color.FromArgb(99, 209, 255)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\:(\s*)([a-z0-9A-Z_ @./#&+-]*)"), Color.FromArgb(230, 154, 73)));
                    break;

                case SyntaxType.XML:
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\<(.*)\>"), Color.FromArgb(60, 109, 214)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"([a-z0-9A-Z]*)\="), Color.FromArgb(99, 209, 255)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"[\=\<\>]"), Color.Gray));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, "\".+?\""), Color.FromArgb(209, 151, 65)));
                    break;

                case SyntaxType.VB:
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\b(Module|Public|Private|Class|As|Integer|End|Sub|Dim|New|Namespace|Imports|)\b"), Color.FromArgb(87, 107, 230)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, "\".+?\""), Color.Brown));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"[\+\-\*\/\%\=\>\<\&\|\!^]"), Color.Gray));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"([a-z0-9A-Z]*)\."), Color.FromArgb(33, 173, 98)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\b([a-z0-9_A-Z]*)\(.*\)"), Color.FromArgb(197, 209, 61)));
                    matches.Add(new SyntaxClass(Regex.Matches(Editor.Text, @"\'\s?(.*)\n"), Color.Brown));
                    break;
            }

            int originalIndex = Editor.SelectionStart;
            int originalLength = Editor.SelectionLength;
            Color originalColor = Color.White;

            label1.Focus();

            Editor.SelectionStart = 0;
            Editor.SelectionLength = Editor.Text.Length;
            Editor.SelectionColor = originalColor;

            foreach (SyntaxClass s in matches)
            {
                foreach (Match m in s.Match)
                {
                    Editor.SelectionStart = m.Index;
                    Editor.SelectionLength = m.Length;
                    Editor.SelectionColor = s.Color;
                }
            }

            Editor.SelectionStart = originalIndex;
            Editor.SelectionLength = originalLength;
            Editor.SelectionColor = originalColor;

            Editor.Focus();
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
                }
            }
            catch (ArgumentException)
            {

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error:");
            }
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
                switch (ext)
                {
                    case "cs":
                    case "cpp":
                    case "c":
                        syntaxType = SyntaxType.C;
                        break;
                    case "vb":
                        syntaxType = SyntaxType.VB;
                        break;
                    case "js":
                        syntaxType = SyntaxType.JavaScript;
                        break;
                    case "xml":
                    case "html":
                        syntaxType = SyntaxType.XML;
                        break;
                }
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
                Form1.ActiveForm.Text = "Hexagon - [ " + FileName + " ]";
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
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.SelectAll();
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
            panel1.Show();
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
            Editor.BackColor = Color.FromArgb(30, 30, 30);
            Editor.ForeColor = Color.White;
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

        private void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.S:
                        saveToolStripMenuItem1.PerformClick();
                        break;
                    case Keys.Z:
                        redoToolStripMenuItem.PerformClick();
                        break;
                    case Keys.Y:
                        redoToolStripMenuItem.PerformClick();
                        break;
                    case Keys.A:
                        selectAllToolStripMenuItem.PerformClick();
                        break;
                    case Keys.R:
                    case Keys.F:
                        e.Handled = true;
                        if (panel1.Visible == true)
                        {
                            panel1.Hide();
                        }
                        else
                        {
                            panel1.Show();
                        }
                        break;

                }
            }
        }

        private void rodoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Editor.Redo();
        }

        private void hTMLXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxType = SyntaxType.XML;
            Editor.Text += "";
        }

        private void cSSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxType = SyntaxType.CSS;
            Editor.Text += "";
        }

        private void javaScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxType = SyntaxType.JavaScript;
            Editor.Text += "";
        }

        private void cCCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxType = SyntaxType.C;
            Editor.Text += "";
        }

        private void visualBasicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxType = SyntaxType.VB;
            Editor.Text += "";
        }

        private void DisabledSyntaxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntaxType = SyntaxType.Disabled;
            Editor.Text += "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string text = findText.Text;
                string replace = replaceText.Text;
                if (text == "")
                {
                    return;
                }
                Editor.Text = Editor.Text.Replace(text, replace);
            }
            catch
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int index = Editor.Find(findText.Text, RichTextBoxFinds.MatchCase);
            if (index >= 0)
            {
                Editor.Select(index, findText.Text.Length);
            }
            else
            {
                MessageBox.Show("Could not find that text", "Find Text");
            }
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}