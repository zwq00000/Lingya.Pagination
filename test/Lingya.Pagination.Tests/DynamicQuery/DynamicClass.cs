using System.Reflection;
using System.Text;

namespace Lingya.Pagination.Tests {
    public abstract class DynamicClass {
        public override string ToString () {
            PropertyInfo[] props = this.GetType ().GetProperties (BindingFlags.Instance | BindingFlags.Public);
            StringBuilder sb = new ();
            sb.Append ('{');
            for (int i = 0; i < props.Length; i++) {
                if (i > 0) sb.Append (", ");
                sb.Append (props[i].Name);
                sb.Append ('=');
                sb.Append (props[i].GetValue (this, null));
            }
            sb.Append ('}');
            return sb.ToString ();
        }
    }
}