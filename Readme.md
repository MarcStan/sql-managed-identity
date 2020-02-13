# SQL managed identity

Example demonstrating how managed identity interacts with an Azure SQL database

**Note:** While this sample uses local accountsI urge you to consider using an oauth provider/Azure AD as the user store for a real project. It is much more secure than managing username/password yourself and users won't have to create a new account and can instead reuse their existing accounts. This sample is purely to demonstrate having a SQL database and the respective MSI connection.

# Run the sample locally

To get the sample running the database first needs to be created (update database command).

In Visual Studio you can simply run

> update-database

inside the package manager console.

Alternatively you can [install the tools](https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/index) (e.g. the .Net Core global tool "ef").

Then you can run the in any commandline inside the directory

> dotnet ef database update

After running it you should be able to launch the website locally and be able to register/login (you might have to apply migrations as well, the website will notify you when registering if this is the case).