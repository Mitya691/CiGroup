using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DesktopClient.Model
{
    public class Settings
    {
        public string DbServer { get; set; }
        public string Database { get; set; }
        public int DbPort { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }

        [XmlIgnore]
        public string DbConnectionString => $"Server={DbServer};Port={DbPort};User={DbUser};Password={DbPassword};Database={Database};AllowLoadLocalInfile=true;";
        
        //конструктор по умолчанию
        public Settings() 
        {
            DbServer = "127.0.0.1";
            Database = "elevatordb";
            DbPort = 3306;
            DbUser = "root";
            DbPassword = "123456";
        }
    }
}
