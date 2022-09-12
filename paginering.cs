

namespace RouteHandlers 
{
    [Description("Retrives the json for each RoadObject of a spesific type")]
    [RouteUrl("api/NVDB/DistinctRoadObjects/{Changed}")]
    [RouteConstraint("Changed", "\\w+")]
    public class DistinctRoadObjects : AuthenticatedRouteHandlerBase {

        /*  Enable if needed  
            protected override bool RequireDeveloper { get { return true; } }
            protected override bool RequireTwoFactor { get { return true; } }
        */
        ///////////////////////////////////////////////
        private class RouteParameters : RouteParametersBase
        {
            public string Changed { get; set; }

            public RouteParameters(RequestContext pContext) : base(pContext) { }
        }
        ///////////////////////////////////////////////
        protected override async Task ProcessAuthenticatedAsync(RequestContext pContext, CancellationToken ct){
            pContext.Response.ContentType = "application/json";
            if(pContext.Request.HttpMethod != "GET")
                throw new HttpException((int)HttpStatusCode.MethodNotAllowed, "Method not allowed");
            
            /* Your Code Here */ 
            string url = "https://nvdbapiles-v2.atlas.vegvesen.no/vegobjekter/";
            DataTable dt = new DataTable();
            int roadObjectID = 3;
            string nextUrl;
            bool finishied = false;
            RouteParameters vParameters = new RouteParameters(pContext);

            dt.Columns.Add("DistinctRoadObject_ID", typeof(int));
            dt.Columns.Add("JSON", typeof(string));
            var newRow = dt.NewRow();


            if(vParameters.Changed.Equals("endret")) {

                string response = await GetChangedRoadObject(url + roadObjectID + "/", vParameters.Changed, "2022-08-08", 1);
                Debug.WriteLine(response, "api_changed");

                
                var results = JsonConvert.DeserializeObject<dynamic>(response);
                //var test = JsonConvert.SerializeObject(results.objekter[0]);

                string test = results.objekter[0].type;
                Object obj = results.objekter[0].vegobjekt;
                string jsonString = JsonConvert.SerializeObject(obj);

                int firstPagingNumb = results.metadata.returnert;
                int distinctRoadObjectID = results.objekter[0].vegobjekt.id;

                newRow["DistinctRoadObject_ID"] = distinctRoadObjectID;
                newRow["JSON"] = jsonString;
                dt.Rows.Add(newRow);

                Debug.WriteLine(dt.Rows.Count,"api_dt");



                Debug.WriteLine(firstPagingNumb,"api_num");
                Debug.WriteLine(test, "api_type");
                Debug.WriteLine(distinctRoadObjectID, "api_vegobjekt");
                Debug.WriteLine(jsonString, "api_vegobjekt");

                //UserContext.ForHttpContext(pContext).BulkInsert(dt, "atbv_NVDB_DistinktVegobjekter", 3600);

            } else if(vParameters.Changed.Equals("slettet")) {
                string response = await GetChangedRoadObject(url + roadObjectID + "/", vParameters.Changed, "2022-08-08", 1);
                 Debug.WriteLine(response, "api_deleted");

                DeletedJsonList results = JsonConvert.DeserializeObject<DeletedJsonList>(response);

                int distinctRoadObjectID = results.objekter[0].vegobjekt;

                                
                                
                Debug.WriteLine(distinctRoadObjectID, "api_vegobjekt");

            } else {
                string response = await GetRequest(url + roadObjectID);
                //Debug.WriteLine(response, "api_response");
            
                JsonList results;
                results = JsonConvert.DeserializeObject<JsonList>(response);
                int firstPagingNumb = results.metadata.returnert;

                Debug.WriteLine(firstPagingNumb,"api_num");
                //Debug.WriteLine(results.objekter[0].id,"api_results");
                Debug.WriteLine(results.metadata.returnert,"api_results");
                //Debug.WriteLine(results.metadata.neste.start,"api_results");
                //Debug.WriteLine(results.metadata.neste.href,"api_results");

                while(finishied) {
                    nextUrl = results.metadata.neste.href;
                    //Debug.WriteLine(nextUrl, "api_nextUrl");

            
                    foreach (JsonObject result in results.objekter)
                    {
                        int distinctRoadObjectID = result.id;
                        string distinctRoadObjectUrl = result.href;

                        newRow["DistinctRoadObject_ID"] = distinctRoadObjectID;

                        //Send get request to get all the distinct road objects of a disctinct roadobject type
                        string response2 = await GetRequest(distinctRoadObjectUrl);
                        //Debug.WriteLine(response2, "api_response2");

                        newRow["JSON"] = response2;

                        dt.Rows.Add(newRow);

                    }

                    //Check if the respons has less then max paging number
                    if (results.metadata.returnert < firstPagingNumb) 
                    {
                        finishied = true;
                    } else 
                    {
                        response = await GetRequest(nextUrl);
                        //Debug.WriteLine(response, "api_response9");
                        results = JsonConvert.DeserializeObject<JsonList>(response);
                        //Debug.WriteLine(results, "api_results9");
                    }
                
                }
                Debug.WriteLine(dt.Rows.Count,"api_dt");

                //UserContext.ForHttpContext(pContext).BulkInsert(dt, "atbv_NVDB_DistinktVegobjekter", 3600);
            }
            

            
        

            await Task.FromResult(true); /* Change if needed to await other async method. */
        }

        public async Task<string> GetRequest (string url)
        {
            HttpClient httpClient = new HttpClient();

            try 
            {
                var httpResponse = await httpClient.GetAsync(url);
                string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                return jsonResponse;


            }
             catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Debug.WriteLine(e, "error");
                return e.Message;
            }
        }

        public async Task<string> GetChangedRoadObject(string url, string change, string date, int paging)
        {
            Debug.WriteLine(url + "endringer?type=" + change + "&etter=" + date + "&antall=" + paging);
           return await GetRequest(url + "endringer?type=" + change + "&etter=" + date + "&antall=" + paging);
        }
    }
    public class JsonList : JsonObject
    {
        public List<JsonObject> objekter { get; set; }
        public Metadata metadata { get; set; }
    }

    public class JsonObject 
    {
        public int id { get; set; }
        public string href {get; set; }
    } 
    public class Metadata
    {
        public int returnert { get; set; }
        public Neste neste { get; set; }
    }
    public class Neste 
    {
        public string start { get; set; }
        public string href { get; set; }
    }


    public class DeletedJsonList : DeletedJsonObject
    {
        public List<DeletedJsonObject> objekter { get; set; }
        public Metadata metadata { get; set; }
    }

    public class DeletedJsonObject 
    {
        public string type { get; set; }
        public int vegobjekt { get; set; }
    }
    
}