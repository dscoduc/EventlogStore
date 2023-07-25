using Microsoft.IdentityServer.ClaimsPolicy.Engine.AttributeStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel;

namespace EventlogStore
{
    /// <summary>
    /// Sign the DLL file using the syntax:
    ///     "c:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /a EventlogStore.dll
    /// </summary>
    public class Eventlog : IAttributeStore
    {
        public string EventLogSource;
        public string EventLogID;

        public void Initialize(Dictionary<string, string> config)
        {
            // check if Eventlog Source is provided in the AD FS Attribute Store configuration
            if (!config.TryGetValue("eventlogsource", out EventLogSource))
            {
                EventLogSource = "AD FS RP Authentication";
            }

            // check if Eventlog ID is provided in the AD FS Attribute Store configuration
            if (!config.TryGetValue("eventlogid", out EventLogID))
            {
                EventLogID = "411";
            }

            // check if eventlog source is configured on this server
            if (!EventLog.SourceExists(EventLogSource))
            {
                string errorMessage = string.Format("Eventlog source {0} not registered on this server (eventcreate /ID 1 /L APPLICATION /T INFORMATION  /SO '{0}' /D 'Server Install'", EventLogSource);
                throw new AttributeStoreInvalidConfigurationException(errorMessage);
            }
        }

        /// <summary>
        /// Sample claim rule to use this custom store
        /// 
        ///     c1:[Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"]
        ///     && c2:[Type == "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-ip"]
        ///     && c3:[Type == "http://schemas.microsoft.com/2012/01/requestcontext/claims/client-request-id"]
        ///      => add(store = "EventLogStore", types = ("EventlogStore"), query = "RP: {0}; Name: {1}; IP: {2}; ReqID: {3}", param = "ClaimsTest", param = c1.Value, param = c2.Value, param = c3.Value);
        ///
        /// </summary>
        public IAsyncResult BeginExecuteQuery(string query, string[] parameters, AsyncCallback callback, object state)
        {
            // check if query was provided
            if (String.IsNullOrEmpty(query))
                throw new AttributeStoreQueryFormatException("No query string provided.");

            // check if parameters were provided
            if (parameters == null)
                throw new AttributeStoreQueryFormatException("No parameters provided.");

            try
            {
                // initialize new Eventlog Instance
                EventLog appLog = new EventLog("Application") { Source = EventLogSource };

                // build eventlog body by combining the query and parameters
                string message = string.Format(query, parameters);

                // convert EventLogID into integer
                int eID = Convert.ToInt32(EventLogID);

                // write event into EventLogName as an Information using specified EventLogID
                appLog.WriteEntry(message, EventLogEntryType.Information, eID);

                TypedAsyncResult<string[][]> asyncResult = new TypedAsyncResult<string[][]>(callback, state);
                asyncResult.Complete(null, true);
                return asyncResult;
            }
            catch (Exception ex)
            {
                throw new AttributeStoreQueryExecutionException(ex.Message, ex.InnerException);
            }
        }

        public string[][] EndExecuteQuery(IAsyncResult result)
        {
            return TypedAsyncResult<string[][]>.End(result);
        }
    }
}