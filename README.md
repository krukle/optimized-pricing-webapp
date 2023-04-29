# Optimized Pricing Web App


![screenshot](https://user-images.githubusercontent.com/72295233/235315900-6e9a70a4-e32b-41c2-a1ab-3cc005837bbf.png)

A web application that displays optimized product prices over time, using .NET 7.0 for the back-end, Angular for the front-end, and a provided dataset. The application shows a table with the current optimized selling price for each product (also called SKU) and how it has changed over time.

## How to run

1. Clone the repository

```bash
git clone git@github.com:krukle/optimized-pricing-webapp.git
```

2. Navigate to the root folder of the project (where the `.csproj` or `.sln` file is located):

```bash
cd optimized-pricing-webapp/
```

3. Restore the required NuGet packages:

```bash
dotnet restore
```

4. As this project uses Angular, navigate to the folder containing the `package.json` file, and install the necessary Node.js packages:

```bash
cd src/ClientApp
npm install
```

5. Head back to the projects root folder and build it:

```bash
cd ..
dotnet build
```

6. Update the database to the lastest migration. Creates the database if it didnt exist already:

> **Note**
>
> Make sure you have the [CLI tools for Entity Framework](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) installed complete with the Microsoft.EntityFrameworkCore.Design package.

```bash
dotnet ef database update
```

7. Run the project:

```
dotnet run
```

8. The webapp will be hosted over at https://localhost:7233. 

## Features

- Load product prices from a CSV dataset
- Calculate optimized prices based on market, currency, and time period
- Display optimized prices in a tabular format per product using Angular
- Follow SOLID and DRY principles for clean and maintainable back-end code
- Utilize MSSQL as the database system

## Prerequisites

- .NET SDK 7.0
- Node.js
- CLI tools for Entity Framework Core

## Built With

- [.NET 7.0](https://dotnet.microsoft.com/download/dotnet/7.0) - The back-end framework used
- [Angular](https://angular.io/) - The front-end framework used
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) - The command-line interface (CLI) tools for Entity Framework Core to perform design-time development tasks.
- [MSSQL](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb?view=sql-server-ver16) -The database system used

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
