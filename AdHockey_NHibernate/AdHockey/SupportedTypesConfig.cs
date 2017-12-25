/*Author: Cameron Block*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace AdHockey {

    public class SupportedTypesSection : ConfigurationSection {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(SupportedTypesCollection), AddItemName = "TypeElement")]
        public SupportedTypesCollection SupportedTypes {
            get { return (SupportedTypesCollection)base[""]; }
        }

        public IEnumerable<Type> GetAllSupportedTypes() {
            for (int i = 0; i < this.SupportedTypes.Count; i++) {
                yield return Type.GetType(SupportedTypes[i].Name);
            }
        }//end method

        public IEnumerable<String> GetAllSupportedTypeNames() {
            for (int i = 0; i < this.SupportedTypes.Count; i++) {
                yield return ((TypeElement)SupportedTypes[i]).Name;
            }
        }//end method

        public List<SelectListItem> GetDdlListItem() {
            return (from name in GetAllSupportedTypeNames()
                   select new SelectListItem() { Text = name, Value = name}).ToList();
        }//end method

        [ConfigurationProperty("default")]
        public string Default {
            get { return (string)base["default"]; }
            set { base["default"] = value; }
        }
    }//end class

    public class SupportedTypesCollection : ConfigurationElementCollection {
        protected override ConfigurationElement CreateNewElement() {
            return new TypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((TypeElement)element).Name.GetHashCode();
        }

        public new TypeElement this[String key] {
            get { return (TypeElement)BaseGet(key); }
        }

        public TypeElement this[int key] {
            get { return (TypeElement)BaseGet(key); }
        }
    }//end class

    public class TypeElement : ConfigurationElement {
        [ConfigurationProperty("name", IsRequired = true)]
        public String Name {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
    }//end class

}//end namespace