namespace RouteHandlers
{
    [RouteUrl("api/contract/import/boq_actuals/{bulk_ID}/{cutoff_ID}/{orgunit_ID}")] 
    public class api_contract_import_boq_actuals : RouteHandlerBase, IRequiresSessionState
    {   
        private class RouteParameters : RouteParametersBase
        {
            public int vegobjecttyper_ID { get; set; }

            //public int orgunit_ID { get; set; }
            public RouteParameters(RequestContext pContext) : base(pContext) { }
        }

        protected override async Task ProcessAsync(RequestContext pContext, CancellationToken ct)
        {
            // Check authentication
		    if( !UserContext.IsAuthenticated(pContext) ) { throw new Exception("You have to be logged in"); }
			if( pContext.Request.HttpMethod != "POST" ) { throw new HttpException((int)HttpStatusCode.MethodNotAllowed, "Method not allowed"); }

            try
            {
                var vFiles = pContext.Request.Files;
                string vData = ImportFile(vFiles.Get(0).InputStream, pContext);
                JSON.Serializer.Serialize(pContext.Response.Output, new { 
                    Status = "OK",
                    Message = "Upload completed",
                    BOQAlreadyFinished = vData
                });
            }
            catch (Exception ex)   
            {
                pContext.HttpContext.Response.Clear();
                pContext.HttpContext.Response.StatusCode = 400;
                JSON.Serializer.Serialize(pContext.Response.Output, new { 
                    error = ex.Message, 
                });
            }
	         
			await Task.FromResult(true);
        }

        private static DataTable GetFinishedBOQ(RequestContext pContext) {
            var vUserContext = UserContext.ForHttpContext(pContext);
            RouteParameters vParameters = new RouteParameters(pContext);
            afProcedureCall vProc = new afProcedureCall("astp_Contract_GetFinishedBOQ");
            vProc.Parameters.Add("Bulk_ID", vParameters.bulk_ID);

            return vUserContext.ExecuteProcedure(vProc).Tables[0];
        }

        private string ImportFile(Stream pStream, RequestContext pContext)
        { 
            var vUserContext = UserContext.ForHttpContext(pContext);
            var vFinishedBOQ = new DataTable();
            string vFinishedBOQString = "";
            vFinishedBOQ = GetFinishedBOQ(pContext);
            RouteParameters vParameters = new RouteParameters(pContext);

            // Load XML file from stream
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(pStream);

            // Import table
            DataTable vImportTable = new DataTable();
            DataRow vNewRow = vImportTable.NewRow();
            vImportTable.Columns.Add("Bulk_ID", typeof(int)); 
            vImportTable.Columns.Add("BOQ_ID", typeof(int)); 
            vImportTable.Columns.Add("CutOff_ID", typeof(int)); 
            vImportTable.Columns.Add("Qty", typeof(decimal)); 

            Debug.WriteLine("Loop through XML file", "api_contract_import_boq");
            
            // Loop through nodes - NS3459 standard format
            foreach (XmlNode NS3459_SUB in xDoc.DocumentElement.ChildNodes) // Node: NS3459
            {
                foreach (XmlNode Avregning_SUB in NS3459_SUB) // Node: Avregning
                {
                    foreach (XmlNode ProsjektNS_SUB in Avregning_SUB) // Node: ProsjektNS
                    {
                        foreach (XmlNode Poster_SUB in ProsjektNS_SUB) // Node: Poster
                        {
                            if(Poster_SUB.Name == "Post")
                            {
                                vNewRow = vImportTable.NewRow(); // Create New Row
                                // vNewRow["Contract_ID"] = vParameters.contract_ID;
                                vNewRow["CutOff_ID"] = vParameters.cutoff_ID;
                                vNewRow["Bulk_ID"] = vParameters.bulk_ID;

                                foreach (XmlNode Post_SUB in Poster_SUB) // Node: Post
                                {
                                    if(Post_SUB.Name == "Postnr") // Node: Postnr 
                                    {
                                        String vBOQName = Post_SUB.InnerText;
                                        bool BOQIsFinished = vFinishedBOQ.AsEnumerable().Any(row => vBOQName == row.Field<String>("BOQ"));

                                        if(BOQIsFinished) {
                                            if(vFinishedBOQString == "") {
                                                vFinishedBOQString += vBOQName;
                                            } else {
                                                vFinishedBOQString += ", " + vBOQName;
                                            }
                                            continue;
                                        } else {
                                            var sp = new afProcedureCall("astp_Contract_GetBOQFromName");
                                            sp.Parameters.Add("Contract_ID", vParameters.orgunit_ID);
                                            sp.Parameters.Add("Name", Post_SUB.InnerText);
                                            var result = vUserContext.ExecuteProcedure(sp);

                                            if(result.Tables[0].Rows.Count != 0)  
                                            {
                                                vNewRow["BOQ_ID"] = result.Tables[0].Rows[0].GetColumnValue("BOQ_ID", -1);
                                            } else 
                                            {
                                                vNewRow["BOQ_ID"] = System.DBNull.Value;
                                            }
                                        }
                                    } else if(Post_SUB.Name == "Prisinfo") // Node: Prisinfo
                                    {
                                        foreach (XmlNode Prisinfo_SUB in Post_SUB)
                                        {
                                            if(Prisinfo_SUB.Name.ToString() == "UtfortMengde") 
                                            {
                                                try { vNewRow["Qty"] = decimal.Parse(Prisinfo_SUB.InnerText.Replace(".", ",")); } catch { }
                                            }
                                        }
                                    }
                                }

                                if(vNewRow["Qty"] != System.DBNull.Value && vNewRow["BOQ_ID"] != System.DBNull.Value) 
                                {
                                    if(Convert.ToDecimal(vNewRow["Qty"]) > 0) 
                                    {
                                        vImportTable.Rows.Add(vNewRow); // Add Row To Table
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Insert bulk
            vUserContext.BulkInsert(vImportTable, "atbv_Contract_BOQActuals", 3600);
            return vFinishedBOQString;
		} 
    }
}