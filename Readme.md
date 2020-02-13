# SQL managed identity

Example demonstrating how managed identity interacts with an Azure SQL database

**Note:** While this sample uses local accountsI urge you to consider using an oauth provider/Azure AD as the user store for a real project. It is much more secure than managing username/password yourself and users won't have to create a new account and can instead reuse their existing accounts. This sample is purely to demonstrate having a SQL database and the respective MSI connection.

# Run the sample locally with a local db

To get the sample running the database first needs to be created (update database command).

In Visual Studio you can simply run

> update-database

inside the package manager console.

Alternatively you can [install the tools](https://docs.microsoft.com/ef/core/miscellaneous/cli/index) (e.g. the .Net Core global tool "ef").

Then you can run the in any commandline inside the directory

> dotnet ef database update

After running it you should be able to launch the website locally and be able to register/login (you might have to apply migrations as well, the website will notify you when registering if this is the case).

Note that after registering you must click the "confim" link as the sample does not provide any way to email the users to confirm their email.

# Run the sample locally with an Azure Azure SQL database

Next setup the SQL server in Azure to start testing with MSI.

## 1. Create the infrastructure

To get started in Azure you must first create the database (the basic database is enough for the sample, ~5$/month).

I suggest you create a new resourcegroup and create the SQL database hand.

## 2. Configure SQL server for MSI

First make yourself the SQL server `Active Directory admin` (alternatively you can make a group the SQL server admin and then add multiple users to the group and they will all become SQL administrators).

Note that due to [limitations](https://docs.microsoft.com/azure/sql-database/sql-database-aad-authentication#azure-ad-features-and-limitations) the creator of the subscription cannot be made the SQL admin (the account will be grayed out in the selection screen and the group feature also does not work around this limit).

For work environments this is usually not a problem but for private projects you are most likely using the same account during development that you used to create the subscription.

In that case you can either add an alternative account of yours as a guest to the Azure AD or create a member account and use that account as the SQL admin.

Or alternatively make that account the subscription owner. You should then be able to make yourself the SQL admin with your development account.

Second make sure the firewall is open for your local IP (on the server firewall settings add your client IP).

Finally you should be able navigate to the sql database and use the query editor.

Instead of having to enter username & password you can just press the `continue as X` button on the right (assuming you are logged in with the SQL active directory admin of course).

## 3. Configure the project

Before you do so you must update the [appsettings.Development.json](./SQLManagedIdentity/appsettings.Development.json) to point to your SQL server & database.

You can simply replace the local connection string with the one from appsettings.json (using your server & database name of course).

The only code change you need to do is in your [database context](./SqlManagedIdentity/Data/ApplicationDbContext.cs) class (it requires the [AppAuthentication](https://www.nuget.org/packages/Microsoft.Azure.Services.AppAuthentication) package):

``` csharp
var con = Database.GetDbConnection();
if (con is SqlConnection connection)
{
    if (connection.ConnectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
        return; // no MSI needed when using local db

    // force sync 
    connection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
}
```

## 4. Run the code

Before launching, make sure you are [signed in](https://docs.microsoft.com/azure/app-service/app-service-web-tutorial-connect-msi#set-up-visual-studio) with the correct account locally as well.

For Visual Studio check `Tools -> Options -> Azure Service Authentication`, else check the logged in az cli account.

With the updated connection string and the SQL firewall opend up you should be able to run the project locally and have it connect to the Azure SQL database.

On first signup you will have to apply the migrations again and and afterwards you will be able to signup users without ever having used the SQL password!

# Run the sample in Azure with SQL database

The final step is to deploy the code to an app service in Azure and run it from there.

# 1. Create the infrastructure

If you don't yet have a webapp you can create one (along with a free app service plan).

Be sure to enable the app service managed identity by flipping the switch in the `Identity` menu to on.

For maximum security you should add the app service IPs (visible in properties) to your SQL server. Alternatively you can open up the SQL server to all Azure services by flipping the `Allow Azure services and resources to access this server` switch in the SQL firewall settings.

# 2. Setup SQL MSI for app service

With the app service created head back into the query editor of the SQL database as we must now create a SQL user for the app service. (Unlike user accounts the managed identities for services are not created automatically inside SQL server).


``` sql
CREATE USER [app service name] FROM EXTERNAL PROVIDER
ALTER ROLE db_datareader ADD MEMBER [app service name]
ALTER ROLE db_datawriter ADD MEMBER [app service name]
```

The output of all commands will be `Query succeeded: Affected rows: 0`.

If need be, you can also add [more roles](https://docs.microsoft.com/sql/relational-databases/security/authentication-access/database-level-roles#fixed-database-roles).

# 3. Configure the project

If you haven't done so already, update the [appsettings.json](./SqlManagedIdentity/appsettings.json) to point to your SQL server & database.

# 4. Publish the project

Either publish directly from Visual Studio or use the [pipeline](./azure-pipelines.yml).

For the pipeline you will have to update the `$(ResourceGroupName)`, `$(AppServiceName)` and `azureSubscription` (the service connection used to deploy the function).

Once published the app service should be up and running and it should establish a successful connection to the SQL database again without using any SQL password!