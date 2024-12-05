# Init variables
resourceGroupName=
accountName=

# Get role definitions
az cosmosdb sql role definition list --resource-group $resourceGroupName --account-name $accountName

# Save the role id for Cosmos DB Built-in Data Contributor
roleDefinitionId=$(az cosmosdb sql role definition list --resource-group $resourceGroupName --account-name $accountName --query "[?name=='00000000-0000-0000-0000-000000000002'].id" -o tsv)

# List roles
az cosmosdb sql role definition list --resource-group $resourceGroupName --account-name $accountName

# Get Cosmos DB account ID
accountId=$(az cosmosdb show --resource-group $resourceGroupName --name $accountName --query "id" --output tsv)

# Get signed in principal ID or set it to a specific user
principalId=$(az ad signed-in-user show --query id -o tsv)

# Assign the role 
az cosmosdb sql role assignment create --resource-group $resourceGroupName --account-name $accountName --role-definition-id $roleDefinitionId --principal-id $principalId --scope $accountId

# Confirm the role assignment
az cosmosdb sql role assignment list --resource-group $resourceGroupName --account-name $accountName

