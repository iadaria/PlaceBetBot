using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Placer.Sport888.Models
{
    public class Sport
    {
        public string name { get; set; }
        public string englishName { get; set; }
        public string termKey { get; set; }
    }

    public class Region
    {
        public string name { get; set; }
        public string englishName { get; set; }
        public string termKey { get; set; }
        public int sortOrder { get; set; }
    }

    public class Course
    {
        public string name { get; set; }
        public string englishName { get; set; }
        public string termKey { get; set; }
    }

    public class Context
    {
        public Sport sport { get; set; }
        public Region region { get; set; }
        public Course course { get; set; }
    }

    public class HourseRacing
    {
        public string meetingId { get; set; }
        public Context context { get; set; }
        public List<Event> events { get; set; }
    }
}
