using System;
using System.IO.Pipes;

namespace WindowsSensorAgent
{
    class FPSProviderPipe : FPSProvider
    {
        Boolean connected = false;
        NamedPipeClientStream stream;

        public FPSProviderPipe() {
            stream = new NamedPipeClientStream(".", "\\PIPE\\RTSS_Frametime", PipeDirection.InOut);
        }
        public int GetFPS()
        {
            if (!connected)
            {
                Console.WriteLine("Opening pipe");
                stream.Connect();
                connected = true;
                Console.WriteLine("Pipe opened");
            }

            byte[] buffer = new byte[8];
            UInt32 app = (UInt32) buffer[0] + buffer[1] << 8 + buffer[2] << 16 + buffer[3] << 24;
            UInt32 frameTime = (UInt32)buffer[4] + buffer[5] << 8 + buffer[6] << 16 + buffer[7] << 24;

            int result = stream.Read(buffer, 0, 8);
            if (result != 8)
            {
                throw new Exception(String.Format("Failed to read message: {0}", result));
            }

            Console.WriteLine("App: {0}, FrameTime: {1}", app, frameTime);

            return (int) frameTime;            
        }
    }
}
