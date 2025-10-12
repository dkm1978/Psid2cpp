using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Psid2cpp
{
    public partial class Form1 : Form
    {
        byte[] bufor, tmp;
        string plik = "";
        long rozmiar = 0;
        int maxtab = 10;
        int sid_version;
        int sid_data_offset;
        int sid_load;
        int sid_init;
        int sid_play;
        int sid_songs;
        int sid_startSong;
        string sid_name;
        byte sid_clock;
        int sid_flags;

        

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 2;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    maxtab = 10;
                    break;
                case 1:
                    maxtab = 15;
                    break;
                case 2:
                    maxtab = 20;
                    break;

                default:
                    break;
            }
            textBox1.AppendText("Layout selected : " + comboBox1.SelectedItem + "\r\n");

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                plik = openFileDialog1.FileName;

                if (File.Exists(plik))
                {
                    textBox1.Clear();
                    FileInfo rozmiar = new FileInfo(plik);
                    sid_name = rozmiar.Name;
                    textBox1.AppendText("File : " + rozmiar.Name + " size [ " + rozmiar.Length.ToString() + " ] bytes\r\n\r\n");
                    bufor = File.ReadAllBytes(plik);
                    if (rozmiar.Length<=65536+127)
                    {
                        
                       // if ((bufor[0] == 0x50) && (bufor[1] == 0x53) && (bufor[2] == 0x49) && (bufor[3] == 0x44))
                       if (BitConverter.ToUInt32(bufor, 0)==0x44495350)
                        {
                            parse_header();
                        }
                        else
                        {
                            textBox1.AppendText("This is not a SID file !!!\r\n");
                            button2.Enabled = false;
                            //textBox1.AppendText((BitConverter.ToInt32(bufor, 0)).ToString());
                        }
                    }
                    else
                    {
                        textBox1.AppendText("The file is too large!!!\r\n");
                    }

                    
                    
                }

            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            generate_hex();
        }

        public void generate_hex()
        {
            int tab = 0;
            int ile = 0;
            textBox2.AppendText("/* Plik utowrzony za pomocą programu Psid2cpp by ProteusPL */\r\n\r\n");
            textBox2.AppendText("/* "+sid_name+" */\r\n\r\n");
            textBox2.AppendText("const int loader=0x" + sid_load.ToString("X4")+";\r\n");
            textBox2.AppendText("const int initm=0x" + sid_init.ToString("X4") + ";\r\n");
            textBox2.AppendText("const int playm=0x" + sid_play.ToString("X4") + ";\r\n\r\n");
            textBox2.AppendText("const int startm=0x" + (sid_startSong-1).ToString("X4") + ";\r\n\r\n");
            textBox2.AppendText("const char music[" + (bufor.Length-1).ToString()+"]={\r\n");
            for (int i = sid_data_offset+2; i < bufor.Length; i++)
            {
                tmp = new byte[1];
                tmp[0] = bufor[i];
                textBox2.AppendText("0x" + BitConverter.ToString(tmp));
                if (i == bufor.Length - 1) { } else { textBox2.AppendText(","); }
                tab++;
                ile++;

                if (tab == maxtab) { textBox2.AppendText("\r\n"); tab = 0; }
            }
           // textBox2.AppendText("\r\n};\r\n /* Rozmiar bufora : " + bufor.Length + " bajtów. Ile=" + ile.ToString() + " */\r\n");
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                TextWriter tw = new StreamWriter(saveFileDialog1.FileName);
                // write a line of text to the file
                tw.WriteLine(textBox2.Text);
                // close the stream
                tw.Close();
            }

        }
        public void parse_header()
        {
            sid_version = bufor[4] * 256 + bufor[5];
            sid_data_offset= bufor[6] * 256 + bufor[7];
            sid_load = bufor[sid_data_offset+1] * 256 + bufor[sid_data_offset];
            sid_init = bufor[10] * 256 + bufor[11];
            sid_play = bufor[12] * 256 + bufor[13];
            sid_songs = bufor[14] * 256 + bufor[15];
            sid_startSong = bufor[16] * 256 + bufor[17];

            textBox1.AppendText("Version \t\t:" + sid_version.ToString()+"\r\n");
            textBox1.AppendText("Offset \t\t: $" + sid_data_offset.ToString("X4")+"\r\n");
            textBox1.AppendText("Load \t\t: $" + sid_load.ToString("X4") + "\r\n");
            textBox1.AppendText("Init \t\t: $" + sid_init.ToString("X4") + "\r\n");
            textBox1.AppendText("Play \t\t: $" + sid_play.ToString("X4") + "\r\n");
            textBox1.AppendText("Songs \t\t: $" + sid_songs.ToString("X4") + "\r\n");
            textBox1.AppendText("Start Song \t: $" + sid_startSong.ToString("X4") + "\r\n");

            if (sid_version == 2)
            {
                sid_flags =  bufor[76]*256+bufor[0x77];
                //textBox1.AppendText("Flags byte :" + sid_flags.ToString());
                if (!IsBitSet(sid_flags, 0))
                {
                    textBox1.AppendText("Butin player in this file. Clock : ");
                }
                if (IsBitSet(sid_flags,2) && !IsBitSet(sid_flags,3))
                {
                    textBox1.AppendText("PAL.\r\n");
                    sid_clock = 1;
                }
                if (!IsBitSet(sid_flags, 2) && IsBitSet(sid_flags, 3))
                {
                    textBox1.AppendText("NTSC.\r\n");
                    sid_clock = 0;
                }
                if (IsBitSet(sid_flags, 2) && IsBitSet(sid_flags, 3))
                {
                    textBox1.AppendText("PAL & NTSC.\r\n");
                    sid_clock = 2;
                }
                if (!IsBitSet(sid_flags, 2) && !IsBitSet(sid_flags, 3))
                {
                    textBox1.AppendText("Unknown.\r\n");
                    sid_clock = 2;
                }
            }
            button2.Enabled = true;
        }

        bool IsBitSet(int b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
