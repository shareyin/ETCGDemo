using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ETCF
{
    class GlobalMember
    {
        public static bool WriteLogSwitch = false;
        public static int OperLogFreshTime = 500;//单位ms

        public static HKCamera HKCameraInter = null;

        public static IPCCamera IPCCameraInter = null;

        public static SQLServerInter SQLInter = null;
    }
}
