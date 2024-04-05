using System.Text.RegularExpressions;

namespace GridStrip6m
{
    public partial class Form1 : Form
    {
        readonly HashSet<string> grids = new HashSet<string>();
        readonly Dictionary<string,int> calls = new Dictionary<string,int>();
        readonly Dictionary<string, string> callFirstHeard = new Dictionary<string, string>();
        readonly Dictionary<string, string> callLastHeard = new Dictionary<string, string>();
        readonly Dictionary<string, string> output = new Dictionary<string, string>();
        public Form1()
        {
            InitializeComponent();
        }

        static string? Myreadline(StreamReader reader, StreamWriter writer)
        {
            if (reader == null)
            { 
                MessageBox.Show("Null reader shouild not be possible??");
                return null;
            }
            String? s = reader.ReadLine();
            if (s == null) return null;
            while (s != null && (s.Length==0 || s[0] == '#' || s.Length < 2))
            {
                if (s.Length > 2) writer.WriteLine(s);
                s = reader.ReadLine();
                if (reader.EndOfStream) return null;
            }
            return s;
        }
        
        string? IsGrid(string token)
        {
            var grid = Regex.Split(token,"{\\A(?![Rr]{2}73)[A-Ra-r]{2}[0-9]{2}([A-Xa-x]{2}){0,1}\\z}");
            if (grid != null && grid.Length > 0 && grid[0].Length == 4)
            {
                if (grid[0].Equals("RR73")) return null;
                if (grid[0].StartsWith("R-")) return null;
                if (grid[0].StartsWith("R+")) return null;
                if (grid[0][0] < 'A') return null;
                if (grid[0][0] > 'R') return null;
                if (grid[0][1] < 'A') return null;
                if (grid[0][1] > 'X') return null;
                if (grid[0][2] < '0') return null;
                if (grid[0][2] > '9') return null;
                if (grid[0][3] < '0') return null;
                if (grid[0][3] > '9') return null;
                if (grid[0].Length == 4) return grid[0];
            }
            return null;
        }

        private void GetGridsFFMA(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            string? s;
            richTextBox1.AppendText("Processing " + filename + "\n");
            this.Update();
            String? grid;
            int nlines = 0;
            saveFileDialog1.Filter = "FFMA output file|*.csv";
            StreamWriter? writer = null;
            Boolean write = false;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                writer = new StreamWriter(saveFileDialog1.FileName);
                write = true;
            }
            while (writer != null && !reader.EndOfStream)
            {
                s = Myreadline(reader,writer);
                if (s == null)
                {
                    break;
                }
                if (reader.EndOfStream) break;
                ++nlines;
                if (nlines % 100 == 0)
                {
                    label1.Text = "Lines: " + nlines;
                    Update();
                }
                if (s == null || s.Length < 48) 
                    continue;
                var tokensLine = s.Split(' ');
                Double freq;
                if (tokensLine.Length > 4)
                {
                    Double.TryParse(tokensLine[4], out freq);
                    if (freq < 50 || freq > 51)
                    {
                       continue;
                    }
                }
                var msg = s.Substring(48);
                var tokensMsg = msg.Split(' ');
                if (tokensMsg.Length == 3) grid = IsGrid(tokensMsg[2]);
                else if (tokensMsg.Length == 4) grid = IsGrid(tokensMsg[3]);
                else grid = null;
                string? callsign = null;
                if (grid != null)
                {
                    if (tokensMsg.Length == 4)
                    {
                        if (tokensMsg[2].Equals("R")) callsign = tokensMsg[1];
                        else callsign = tokensMsg[2];
                    }
                    else callsign = tokensMsg[1];
                    if (callsign != null && !callsign.Contains("<"))
                    {
                        if (calls.ContainsKey(callsign))
                        {
                            callLastHeard[callsign] = tokensLine[0];
                        }
                        else
                        {
                            calls.Add(callsign, 0);
                            callFirstHeard[callsign] = tokensLine[0];
                            callLastHeard[callsign] = tokensLine[0];
                        }
                        var key = grid + "," + callsign;
                        string heard = tokensLine[0];
                        string firstHeard = heard;
                        if (!callFirstHeard.ContainsKey(key))
                        {
                            callFirstHeard.Add(key, heard);
                            callLastHeard.Add(key, heard);
                        }
                        else
                        {
                            firstHeard = callFirstHeard[key];
                        }
                        if (grids.Add(key))
                        {
                            calls[callsign]++;
                        }
                        var outputLine = key + "," + calls[callsign] + "," + firstHeard + "," + heard;
                        output[key] = outputLine;
                        //writer.WriteLine(key + "," + calls[callsign] + "," + firstHeard + "," + heard);
                        //writer.BaseStream.FlushAsync().Wait();
                    }
                }
            }
            if (writer != null)
            {
                foreach (KeyValuePair<string, string> k in output)
                {
                    writer.WriteLine(k.Value);

                }
            }
            label1.Text = "Lines: " + nlines;

            if (write && writer != null) writer.Close();
            reader.Close();
            richTextBox1.AppendText("Written to " + saveFileDialog1.FileName + "\n");
            richTextBox1.AppendText("Unique 6M grid+call: " + output.Count() + "\n");
        }
        
        private void Button1_Click(object sender, EventArgs e)
        {
            label1.Visible = true;
            Cursor.Current = Cursors.WaitCursor;
            button1.Enabled = false;
            this.Refresh();
            var path = Environment.GetEnvironmentVariable("LOCALAPPDATA") + "\\WSJT-X";
            openFileDialog1.InitialDirectory = path;
            openFileDialog1.Filter = "ALL.TXT files|*ALL.txt";
            openFileDialog1.Title = "ALL.TXT File to read";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.AppendText(openFileDialog1.FileName+"\n");
                GetGridsFFMA(openFileDialog1.FileName);
            }
            button1.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
