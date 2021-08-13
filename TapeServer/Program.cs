using Archiver.Shared.Interfaces;
using Archiver.TapeServer.Classes.Config;

namespace Archiver.TapeServer
{
    class Program
    {
        static void Main()
        {
            TapeServerConfig config = TapeServerHelpers.ReadConfig();
            ITapeDrive tapeDrive = TapeServerHelpers.GetTapeDrive(config);

            NetworkServer server = new NetworkServer(config, tapeDrive);
            server.StartControlServer();
        }

        
    }
}
