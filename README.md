# T Dining API and Angular frontend

This repository implements a restaurant backend for **T Dining** (target 60–70 seats) using a hexagonal architecture, SQLite persistence, and a transactional outbox. The independent Angular application in `frontend/` follows the dashboard, order, menu, and billing structure of the referenced [Cafe Management System Angular/Spring Boot frontend](https://github.com/Bahri-Adem/Cafe-Management-System-Angular-SpringBoot/tree/main/Frontend), adapted to T Dining's API and restaurant workflows.

## Architecture

```text
src/TDining.Api
  Domain/Entities, Services, Events
  Application/Ports, UseCases, DTOs
  Infrastructure/Persistence, Outbox
  Program.cs
frontend
  Angular CLI standalone application
```

The API supports orders and payments, ingredient inventory, dining tables, daily reports, and reservations with a 70-seat capacity guard. The Angular workspace provides overview, orders and checkout, floor and table status, menu, reservations, inventory, and reports.

## Run locally

Start the API:

```bash
dotnet run --project src/TDining.Api/TDining.Api.csproj --urls http://localhost:5000
```

In another terminal, start Angular:

```bash
cd frontend
npm install
npm start
```

Open `http://localhost:4200`. Angular's development proxy forwards `/api/*` calls to the .NET API on port 5000. For a production build, run `npm run build` in `frontend/` and deploy `frontend/dist/tdining/browser` independently from the API.

The database is created at `src/TDining.Api/tdining.db` the first time the API starts.

## API endpoints

- `GET /` and `GET /api` — service summary
- `GET /tables`
- `PATCH /tables/{tableCode}/status`
- `GET /menu`
- `GET /inventory`
- `GET /orders`
- `POST /orders`
- `POST /orders/{orderId}/items/add`
- `POST /orders/{orderId}/items/remove`
- `POST /orders/{orderId}/send-to-kitchen`
- `POST /orders/{orderId}/mark-served`
- `POST /orders/{orderId}/payments`
- `POST /orders/{orderId}/close`
- `GET /reservations`
- `POST /reservations`
- `GET /reports/daily/{date}`
