using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace pscommander
{
    public class ShortcutService
    {
        private readonly DataService _dataService;
        private readonly PowerShellService _powerShellService;
        private readonly MenuService _menuService;
        private List<Shortcut> _shortcuts;

        public ShortcutService(DataService dataService, PowerShellService powerShellService, MenuService menuService)
        {
            _dataService = dataService;
            _powerShellService = powerShellService;
            _menuService = menuService;
            _shortcuts = new List<Shortcut>();
        }

        public void Execute(int id)
        {
            var shortcut = _shortcuts.FirstOrDefault(m => m.Id == id);
            if (shortcut == null) return;

            try 
            {
                _powerShellService.Execute(shortcut.Action, shortcut);
            }
            catch (Exception ex)
            {
                _menuService.ShowError(ex.Message);
            }
        }

        public void SetShortcuts(IEnumerable<Shortcut> shortcuts)
        {
            var existing = _dataService.Shortcuts.FindAll();
            foreach(var item in existing)
            {
                string lnkFileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{item.Text}.lnk");
                File.Delete(lnkFileName);
            }

            _shortcuts.Clear();
            _dataService.Shortcuts.DeleteAll();
            var exePath = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe");

            foreach(var item in shortcuts)
            {
                string lnkFileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{item.Text}.lnk");
                ShortcutHelper.Create(lnkFileName, exePath, $"--shortcut {item.Id}", null, item.Description, null, item.Icon);
                _shortcuts.Add(item);
                _dataService.Shortcuts.Insert(item);
            }
        }
    }

    public class ShortcutHelper
    {

        private static Type m_type = Type.GetTypeFromProgID("WScript.Shell");
        private static object m_shell = Activator.CreateInstance(m_type);

        [ComImport, TypeLibType((short)0x1040), Guid("F935DC23-1CF0-11D0-ADB9-00C04FD58A0B")]
        private interface IWshShortcut
        {
            [DispId(0)]
            string FullName { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0)] get; }
            [DispId(0x3e8)]
            string Arguments { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3e8)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3e8)] set; }
            [DispId(0x3e9)]
            string Description { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3e9)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3e9)] set; }
            [DispId(0x3ea)]
            string Hotkey { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3ea)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3ea)] set; }
            [DispId(0x3eb)]
            string IconLocation { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3eb)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3eb)] set; }
            [DispId(0x3ec)]
            string RelativePath { [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3ec)] set; }
            [DispId(0x3ed)]
            string TargetPath { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3ed)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3ed)] set; }
            [DispId(0x3ee)]
            int WindowStyle { [DispId(0x3ee)] get; [param: In] [DispId(0x3ee)] set; }
            [DispId(0x3ef)]
            string WorkingDirectory { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3ef)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3ef)] set; }
            [TypeLibFunc((short)0x40), DispId(0x7d0)]
            void Load([In, MarshalAs(UnmanagedType.BStr)] string PathLink);
            [DispId(0x7d1)]
            void Save();
        }

        public static void Create(string fileName, string targetPath, string arguments, string workingDirectory, string description, string hotkey, string iconPath)
        {
            IWshShortcut shortcut = (IWshShortcut)m_type.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, m_shell, new object[] { fileName });
            shortcut.Description = description;
            //shortcut.Hotkey = hotkey;
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workingDirectory;
            shortcut.Arguments = arguments;
            if (!string.IsNullOrEmpty(iconPath))
                shortcut.IconLocation = iconPath;
            shortcut.Save();
        }
    }
}