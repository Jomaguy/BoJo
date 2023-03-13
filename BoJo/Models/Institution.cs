﻿namespace BoJo.Models
{
    public class Institution
    {
        public int intitutionID { get; set; }
        public string name { get; set; }
        public string about { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string tel_num { get; set; }
        public string region { get; set; }
        public string level { get; set; }
        public string control { get; set; }

        public string size { get; set; }
        public float acceptance_rate { get; set; }

        public float graduation_rate { get; set; }
        public float total_cost { get; set; }
        public float average_cost_after_aid { get; set; }
        public string apply_url { get; set; }
        public string website_url { get; set; }
        public string majors { get; set; }
        public float average_GPA { get; set; }
        public int average_SAT { get; set; }
        public int average_ACT { get; set; }
    }
}