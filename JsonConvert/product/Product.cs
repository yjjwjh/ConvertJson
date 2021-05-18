using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertJson.product
{
    public class Product
    {
        public string MC { get; set; }

        public string FJ { get; set; }

        public string FL { get; set; }

        public float CW { get; set; }

        public float CH { get; set; }

        public float CD { get; set; }

        public string FN { get; set; }
        public List<Parts> parts { get; set; }
    }
}
