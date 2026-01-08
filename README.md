## General
Copy .env.example, rename the copy to .env and change the variables inside as you want them.

## Docker Setup

To run the entire application with Docker:

```bash
docker compose up --build
```

This will start:
- SQL Server database on port 1433
- Backend API on port 8080
- Frontend on port 3000

Access the application at:
- Frontend: http://localhost:3000
- Backend API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

To stop all services:
```bash
docker compose down
```

To rebuild after code changes:
```bash
docker compose up --build
```

## Local Dev

### Frontend
To install the frontend you need to use [NodeJS](https://nodejs.org/en/download/) to run the application locally and install packages used.

To setup the project for the first time, in the `/frontend` folder, run `npm i`.

Following this, run `npm run dev` to run the local environment.

### Backend

Install [dotnet](https://dotnet.microsoft.com/en-us/download) (dotnet9)

To run the backend go to the `/backend` folder and run `dotnet run`.

### Database

Run db with `docker compose up mssql` (or use the entire Docker for front back and db under Docker setup)
