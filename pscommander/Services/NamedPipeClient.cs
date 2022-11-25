using System.IO.Pipes;
using Newtonsoft.Json;

namespace pscommander
{
    public class NamedPipeClient 
    {
        public void SendCommand(Command command)
        {
            var client = new NamedPipeClientStream("pscommander");
            var message = JsonConvert.SerializeObject(command);
            client.Connect();

            var stringStream = new StreamString(client);
            stringStream.WriteString(message);
            client.WaitForPipeDrain();
            client.Close();
            client.Dispose();
        }
    }
}