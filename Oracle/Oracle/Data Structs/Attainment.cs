using System;
using System.Collections.Generic;
using System.Text;

namespace Oracle.Data
{
    public class Attainment
    {
        public string Arcana { get; set; }
        public int Dots { get; set; }
        public string Name { get; set; }
        public string Fluff { get; set; }
        public string Effect { get; set; }

        public Attainment(string _Name, string _Arcana, int _Dots, string _fluff, string _Effect)
        {
            Name = _Name;
            Arcana = _Arcana;
            Dots = _Dots;
            Effect = _Effect;
            Fluff = _fluff;
        }
    }
}
