using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace pscommander
{
    public class NamedPipeService 
    {
        private readonly Thread thread;
        private static int numThreads = 4;
        private static CommandService _commandService;
        private static MenuService _menuService;
        public NamedPipeService(CommandService commandService, MenuService menuService)
        {
            thread = new Thread(ServerThread);
            thread.Start();

            _commandService = commandService;
            _menuService = menuService;
        }

        private static void ServerThread(object data)
        {
            while(true)
            {
                var pipeServer = new NamedPipeServerStream("pscommander", PipeDirection.InOut, numThreads, PipeTransmissionMode.Message, PipeOptions.CurrentUserOnly);

                 pipeServer.WaitForConnection();
                 var ss = new StreamString(pipeServer);
                 var input = ss.ReadString();

                 if (input == "shutdown")
                 {
                     break;
                 }

                 try 
                 {
                    var command = JsonConvert.DeserializeObject<Command>(input);
                    _commandService.ProcessCommand(command);
                 }
                 catch (Exception ex)
                 {
                     _menuService.ShowError(ex.Message);
                 }
            }
        }
    }

    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

}