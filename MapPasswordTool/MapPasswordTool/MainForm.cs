using GBX.NET.LZO;
using GBX.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GBX.NET.Engines.Game;
using Color = GBX.NET.Color;

namespace MapPasswordTool
{
    public partial class MainForm : Form
    {
        private CGameCtnChallenge _loadedMap;
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            //dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            dialog.Filter = "GBX files (*.gbx)|*.gbx|All files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Invalid file path selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            var path = dialog.FileName;

            try
            {
                Gbx.LZO = new MiniLZO();
                _loadedMap = Gbx.ParseNode<CGameCtnChallenge>(path);

                nameTextBox.Text = _loadedMap.MapName;
                nicknameTextBox.Text = _loadedMap.AuthorNickname;
                blocksTextBox.Text = _loadedMap.Blocks.Count.ToString();
                crcTextBox.Text = _loadedMap.Crc32.ToString();

                if (_loadedMap.HashedPassword.HasValue && _loadedMap.HashedPassword.Value != UInt128.Zero)
                {
                    presentTextBox.Text = "Yes";
                    presentTextBox.ForeColor = System.Drawing.Color.Red;
                    hashTextBox.Text = Utils.ByteArrayToString(_loadedMap.HashedPassword.Value.GetBytes());
                    removePasswordButton.Enabled = true;
                }
                else
                {
                    presentTextBox.Text = "No";
                    presentTextBox.ForeColor = System.Drawing.Color.Green;
                    hashTextBox.Text = "-";
                    removePasswordButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Map parsing failed: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void removePasswordButton_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "GBX files (*.gbx)|*.gbx|All files (*.*)|*.*";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("None or invalid file path selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var path = dialog.FileName;

            try
            {
                _loadedMap.MapName = "[NOPSWD] " + _loadedMap.MapName;
                _loadedMap.RemovePassword();
                _loadedMap.Save(path);

                MessageBox.Show("Patch successful", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Map saving failed: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }
}
