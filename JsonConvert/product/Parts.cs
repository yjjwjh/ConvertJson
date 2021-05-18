using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertJson.product
{
    public class Parts
    {
        public int ID { get; set; }

        public string MC { get; set; }

        public float PW { get; set; }

        public float PH { get; set; }

        public float PT { get; set; }

        public string WL { get; set; }

        public List<Revolve> R { get; set; }

        public List<Parameter> CS { get; set; }

        public List<jjm> JJM { get; set; }


    }
}
