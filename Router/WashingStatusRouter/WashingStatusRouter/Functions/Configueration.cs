using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace WashingStatusRouter.Functions
{
    class Configure
    {
        JsonObjectAttribute JsonObject = new JsonObjectAttribute(MemberSerialization.OptIn);
        public static bool ReadConfiguration(string Path)
        {
            if (!File.Exists(Path))
            {
                return false;
            }
            return true;
        }
    }
    class Configueration
    {
        bool IsSavedServer;
        bool IsSavedUser;
    }
    class ServerInfo
    {
        string ServerAddress;
        string ServerPort;
    }
    class UserInfo
    {
        string UserName;
        string Password;
    }
}
