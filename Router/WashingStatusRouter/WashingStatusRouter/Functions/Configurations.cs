using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace WashingStatusRouter.Functions
{
    class Configurations
    {
        public static string ServerIP;
        public static string ServerPort;
        public static string Username;
        public static string Password;
        private static string tempIp, tempPort, tempUsername, tempPassword;
        public static bool ServerIsSaved, UserIsSaved;
        public static void SaveLoadInfo()
        {
            if (File.Exists("LoginInfo.xml")) File.Delete("LoginInfo.xml");
            XElement xElement = new XElement("LoginInformation");
            XElement Server = new XElement("Server");
            if (tempIp != null)
            {
                Server.Add(new XAttribute("IsSaved", true));
                Server.Add(
                    new XElement("ServerIP", tempIp),
                    new XElement("Port", tempPort)
                );
            }
            else
            {
                Server.Add(new XAttribute("IsSaved", false));
            }
            xElement.Add(Server);
            XElement User = new XElement("User");
            if (tempUsername != null)
            {
                User.Add(new XAttribute("IsSaved", true));
                User.Add(
                    new XElement("Username", tempUsername),
                    new XElement("Password", tempPassword)
                );
            }
            else
            {
                User.Add(new XAttribute("IsSaved", false));
            }
            xElement.Add(User);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create("LoginInfo.xml", settings);
            xElement.Save(writer);
            writer.Flush();
            writer.Close();
        }
        public static void SaveServerInfo(string ServerIP, string Port)
        {
            tempIp = ServerIP;
            tempPort = Port;
        }

        public static void SaveUserInfo(string Username, string Password)
        {
            tempUsername = Username;
            tempPassword = Password;
        }
        public static bool LoadLoginInfo()
        {
            if (!File.Exists("LoginInfo.xml"))
            {
                return false;
            }
            else
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load("LoginInfo.xml");
                    XmlElement server = (XmlElement)doc.SelectSingleNode("LoginInformation/Server");
                    XmlElement user = (XmlElement)doc.SelectSingleNode("LoginInformation/User");
                    ServerIsSaved = server.GetAttribute("IsSaved") == "true" ? true : false;
                    UserIsSaved = user.GetAttribute("IsSaved") == "true" ? true : false;
                    if (ServerIsSaved)
                    {
                        ServerIP = doc.SelectSingleNode("LoginInformation/Server/ServerIP").InnerText;
                        ServerPort = doc.SelectSingleNode("LoginInformation/Server/Port").InnerText;
                    }
                    if (UserIsSaved)
                    {
                        Username = doc.SelectSingleNode("LoginInformation/User/Username").InnerText;
                        Password = doc.SelectSingleNode("LoginInformation/User/Password").InnerText;
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
