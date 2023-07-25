# EventlogStore
Adds an Eventlog Attribute Store to AD FS to allow recording authentications into the Windows Eventlog

Installation:

1. Copy EventlogStore.dll into C:\Windows\ADFS folder
2. Create new Eventlog Source entry (eventcreate /ID 1 /L APPLICATION /T INFORMATION  /SO "AD FS RP Authentication" /D "Server Install")
3. Add new Custom Attribute Store in the AD FS Management Console | Trust Relationships | Attribute Stores
    - Display name: EventLogStore
    - Custom attribute store class name: EventlogStore.Eventlog,EventlogStore
4. Add new Custom Claim Rule on each Relying Party to be recorded into the Windows Eventlog
    - Claim rule name: Record Auth in Eventlog
    - Custom Rule:
      c1:[Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname"]
      && c2:[Type == "http://schemas.microsoft.com/2012/01/requestcontext/claims/x-ms-client-ip"]
      && c3:[Type == "http://schemas.microsoft.com/2012/01/requestcontext/claims/client-request-id"]
      => add(store = "EventLogStore", 
      types = ("Eventlog"), query = "RP: {0}; Name: {1}; IP: {2}; ReqID: {3}", 
      param = "<Add_RelyingPartyName_Here>", param = c1.Value, param = c2.Value, param = c3.Value);

Reference: https://learn.microsoft.com/en-us/archive/blogs/cloudpfe/how-to-create-a-custom-attribute-store-for-active-directory-federation-services-3-0
