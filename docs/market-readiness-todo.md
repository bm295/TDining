# T Dining Market Readiness To-Do List

## Verdict

T Dining is **not ready to market yet**. The repository has a useful backend prototype for restaurant operations, but it is still missing production basics such as automated tests, authentication, deployment configuration, migrations, observability, and several operational workflows required before selling or launching to real restaurants.

## Current product strengths

- Hexagonal backend structure is already present with Domain, Application, Infrastructure, and API layers.
- Core restaurant flows already exist for tables, menu, orders, payments, reservations, inventory, and daily reporting.
- SQLite seed data makes local demonstrations easy.
- Transactional outbox foundation exists for future integrations.

## Market readiness gaps

- No automated test project is present.
- No authentication, authorization, or role-based staff access is implemented.
- No frontend or operator-facing UI is present.
- No production deployment files are present.
- No database migration workflow is present.
- Minimal API endpoints do not publish OpenAPI/Swagger documentation.
- Input validation is mostly domain-exception based and not consistently represented as request validation errors.
- Restaurant operations are incomplete: menu CRUD, reservation cancellation, order served/cancelled status updates, refunds, staff management, shift management, receipts, audit logs, and configurable restaurant settings are missing.
- Current payment processing records payments but does not integrate with a payment provider or receipt printer.
- Inventory deduction happens during payment rather than kitchen preparation/serving, which may not match real stock-control workflows.
- No security/privacy controls are documented for customer names and phone numbers.
- No monitoring, structured logs, health checks, backup/restore process, or support runbook is present.

## Phase 1 — Build confidence with tests

- [x] Create test project `tests/TDining.Api.Tests/TDining.Api.Tests.csproj` using xUnit.
- [x] Create class `OrderUseCasesTests` in `tests/TDining.Api.Tests/Application/UseCases/OrderUseCasesTests.cs`.
- [x] Add test `CreateOrderAsync_WhenTableExists_CreatesOrderAndOccupiesTable`.
- [x] Add test `CreateOrderAsync_WhenMenuItemMissing_ReturnsClearFailure`.
- [x] Add test `AddItemAsync_WhenQuantityIsZero_ReturnsValidationFailure`.
- [x] Add test `RemoveItemAsync_WhenRemovingTooManyItems_ReturnsValidationFailure`.
- [x] Add test `SendToKitchenAsync_WhenOrderHasNoLines_ReturnsValidationFailure`.
- [x] Add test `ProcessPaymentAsync_WhenInventoryIsInsufficient_DoesNotRecordPayment`.
- [x] Add test `CloseOrderAsync_WhenNotFullyPaid_ReturnsValidationFailure`.
- [x] Create class `ReservationUseCasesTests` in `tests/TDining.Api.Tests/Application/UseCases/ReservationUseCasesTests.cs`.
- [x] Add test `CreateReservationAsync_WhenCapacityExceeded_ReturnsValidationFailure`.
- [x] Add test `CreateReservationAsync_WhenInputHasWhitespace_TrimsCustomerAndPhone`.
- [x] Create class `ReportingUseCasesTests` in `tests/TDining.Api.Tests/Application/UseCases/ReportingUseCasesTests.cs`.
- [x] Add test `GetDailyReportAsync_ReturnsSalesAndOrderCountsForDate`.
- [x] Create class `ApiEndpointTests` in `tests/TDining.Api.Tests/Api/ApiEndpointTests.cs` using `WebApplicationFactory`.
- [x] Add test `GetMenu_ReturnsSeededMenu`.
- [x] Add test `PostOrder_WithInvalidMenuItem_ReturnsBadRequestProblemDetails`.
- [x] Add GitHub Actions workflow `.github/workflows/ci.yml` to run restore, build, test, and format checks.

## Phase 2 — Add production-ready validation and API contracts

- [ ] Create class `ValidationException` in `src/TDining.Api/Application/Common/ValidationException.cs`.
- [ ] Create class `CreateOrderCommandValidator` in `src/TDining.Api/Application/Validation/CreateOrderCommandValidator.cs`.
- [ ] Add validation rule: `TableCode` is required and max length 20.
- [ ] Add validation rule: `CustomerName` is required and max length 200.
- [ ] Add validation rule: order must contain at least one item.
- [ ] Add validation rule: every order line quantity must be greater than zero.
- [ ] Create class `ProcessPaymentCommandValidator` in `src/TDining.Api/Application/Validation/ProcessPaymentCommandValidator.cs`.
- [ ] Add validation rule: payment amount must be greater than zero.
- [ ] Create class `CreateReservationCommandValidator` in `src/TDining.Api/Application/Validation/CreateReservationCommandValidator.cs`.
- [ ] Add validation rule: reservation customer name is required and max length 200.
- [ ] Add validation rule: phone number is required and max length 50.
- [ ] Add validation rule: guest count must be between 1 and 70.
- [ ] Add validation rule: booking time must be in the future.
- [ ] Update `Program.cs` to return RFC 7807 Problem Details for validation failures.
- [ ] Add Swagger/OpenAPI services in `Program.cs`.
- [ ] Add endpoint summaries, descriptions, request examples, and response examples for all public endpoints.
- [ ] Create `docs/api-contract.md` documenting every endpoint, request body, response body, and error response.

## Phase 3 — Complete restaurant operations

- [ ] Create interface `IMenuUseCases` in `src/TDining.Api/Application/Ports/In/IMenuUseCases.cs`.
- [ ] Create class `MenuUseCases` in `src/TDining.Api/Application/UseCases/MenuUseCases.cs`.
- [ ] Add command record `CreateMenuItemCommand` in `src/TDining.Api/Application/DTOs/Contracts.cs`.
- [ ] Add command record `UpdateMenuItemCommand` in `src/TDining.Api/Application/DTOs/Contracts.cs`.
- [ ] Add endpoint `POST /menu` to create a menu item.
- [ ] Add endpoint `PUT /menu/{menuItemId}` to update menu item details.
- [ ] Add endpoint `PATCH /menu/{menuItemId}/availability` to enable or disable a menu item.
- [ ] Create interface `IInventoryUseCases` in `src/TDining.Api/Application/Ports/In/IInventoryUseCases.cs`.
- [ ] Create class `InventoryUseCases` in `src/TDining.Api/Application/UseCases/InventoryUseCases.cs`.
- [ ] Add command record `CreateInventoryItemCommand` in `src/TDining.Api/Application/DTOs/Contracts.cs`.
- [ ] Add command record `AdjustInventoryCommand` in `src/TDining.Api/Application/DTOs/Contracts.cs`.
- [ ] Add endpoint `POST /inventory` to create an inventory item.
- [ ] Add endpoint `POST /inventory/{inventoryItemId}/adjustments` to increase or decrease inventory with a reason.
- [ ] Create entity `InventoryAdjustment` in `src/TDining.Api/Domain/Entities/InventoryAdjustment.cs`.
- [ ] Create endpoint `POST /orders/{orderId}/served` to mark an order as served.
- [ ] Create endpoint `POST /orders/{orderId}/cancel` to cancel an order.
- [ ] Create endpoint `POST /orders/{orderId}/refunds` to record a refund.
- [ ] Create endpoint `POST /reservations/{reservationId}/cancel` to cancel a reservation.
- [ ] Create endpoint `POST /reservations/{reservationId}/seat` to convert a reservation into an occupied table/order.
- [ ] Create entity `StaffMember` in `src/TDining.Api/Domain/Entities/StaffMember.cs`.
- [ ] Create entity `StaffShift` in `src/TDining.Api/Domain/Entities/StaffShift.cs`.
- [ ] Create interface `IStaffUseCases` in `src/TDining.Api/Application/Ports/In/IStaffUseCases.cs`.
- [ ] Create class `StaffUseCases` in `src/TDining.Api/Application/UseCases/StaffUseCases.cs`.
- [ ] Add endpoints for staff creation, staff listing, shift clock-in, and shift clock-out.
- [ ] Create entity `Receipt` in `src/TDining.Api/Domain/Entities/Receipt.cs`.
- [ ] Create class `ReceiptUseCases` in `src/TDining.Api/Application/UseCases/ReceiptUseCases.cs`.
- [ ] Add endpoint `GET /orders/{orderId}/receipt` to return printable receipt data.

## Phase 4 — Security and privacy

- [ ] Add ASP.NET Core authentication to `Program.cs`.
- [ ] Create class `CurrentUser` in `src/TDining.Api/Application/Security/CurrentUser.cs`.
- [ ] Create enum `StaffRole` in `src/TDining.Api/Domain/Entities/StaffRole.cs` with values `Admin`, `Manager`, `Cashier`, `Server`, and `Kitchen`.
- [ ] Add authorization policy `CanManageMenu` for managers and admins.
- [ ] Add authorization policy `CanTakePayment` for cashiers, managers, and admins.
- [ ] Add authorization policy `CanViewReports` for managers and admins.
- [ ] Protect all write endpoints with authorization.
- [ ] Protect customer phone numbers from unauthorized roles in reservation responses.
- [ ] Create migration or script to add staff login tables.
- [ ] Add password hashing or external identity-provider integration.
- [ ] Add audit entity `AuditLogEntry` in `src/TDining.Api/Domain/Entities/AuditLogEntry.cs`.
- [ ] Record audit logs for menu changes, inventory adjustments, payments, refunds, and reservation changes.
- [ ] Create `docs/security-and-privacy.md` covering customer data handling and retention.

## Phase 5 — Database, deployment, and operations

- [ ] Add EF Core design package to `src/TDining.Api/TDining.Api.csproj`.
- [ ] Create initial EF Core migration in `src/TDining.Api/Infrastructure/Persistence/Migrations`.
- [ ] Add command documentation for applying migrations in production.
- [ ] Move seed data behind an environment flag so production does not automatically reseed demo data.
- [ ] Add `Dockerfile` for the API.
- [ ] Add `docker-compose.yml` for local API and database startup.
- [ ] Add `appsettings.Production.json` with safe logging defaults and no secrets.
- [ ] Add `/health` endpoint for service health.
- [ ] Add `/ready` endpoint that checks database connectivity.
- [ ] Add structured JSON logging.
- [ ] Add correlation ID middleware.
- [ ] Add metrics for request count, error count, order count, payment count, and outbox failures.
- [ ] Create `docs/production-runbook.md` with startup, shutdown, backup, restore, and incident steps.
- [ ] Create `docs/backup-restore.md` for SQLite or the chosen production database.
- [ ] Create `docs/release-checklist.md` for market launch readiness.

## Phase 6 — User experience and sales readiness

- [ ] Decide whether the first marketable release is API-only, web app, or tablet POS.
- [ ] Create web frontend project `src/TDining.Web` if the product will be sold with an operator UI.
- [ ] Create login screen for staff.
- [ ] Create table floor screen showing available, occupied, reserved, and cleaning tables.
- [ ] Create order-taking screen for servers.
- [ ] Create kitchen display screen for preparing orders.
- [ ] Create cashier payment screen.
- [ ] Create reservation calendar screen.
- [ ] Create manager dashboard screen with daily sales and active orders.
- [ ] Create inventory adjustment screen.
- [ ] Create menu management screen.
- [ ] Create printable receipt template.
- [ ] Create demo script in `docs/demo-script.md`.
- [ ] Create onboarding guide in `docs/onboarding-guide.md`.
- [ ] Create pricing and packaging notes in `docs/pricing-packaging.md`.
- [ ] Create support FAQ in `docs/support-faq.md`.

## Phase 7 — Final go-to-market checks

- [ ] Run full automated test suite in CI.
- [ ] Run a manual end-to-end demo: create reservation, seat guest, create order, send to kitchen, serve, pay, close, print receipt, view report.
- [ ] Verify role permissions for admin, manager, cashier, server, and kitchen users.
- [ ] Verify invalid inputs return clear validation messages.
- [ ] Verify database backup and restore on a clean environment.
- [ ] Verify deployment can be rebuilt from source using documented commands.
- [ ] Verify production logs contain no payment secrets or sensitive customer data.
- [ ] Verify customer phone numbers are hidden from unauthorized users.
- [ ] Verify the application can support expected restaurant traffic for 60-70 seats.
- [ ] Collect feedback from at least one real restaurant operator before launch.
