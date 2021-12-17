using System.Reflection;
using System.Text;

namespace Lingya.Pagination.Tests {
    public abstract class DynamicClass {
        public override string ToString () {
            PropertyInfo[] props = this.GetType ().GetProperties (BindingFlags.Instance | BindingFlags.Public);
<<<<<<< HEAD
            StringBuilder sb = new ();
            sb.Append ('{');
            for (int i = 0; i < props.Length; i++) {
                if (i > 0) sb.Append (", ");
                sb.Append (props[i].Name);
                sb.Append ('=');
                sb.Append (props[i].GetValue (this, null));
            }
            sb.Append ('}');
=======
            StringBuilder sb = new StringBuilder ();
            sb.Append ("{");
            for (int i = 0; i < props.Length; i++) {
                if (i > 0) sb.Append (", ");
                sb.Append (props[i].Name);
                sb.Append ("=");
                sb.Append (props[i].GetValue (this, null));
            }
            sb.Append ("}");
>>>>>>> af9c08603256dd2d65573c09bca64f6b666b9013
            return sb.ToString ();
        }
    }
}