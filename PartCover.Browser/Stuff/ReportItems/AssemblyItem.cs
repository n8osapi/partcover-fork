using System.Collections.Generic;
using PartCover.Browser.Api.ReportItems;

namespace PartCover.Browser.Stuff.ReportItems
{
    class AssemblyItem : IAssembly
    {
        private readonly string name;
        private readonly List<ClassItem> types = new List<ClassItem>();
        private readonly List<NamespaceItem> namespaces = new List<NamespaceItem>();

        public AssemblyItem(string name)
        {
            this.name = name;
        }

        public void AddType(ClassItem item)
        {
            types.Add(item);
        }

        public IClass[] GetTypes()
        {
            return types.ToArray();
        }

        public string Name
        {
            get { return name; }
        }

        public NamespaceItem FindNamespace(string n, NamespaceItem parentNamespace)
        {
            return namespaces.Find(delegate(NamespaceItem actual) {
                return actual.Parent == parentNamespace && actual.Name == n;
            });
        }

        public void AddNamespace(NamespaceItem namespaceItem)
        {
            namespaces.Add(namespaceItem);
        }
    }
}
