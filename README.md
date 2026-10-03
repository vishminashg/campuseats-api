# CampusEats API (ASP.NET Core 8)

REST API for the CampusEats application (SE3090 Lab 11).

| Endpoint | Description |
|----------|-------------|
| `GET /` | API info |
| `GET /health` | Health check |
| `GET /api/status` | Environment + database status |
| `GET /api/menu` | Menu items (PostgreSQL, or in-memory if no DB configured) |

## Environment variables (set on the hosting platform – never commit values)
| Variable | Purpose |
|----------|---------|
| `PORT` | Port to listen on (set by the platform; default 8080) |
| `ConnectionStrings__Default` | PostgreSQL connection string |
| `Cors__AllowedOrigins` | Frontend origin(s), comma separated, e.g. `https://my-app.vercel.app` |
| `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience` | JWT settings (production secret differs from dev) |

## Run locally
```
dotnet run
```
