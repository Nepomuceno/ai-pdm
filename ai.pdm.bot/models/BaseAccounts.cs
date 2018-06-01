using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ai.pdm.bot.models
{

    public class Request
    {
        public string statusCode { get; set; }
        public Schema schema { get; set; }
        public Body body { get; set; }
    }

    public class Schema
    {
        public Properties properties { get; set; }
        public string type { get; set; }
    }

    public class Properties
    {
        public Accounts accounts { get; set; }
    }

    public class Accounts
    {
        public string type { get; set; }
    }

    public class Body
    {
        public Account[] accounts { get; set; }
    }

    public class Account
    {
        public string business_unit { get; set; }
        public string crm_id { get; set; }
        public string email { get; set; }
        public string industy { get; set; }
        public int? isv_busines_model { get; set; }
        public string mpn_id { get; set; }
        public string name { get; set; }
        public object primary_contact { get; set; }
        public string[] responsible { get; set; }
        public int status { get; set; }
        public string tier { get; set; }
        public string website { get; set; }
    }

}
