using System;
using System.Text.RegularExpressions;

namespace Como.Model
{
    public class CustomEvent
    {
        public CustomEvent() { }
        private string _id;
        public String ID
        {
            get { return _id; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    if(Regex.IsMatch(value, "^[a-zA-Z0-9]+$"))
                        _id = value.ToUpperInvariant();

                }
            }
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate{ get; set; }
    }

}
