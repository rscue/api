Sisgrua Api - The sisgrua HTTP Rest Api
========================================


Features
--------
The Api handles all the Sisgrua backend services needed for the system

Requisites
----------

You need to use [Visual Studio Code](https://code.visualstudio.com/) or 
[Visual Studio 2015 Update 3](https://www.visualstudio.com/news/releasenotes/vs2015-update3-vs) 
with the [AspNet Core tools](https://go.microsoft.com/fwlink/?LinkId=817245)

Environment
-----------
You need to setup the following environment variables. Values are distributed separately
* ```RSCUE_API_AzureSettings__StorageConnectionString``` - The connection string for the application storage account
* ```RSCUE_API_MongoDb__Url``` - The connection string for the application mongo db database
