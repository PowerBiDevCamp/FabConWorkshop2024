string workspaceName = "Demo7 - CreateShortcut";

// Setup: View capacities to update AppSettings.cs with PremiumCapacityId
//CustomerTenantBuilder.ViewCapacities();

// Test1
//CustomerTenantBuilder.CreateCustomerTenantWithEmptyLakehouse();

// Test2
// CustomerTenantBuilder.GetShortcutForLakehouse();

// Test3
// CustomerTenantBuilder.CreateCustomerTenantWithShortcutExamples();

// Test4
CustomerTenantBuilder.CreateCustomerTenantWithWarehouse(workspaceName);

// Test5
// CustomerTenantBuilder.CreateCustomerTenantAndLoadTaxiData();