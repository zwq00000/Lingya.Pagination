using System;

namespace PaginationTests
{
    public class DynamicProperty {
        string name;
        Type type;

        public DynamicProperty(string name, Type type) {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (type == null) throw new ArgumentNullException(nameof(type));
            this.name = name;
            this.type = type;
        }

        public string Name {
            get { return name; }
        }

        public Type Type {
            get { return type; }
        }
    }
} 