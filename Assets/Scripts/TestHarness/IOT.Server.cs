using Newtonsoft.Json;

namespace GraphQLCodeGen {
    public class Types {
    
        #region Query
        public class Query {
            #region members
            [JsonProperty("currentNumber")]
            public int? currentNumber { get; set; }
    
            [JsonProperty("hello")]
            public string hello { get; set; }
            #endregion
        }
        #endregion
    
        #region Result
        public class Result {
            #region members
            [JsonProperty("dataType")]
            public string dataType { get; set; }
    
            [JsonProperty("id")]
            public string id { get; set; }
    
            [JsonProperty("json")]
            public string json { get; set; }
    
            [JsonProperty("slider")]
            public int? slider { get; set; }
    
            [JsonProperty("value")]
            public string value { get; set; }
            #endregion
        }
        #endregion
    
        #region Subscription
        public class Subscription {
            #region members
            [JsonProperty("greetings")]
            public Result greetings { get; set; }
    
            [JsonProperty("numberIncremented")]
            public int? numberIncremented { get; set; }
            #endregion
        }
        #endregion
    }
  
}

namespace GraphQLCodeGen {
}