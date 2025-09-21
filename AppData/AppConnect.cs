using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeBar.AppData
{
    internal class AppConnect
    {
        public static dynamic CurrentUser { get; set; }
        public static CoffeeBarDBEntities1 model01 = new CoffeeBarDBEntities1();
        public static string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CoffeeBarDB;Integrated Security=True;";
    }
}
