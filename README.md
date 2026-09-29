# Mechanical Designs API

An ASP.NET Core backend for a decor and custom-design store, built with .NET 10, PostgreSQL, and Entity Framework Core.

## Features

- JWT authentication and role-based administration
- Product catalog, categories, images, and reviews
- Shopping carts, orders, and delivery fees
- Custom design requests and advertisements
- Email verification and password recovery

## Structure

The solution contains API, Application, Domain, and Infrastructure projects under `src/`.

## Development

Install the .NET 10 SDK and configure `ConnectionStrings:DefaultConnection` and `Jwt:Key` through the API project's .NET user secrets. Email workflows also require SMTP configuration.

```sh
dotnet restore
dotnet run --project src/MechanicalDesigns.Api
```

The default local address is `http://localhost:5210`. Swagger is available at `/swagger` in Development. `/health` checks that the HTTP service is running; it does not test database or email connectivity.

## Hosting

The root Dockerfile builds the production API. Supply credentials through environment variables, apply database migrations before release, and attach persistent storage at `/app/wwwroot/uploads` for uploaded images. Configure allowed frontend origins with `AllowedOrigins__0`, `AllowedOrigins__1`, and so on.

Secrets, local documentation, and uploaded files are excluded from version control.
