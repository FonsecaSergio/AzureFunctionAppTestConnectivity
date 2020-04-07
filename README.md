# AzureFunctionAppTestConnectivity
Sample Azure Function application to test SQL Connectivity


Function_ConnectSQL
	Azure function expect parameter "ConnectionString" that you can copy directly from Azure Portal. But should looks like sample below
	
	Server=tcp:XXXXXXX.database.windows.net,1433;Initial Catalog=sandbox;Persist Security Info=False;User ID=XXXXXXXX;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;

Function_ConnectSQL_MSI
	Azure function expect parameter "ServerName" / "DatabaseName"