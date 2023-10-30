
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace HC5D_Lis_Service
{

    
    public class Component
    {
        public int index { get; set; }
        public string value { get; set; }
        
        public Component()
        {

        }

    }
    public class Segment
    {
        public int index { get; set; }
        public List<Component> components { get; set; }

        public Segment(string input) {
            components = new List<Component>();

            string[] tokens = Regex.Split(input, "\\|");

            for (int i = 0; i < tokens.Length; i++)
            {
                string current = tokens[i].ToString();
                components.Add(new Component() { index = i, value = current });
            }
        }

    }
    class Hl7Message
    {
        public List<Segment> segments { get; set; }

        public Hl7Message(string[] hl7messages)
        {
            this.segments = new List<Segment>();
            foreach(string rec in hl7messages)
            {
                this.segments.Add(new Segment(rec));
            }
        }
    }
}
