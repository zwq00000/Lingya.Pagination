using System;

namespace PaginationTests
{
    public sealed class ParseException : Exception
    {
        int position;

        public ParseException(string message, int position)
            : base(message) {
            this.position = position;
        }

        public int Position {
            get { return position; }
        }

        public override string ToString() {
            return string.Format(Res.ParseExceptionFormat, Message, position);
        }
    }
} 
 
/* Test Code 
namespace Dynamic 
{ 
    class Program 
    { 
        static void Main(string[] args) 
        { 
            // For this sample to work, you need an active database server or SqlExpress. 
            // Here is a connection to the Data sample project that ships with Microsoft Visual Studio 2008. 
            string dbPath = Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\..\..\Data\NORTHWND.MDF")); 
            string sqlServerInstance = @".\SQLEXPRESS"; 
            string connString = "AttachDBFileName='" + dbPath + "';Server='" + sqlServerInstance + "';user instance=true;Integrated Security=SSPI;Connection Timeout=60"; 
 
            // Here is an alternate connect string that you can modify for your own purposes. 
            // string connString = "server=test;database=northwind;user id=test;password=test"; 
 
            Northwind db = new Northwind(connString); 
            db.Log = Console.Out; 
 
            var query = 
                db.Customers.Where("City == @0 and Orders.Count >= @1", "London", 10). 
                OrderBy("CompanyName"). 
                Select("New(CompanyName as Name, Phone)"); 
 
            Console.WriteLine(query); 
            Console.ReadLine(); 
        } 
    } 
} 
 
*/
