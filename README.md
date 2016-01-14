# Custom ASP.NET User Manager w/ Dapper Dot Net
This project is an open source implementation of a custom User Manager (ASP.NET Identity Framework 2) that uses Dapper Dot Net instead of Entity Framework for an ORM.  This project is available under the MIT license and can be used as a starting point for your next project or as a learning tool to get a feel for customizing the User Manager.

Examples for this project include a custom User Store, User Login Store, User Password Store and User Security Stamp Store.

##Getting Started

This solution is intended to be used as a learning reference or for starting a fresh project.  The best way to get a feel for this solution is to simply download the source code from this repository.  If you choose the former, the solution is ready to compile and run right out of the gate after NuGet packages are downloaded.  After downloading the solution follow these steps to run the solution.

*Note:  This project was built with Visual Studio 2015 using the MVC 5 web project template but it should be able to be ran on older versions of Visual Studio without too much hassle*

1.  Find the create scripts in the project and set them aside.
  1.  You can find these under DapperIdentity.Data project in the Scripts folder
2.  Create a new database either on your LocalDB or SQL Server of your choice
3.  Execute CreateUserTable.sql
4.  Execute CreateExternalLoginsTable.sql
5.  Update the connectionString property of the DefaultConnection element in the Web projects Web.config
  1.  `<add name="DefaultConnection" connectionString="Data Source=(LocalDb)\MSSQLLocalDB;Database=DapperIdentity;Integrated Security=True" providerName="System.Data.SqlClient" />`
6.  Build the solution.  The NuGet packages for the solution should be downloaded at this time.
7.  Hit F5 and test the site out.

##Customizing the User Account

The user object for this example has a couple of custom fields out the gate but you may be interested in adding more.  When creating the user object you must implement the IUser interface which, by default, requires a string id and string UserName field. An int id can be used in place of it but requires extensive customization of each implemented interface, the UserRepository and the database.

```
public class User : IUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsConfirmed { get; set; }
        public string ConfirmationToken { get; set; }
        public DateTime CreatedDate { get; set; }
    }
```

In this solution the custom fields include Nickname, IsConfirmed, ConfirmationToken and CreatedDate.  

*Note: The IsConfirmed and ConfirmationToken are used throughout the application to provide an example of doing account confirmation via email, however, the full example is not implemented and is intended for example purposes.  Please see the inline comments throughout the AccountController for more information.*

If you do change the User object you will be required to make changes to the following methods to account for the change.

Location | Change
-------- | --------
DapperIdentity.Data.Repositories.UserRepository.CreateAsync | Modify the UPDATE query
DapperIdentity.Data.Repositories.UserRepository.UpdateAsync | Modify the UPDATE query
DapperIdentity.Web.Controllers.AccountController.Register | The HttpPost action needs to account for the field when instantiating the User object before calling CreateAsync
DapperIdentity.Web.ViewModels.RegisterViewModel | If the data you are trying to collect is from the user at the time of registration, make sure to update the ViewModel for use with the Register view
DapperIdentity.Web.ViewModels.ExternalLoginConfirmationViewModel | The ExternalLoginConfirmationViewModel is used when collecting additional information about the user if you enable registration with third party systems such as Google
Register.cshtml | Finally after updating the the register and external login confirmation view models, you should update the view to collect this information
User Database Table | Should be updated with a column with the appropriate data type for the new field
