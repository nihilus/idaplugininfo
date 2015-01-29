
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Reflection;
using System.Security;


namespace IdaPluginInfo
{        
    public partial class Form1 : Form
    {       
        public Form1()
        {
            InitializeComponent();
        }


        // From IDA SDK
        [StructLayout(LayoutKind.Explicit)]
        public struct plugin_t
        {
            [FieldOffset(0x00)]public Int32 version;
            [FieldOffset(0x04)]public Int32 flags;
            [FieldOffset(0x08)]public UInt32 init;  // int (WINAPI *init)(void);
            [FieldOffset(0x0C)]public UInt32 term;  // void (WINAPI *term)(void);
            [FieldOffset(0x10)]public UInt32 run;   // void (WINAPI *run)(int arg);
            [FieldOffset(0x14)][MarshalAs(UnmanagedType.LPStr)]public String comment;
            [FieldOffset(0x18)][MarshalAs(UnmanagedType.LPStr)]public String help;
            [FieldOffset(0x1C)][MarshalAs(UnmanagedType.LPStr)]public String wanted_name;
            [FieldOffset(0x20)][MarshalAs(UnmanagedType.LPStr)]public String wanted_hotkey;
        }

        private void praseExport(string fileName, IntPtr export)
        {
            plugin_t pt = (plugin_t)Marshal.PtrToStructure(export, typeof(plugin_t));
            //Console.WriteLine("hotkey: " + pt.wanted_hotkey);

            // Add row
            string[] row = new string[6];
            row[0] = fileName;
            row[1] = pt.wanted_name;
            row[2] = pt.version.ToString();
            row[3] = pt.flags.ToString("X4");
            row[4] = pt.wanted_hotkey;            
            row[5] = pt.comment;
            dataGridView1.Rows.Add(row);
            //dataGridView1.Rows[1].ReadOnly = true;
        }

        const uint DONT_RESOLVE_DLL_REFERENCES  = 0x00000001;
        const uint LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010;

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr LoadLibraryExA(string fileName, IntPtr dummy, uint flags);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        private void requestAndProcessFolder(string basePath, string searchExt)
        {         
            DirectoryInfo di = new DirectoryInfo(basePath);
            foreach (var fi in di.EnumerateFiles(searchExt, SearchOption.TopDirectoryOnly))
            {
                IntPtr hModule = LoadLibraryExA(fi.FullName, IntPtr.Zero, (DONT_RESOLVE_DLL_REFERENCES | LOAD_IGNORE_CODE_AUTHZ_LEVEL));                
                if (hModule != IntPtr.Zero)
                {                  
                    IntPtr export = GetProcAddress(hModule, "PLUGIN");
                    if (export == IntPtr.Zero)
                        export = GetProcAddress(hModule, "_PLUGIN");
                    if (export != IntPtr.Zero)
                        praseExport(fi.Name, export);                    
                    else
                        throw new DllNotFoundException("Plugin '" + fi.Name + "' is missing 'PLUGIN' export.");                           
                    FreeLibrary(hModule);
                } 
                else
                    throw new DllNotFoundException("Failed to load '" + fi.Name + "'.");                
            }
        }

        DialogResult requestAndProcessFolder()
        {                 
            // Request plugins folder
            FolderBrowserDialog fld = new FolderBrowserDialog();
            fld.Description = "Select Plugins Folder";
            DialogResult result = fld.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Back to view
                this.Activate();

                // Clear view
                while (dataGridView1.Rows.Count != 0)
                    dataGridView1.Rows.RemoveAt(0);      

                // Enumerate 32bit
                requestAndProcessFolder(fld.SelectedPath, "*.plw");

                // "" 32bit EA64
                requestAndProcessFolder(fld.SelectedPath, "*.p64");
            }

            return (result);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DialogResult result = requestAndProcessFolder();
            if (result != DialogResult.OK)
                Application.Exit();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            requestAndProcessFolder(); 
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            using (AboutBox box = new AboutBox())
            {
                box.ShowDialog(this);
            }
        }
    }
}
