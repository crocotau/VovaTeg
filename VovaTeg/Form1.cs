using Ookii.Dialogs.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using File = System.IO.File;

namespace VovaTeg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static string Translit(string str)
        {
            string[] lat_up = { "A", "B", "V", "G", "D", "E", "Yo", "Zh", "Z", "I", "Y", "K", "L", "M", "N", "O", "P", "R", "S", "T", "U", "F", "Kh", "Ts", "Ch", "Sh", "Shch", "", "Y", "", "E", "Yu", "Ya" };
            string[] lat_low = { "a", "b", "v", "g", "d", "e", "yo", "zh", "z", "i", "y", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch", "", "y", "", "e", "yu", "ya" };
            string[] rus_up = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я" };
            string[] rus_low = { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" };
            for (int i = 0; i <= 32; i++)
            {
                str = str.Replace(rus_up[i], lat_up[i]);
                str = str.Replace(rus_low[i], lat_low[i]);
            }
            return str;
        }

        VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
        
        string[] Gfiles(string path)
        {

            //приём файлов
            return Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Select(p => Path.GetFileName(p))
                .Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                            || s.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)).ToArray();

        }

        public void ChangeFiles()
        {
            label1.Text = "";
            label1.Visible = false;
            label1.BorderStyle = BorderStyle.None;
            dialog.Description = "Выбери папку с музыкой";
            dialog.ShowNewFolderButton = true;
            dialog.UseDescriptionForTitle = true;
            

            

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.SelectedPath;

                var myFiles = Gfiles(path);
                //переименование
                foreach (var VARIABLE in myFiles)
                {
                   File.Move(path.ToString() + '\\'
                                                  + VARIABLE, path.ToString() + '\\'
                                                                              + Translit(VARIABLE.ToString()
                                                                                  .ToUpper().Replace(".MP3", ".mp3")
                                                                                  .Replace(".WAV", ".wav")));
                }
                
                    //await Task.Delay(2000);
                    button2.Enabled = true;
                    button2.Focus();
                    label1.Visible = true;
                    label1.BorderStyle = BorderStyle.None;
                    label1.Text = "Нажми \"Заменить\"";

            }
            else
            {
                label1.Visible = true;
                label1.BorderStyle = BorderStyle.None;
                label1.Text = "Нажми \"Выбрать\"";
            }
        }


        async void ChangeTag()
        {
            progressBar1.Visible = true;
            label1.Text = "";
            label1.Visible = false;
            label1.BorderStyle = BorderStyle.None;
            progressBar1.Value = 0;

            var path = dialog.SelectedPath;

            //приём файлов
            var myFiles = Gfiles(path);

            //очистка тегов
            foreach (var VARIABLE in myFiles)
            {
                File.SetAttributes(path + "\\" + VARIABLE, FileAttributes.Normal);
                var tfile = TagLib.File.Create(path + "\\" + VARIABLE);
                tfile.Tag.Clear();
                tfile.Save();
            }

            int qt = myFiles.Length;
            progressBar1.Maximum = qt;

            while (progressBar1.Value < qt)
            {
                //% на прогресс баре
                int percent = (int)(((double)progressBar1.Value / (double)progressBar1.Maximum) * 100);
                progressBar1.Refresh();
                progressBar1.CreateGraphics().DrawString(percent.ToString() + "%",
                    new Font("Arial", (float)10, FontStyle.Regular),
                    Brushes.Black,
                    new PointF(progressBar1.Width / 2 - 10, progressBar1.Height / 2 - 7));

                await Task.Run(() =>
                {
                    //имя файла => тайтл
                    foreach (var VARIABLE in myFiles)
                    {
                        var tfile = TagLib.File.Create(path + "\\" + VARIABLE);
                        tfile.Tag.Title = tfile.Name.Substring(tfile.Name.LastIndexOf("\\")+1)
                            .Replace(".mp3", "").Replace(".wav", "");

                        //TODO: попробовать удалить из тайтла цифры


                        tfile.Save();
                    }



                    //начальные цифры в именах файлов до точки => номер трека
                    foreach (var VARIABLE in myFiles)
                    {
                        var tfile = TagLib.File.Create(path + "\\" + VARIABLE);
                        var tempString = tfile.Name.Replace(path, "").Remove(0, 1)
                            .Replace(".mp3", "");

                        var v = tempString.Substring(0, tempString.IndexOf("."));
                        tfile.Tag.Track = Convert.ToUInt32(v);

                        tfile.Save();
                    }
                    ////нумерация треков
                    //List<uint> trackList = new List<uint>();
                    //foreach (var VARIABLE in myFiles)
                    //{
                    //    var tfile = TagLib.File.Create(path + "\\" + VARIABLE);
                    //    trackList.Add(tfile.Tag.Track);

                    //    for (uint i = 0; i <= trackList.Count; i++)
                    //    {
                    //        tfile.Tag.Track = i;
                    //    }
                        
                    //    tfile.Save();

                    //}

                });

                progressBar1.Value++;

                if (progressBar1.Value == qt)
                {
                    //await Task.Delay(500);
                    label1.BorderStyle = BorderStyle.None;
                    label1.Visible = true;
                    label1.Text = "Готово!";
                    button1.Focus();
                    button2.Enabled = false;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChangeFiles();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ChangeTag();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            progressBar1.Visible = false;
            button2.Enabled = false;
            label1.Visible = true;
            label1.BorderStyle = BorderStyle.None;
            label1.Text = "Нажми \"Выбрать\"";
        }
    }
}