using System;
using System.Collections.Generic;
using System.Linq;

namespace Lingya.Pagination.Tests {

    internal class Signature : IEquatable<Signature> {
        public DynamicProperty[] properties;
        public int hashCode;

        public Signature (IEnumerable<DynamicProperty> properties) {
            this.properties = properties.ToArray ();
            hashCode = 0;
            foreach (DynamicProperty p in properties) {
                hashCode ^= p.Name.GetHashCode () ^ p.Type.GetHashCode ();
            }
        }

        public override int GetHashCode () {
            return hashCode;
        }

        public override bool Equals (object obj) {
            return obj is Signature signature && Equals (signature);
        }

        public bool Equals (Signature other) {
            if (properties.Length != other.properties.Length) return false;
            for (int i = 0; i < properties.Length; i++) {
                if (properties[i].Name != other.properties[i].Name ||
                    properties[i].Type != other.properties[i].Type) return false;
            }
            return true;
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
}