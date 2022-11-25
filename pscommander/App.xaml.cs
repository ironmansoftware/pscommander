using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;

namespace pscommander
{
    public class Options
    {
        [Option('f', "filePath", Required = false, HelpText = "File path for file association")]
        public string FilePath { get; set; }

        [Option('s', "shortcut", Required = false, HelpText = "Shortcut")]
        public string Shortcut { get; set; }
        [Option('c', "context", Required = false, HelpText = "Context Menu")]
        public string ContextMenu { get; set; }

        [Option('p', "contextPath", Required = false, HelpText = "Context Menu")]
        public string ContextMenuPath { get; set; }

        [Option("configFilePath", Required = false, HelpText = "Config File Path")]
        public string ConfigFilePath { get; set; }

        [Option("protocol", Required = false, HelpText = "Protocol")]
        public string Protocol { get; set; }

        [Option("protocolArg", Required = false, HelpText = "Protocol Arg")]
        public string ProtocolArg { get; set; }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Parser.Default.ParseArguments<Options>(e.Args).WithParsed<Options>(o => {
                var namedPipeClient = new NamedPipeClient();
                if (!string.IsNullOrWhiteSpace(o.FilePath))
                {
                    namedPipeClient.SendCommand(new Command {
                        Name = "fileAssociation", 
                        Properties = new Dictionary<string, string> {
                            { "filePath", o.FilePath }
                        }
                    });

                    Environment.Exit(0);
                }

                if (!string.IsNullOrWhiteSpace(o.Shortcut))
                {
                    namedPipeClient.SendCommand(new Command {
                        Name = "shortcut", 
                        Properties = new Dictionary<string, string> {
                            { "id", o.Shortcut }
                        }
                    });

                    Environment.Exit(0);
                }

                if (!string.IsNullOrWhiteSpace(o.ContextMenu))
                {
                    namedPipeClient.SendCommand(new Command {
                        Name = "contextMenu", 
                        Properties = new Dictionary<string, string> {
                            { "id", o.ContextMenu },
                            { "path", o.ContextMenuPath }
                        }
                    });

                    Environment.Exit(0);
                }

                if (!string.IsNullOrWhiteSpace(o.Protocol))
                {
                    namedPipeClient.SendCommand(new Command {
                        Name = "protocol", 
                        Properties = new Dictionary<string, string> {
                            { "protocol", o.Protocol },
                            { "arg", o.ProtocolArg }
                        }
                    });

                    Environment.Exit(0);
                }

                if (!string.IsNullOrWhiteSpace(o.ConfigFilePath))
                {
                    ConfigService.ConfigFilePath = o.ConfigFilePath;
                }

                base.OnStartup(e);
            });
        }
    }
}
