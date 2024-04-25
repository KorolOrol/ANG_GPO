using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClasses.Interface
{
    public interface IPart
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public void Merge(IPart part);

        public string FullInfo();

        public bool IsEmpty();
    }
}
