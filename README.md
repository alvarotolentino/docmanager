# Document Manager Rest API
## ASP.NET Core 3.1 Web API - Clean Architecture 

This is a clean-architecture solution with loosely-coupled, inverted-dependency and many more essential features.

Features:

:white_check_mark: CQRS with MediatR

:white_check_mark: Repository Pattern

:white_check_mark: Npgsql and ADO.NET-compatible

:white_check_mark: Repository Pattern

:white_check_mark: MediatR Pipeline Logging & Validation

:white_check_mark: Serilog

:white_check_mark: Swagger UI

:white_check_mark: Response Wrappers

:white_check_mark: Healthchecks

:white_check_mark: Pagination

:white_check_mark: Microsoft Identity with JWT Authentication

:white_check_mark: Role based Authorization

:white_check_mark: Database Seeding

:white_check_mark: Custom Exception Handling Middleware.

:white_check_mark: API Versioning

:white_check_mark: User - Role - Group Management (Register/Confirmation Email)

Technologies used:

* .Net Core 3.1
* PostgreSQL

Functionalities implemented:

* Login.
* Upload and download of files.
* Create and list Users and Groups.
* A user can belong to one or more groups.
* Document access can be granted to groups or directly to users.
* Default roles: Admin, Manager and Basic
    * Basic can download documents.
    * Regular can upload and download documents.
    * Admin can crud Users, Groups, upload and dowload documents.
* Store metadata and files in separate databases.


This is the Swagger documentation:

![Swagger](swagger.png)

Example of confirmation email, when an account is registered:

![ConfirmationEmail](confirmation_email.png)

The resouces are grouped as follows:

* Accounts: Creates a new account, assign roles and groups.
* Documents: Allows to upload, download and retrives metadata files.
* Groups: Creates and update groups.
* Secutiry: Allows Login and generate token.

Below, how the solution structure looks like.

![Project](solution_structure.png) 


There are the list of components:

* Domain: Contains all the entities and it does not depends on anything else.
* Application: Contains Interfaces, Validators (FluentValidation), Mapping (AutoMapper), CQRS (MediatR), Behaviors, etc.
* Infraestructure: Contains three components.
    * Persistence: Contains all data access.
    * Identity: Injects authentication and JWToken
    * Shared: Manage some services as Mail Service and Date Time Service
* Api: The presentation layer.

## How to run the app?
* Ensure the connectionstrings in the __appsettings.json__ file is correct.
    * __DocumentMetadataConnection__: Connection to the document metadata DB, includes User, Role, Group, etc tables.
    * __DocumentDataConnection__:  Connection to the blob store, contains a document table to store the file content.
* Run the each script file in the correct DB:
    * __MetadataRepoMigration.sql__ For the metadata DB, it script initialize the database objects (tables, index, procedures)
    * __FileRepoMigration.sql__ For the DB which contains the file content.

Finally, from the root folder execute the following command:

```
dotnet "run" "--project" "c:\Source\repos\DocManager\DocManager.Api\DocManager.Api.csproj" 
```

To debug just press __F5__ keyboard.

The following URLs will be available:

* Swagger documentation: https://localhost:5001/swagger
* Healthcheck: https://localhost:5001/health
