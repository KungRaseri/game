# Game.Economy - CQS Architecture

This project implements the economy and treasury management system for the shop keeper game using the Command Query Separation (CQS) pattern.

## Architecture Overview

The project follows the CQS pattern to separate operations that change state (Commands) from operations that retrieve data (Queries).

### Directory Structure

```
Game.Economy/
├── Commands/           # State-changing operations
├── Queries/           # Data retrieval operations  
├── Handlers/          # Individual command/query handlers
├── Models/            # Domain models and entities
├── Systems/           # Core business logic systems
├── Data/              # Factories and templates
└── Extensions/        # Dependency injection setup
```

### Core Components

#### Commands (State Changes)
- `AddRevenueCommand` - Add revenue to treasury
- `ProcessExpenseCommand` - Process business expenses
- `MakeInvestmentCommand` - Make shop investments
- `SetMonthlyBudgetCommand` - Set expense budgets
- `ProcessRecurringExpensesCommand` - Process recurring expenses

#### Queries (Data Retrieval)
- `GetFinancialSummaryQuery` - Get comprehensive financial data
- `GetCurrentGoldQuery` - Get current treasury balance
- `GetExpenseHistoryQuery` - Get expense history with filtering
- `GetAvailableInvestmentsQuery` - Get investment opportunities
- `GetCompletedInvestmentsQuery` - Get completed investments
- `GetRecommendedInvestmentsQuery` - Get AI-recommended investments
- `GetMonthlyBudgetQuery` - Get budget for expense type

#### Systems
- `TreasuryManager` - Core treasury and financial management
- `EconomyService` - High-level CQS-based API

#### Models
- `ShopExpense` - Individual expense transactions
- `InvestmentOpportunity` - Shop improvement investments
- `FinancialSummary` - Comprehensive financial analysis
- `ExpenseCategory` - Expense categorization data

## Usage Examples

### Basic Usage with EconomyService

```csharp
// Register services
services.AddEconomyServices();
services.AddCqsServices(); // From Game.Core

// Use the service
var economyService = serviceProvider.GetService<EconomyService>();

// Add revenue
await economyService.AddRevenueAsync(100m, "Item Sale");

// Process expense
await economyService.ProcessExpenseAsync(
    ExpenseType.Rent, 50m, "Monthly rent payment");

// Get financial summary
var summary = await economyService.GetFinancialSummaryAsync();

// Make investment
await economyService.MakeInvestmentAsync("display-upgrade-1");
```

### Direct CQS Usage

```csharp
// Register services
services.AddEconomyServices();

// Use the dispatcher directly
var dispatcher = serviceProvider.GetService<IDispatcher>();

// Execute commands
var addRevenueCommand = new AddRevenueCommand { Amount = 100m, Source = "Sales" };
await dispatcher.DispatchCommandAsync(addRevenueCommand);

// Execute queries
var query = new GetCurrentGoldQuery();
var currentGold = await dispatcher.DispatchQueryAsync<GetCurrentGoldQuery, decimal>(query);
```

## Investment System

The economy supports a sophisticated investment system with:

- **Investment Opportunities** - Predefined shop improvements
- **ROI Calculations** - Automatic return on investment analysis
- **Risk Categories** - Investment risk assessment
- **Payback Periods** - Time-based investment planning

### Investment Types
- Display Upgrades
- Shop Expansion
- Staff Hiring
- Security Upgrades
- Marketing Campaigns
- Inventory Expansion
- Aesthetic Upgrades
- Technology Upgrades
- Storage Upgrades

## Budget Management

The system includes comprehensive budget management:

- **Monthly Budgets** - Set spending limits by expense type
- **Budget Templates** - Predefined budget configurations
- **Budget Tracking** - Automatic budget compliance monitoring
- **Financial Alerts** - Warnings for budget overruns

### Budget Templates
- **Startup** - Conservative budgets for new shops
- **Established** - Balanced budgets for growing shops
- **Premium** - High-end budgets for luxury shops
- **Conservative** - Minimal spending approach
- **Growth** - Aggressive expansion budgets

## Financial Analytics

The system provides detailed financial analytics:

- **Profit Margins** - Revenue vs expense analysis
- **Cash Flow** - Daily/monthly cash flow tracking
- **Runway Analysis** - Time until bankruptcy projection
- **Health Score** - Overall financial health rating
- **Insights** - AI-generated financial recommendations

## Events

The treasury system emits events for integration:

- `TreasuryChanged` - Fired when treasury balance changes
- `ExpenseProcessed` - Fired when expense is processed
- `InvestmentCompleted` - Fired when investment is made
- `FinancialAlert` - Fired for financial warnings

## Testing

The CQS architecture enables easy testing:

- Mock `ITreasuryManager` for unit testing handlers
- Mock `IDispatcher` for testing the service layer
- Individual handlers can be tested in isolation
- Commands and queries are simple DTOs

## Integration

To integrate with other game systems:

1. **Register Services**: Add `services.AddEconomyServices()` to DI container
2. **Listen to Events**: Subscribe to treasury events for UI updates
3. **Use EconomyService**: High-level API for common operations
4. **Direct CQS**: Use dispatcher for custom operations

## Configuration

The system supports various configuration options:

- Initial treasury amounts
- Default investment opportunities
- Budget templates
- Financial health thresholds
- Alert conditions
