using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsSensorAgent
{
    class FPSProvider
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;

        public FPSProvider()
        {
            mmf = MemoryMappedFile.OpenExisting("RTSSSharedMemoryV2");
            accessor = mmf.CreateViewAccessor();
        }

        public int GetFPS()
        {
            UInt32 version = accessor.ReadUInt32(4);
            uint size = accessor.ReadUInt32(8);
            uint offset = accessor.ReadUInt32(12);

            for (int i = 0; i < 256; i++)
            {
                long current_offset = offset + i * size;

                uint process_id = accessor.ReadUInt32(current_offset);

                IntPtr hWnd = GetForegroundWindow();
                IntPtr processIdPtr = new IntPtr(sizeof(UInt32));
                int ProcessIdTest;
                GetWindowThreadProcessId(hWnd, out ProcessIdTest);

                if (ProcessIdTest == process_id)
                {
                    int fps = RoundToClosest10((int)accessor.ReadUInt32(current_offset + 5024)) / 10;
                    return fps;
                }
            }

            return 0;
        }
        private int RoundToClosest10(int value)
        {
            int lower = (value / 10) * 10;
            int upper = (value / 10 + 1) * 10;

            int result = (value - lower < upper - value) ? lower : upper;

            return result;
        }
    }
}
