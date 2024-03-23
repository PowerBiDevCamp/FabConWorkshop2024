
string workspaceName = "Demo8 - CreateDataPipeline";

// Setup: View capacities to update AppSettings.cs with PremiumCapacityId
// CustomerTenantBuilder.ViewCapacities();

// Test1
CustomerTenantBuilder.CreateCustomerTenantWithDataPipeline(workspaceName);