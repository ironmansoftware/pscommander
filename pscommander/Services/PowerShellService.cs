using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace pscommander
{
    public class PowerShellService
    {
        private Runspace _runspace;
        private readonly object _locker = new object();
        public void Initialize(Dictionary<string, object> variables)
        {
            var filePath = Assembly.GetExecutingAssembly().Location;
            var fileInfo = new FileInfo(filePath);

            var init = InitialSessionState.CreateDefault();
            init.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.RemoteSigned;
            init.ImportPSModule(Path.Combine(fileInfo.DirectoryName, "PSCommander.psd1"));

            foreach(var variable in variables)
            {
                init.Variables.Add(new SessionStateVariableEntry(variable.Key, variable.Value, string.Empty));
            }

            _runspace = RunspaceFactory.CreateRunspace(init);
            _runspace.Open();

            Runspace.DefaultRunspace = _runspace;
        }

        public void Execute(ScriptBlock scriptBlock, params object[] arguments)
        {
            lock(_locker)
            {
                Runspace.DefaultRunspace = _runspace;
                scriptBlock.Invoke(arguments);
            }
        }

        public IEnumerable<T> Execute<T>(ScriptBlock scriptBlock, params object[] arguments)
        {
            lock(_locker)
            {
                Runspace.DefaultRunspace = _runspace;
                return scriptBlock.Invoke(arguments).Select(m => m.BaseObject).OfType<T>();
            }
        }

        public IEnumerable<PSObject> ExecuteNoUnwrap(ScriptBlock scriptBlock, params object[] arguments)
        {
            lock(_locker)
            {
                Runspace.DefaultRunspace = _runspace;
                return scriptBlock.Invoke(arguments);
            }
        }

        public IEnumerable<T> ExecuteNewRunspace<T>(string script)
        {
            var filePath = Assembly.GetExecutingAssembly().Location;
            var fileInfo = new FileInfo(filePath);

            var init = InitialSessionState.CreateDefault();
            init.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.RemoteSigned;
            init.ImportPSModule(Path.Combine(fileInfo.DirectoryName, "PSCommander.psd1"));

            using(var runspace = RunspaceFactory.CreateRunspace(init))
            {
                runspace.Open();
                using(var powerShell = PowerShell.Create())
                {
                    powerShell.AddScript(script);
                    return powerShell.Invoke<T>();
                }
            }
        }
    }
}