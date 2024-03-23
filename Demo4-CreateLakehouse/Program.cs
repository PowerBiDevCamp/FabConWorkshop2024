
string workspaceName = "Demo4 - CreateLakehouse";

// Setup: View capacities to update AppSettings.cs with PremiumCapacityId
// CustomerTenantBuilder.ViewCapacities();

// Test1
// CustomerTenantBuilder.CreateCustomerTenantForNotebookDemo(workspaceName);

// Test2
CustomerTenantBuilder.CreateCustomerTenantWithLakehouse(workspaceName);