#!/bin/bash

# Script to generate .env file from user secrets
# This keeps secrets out of source control while making them available to Docker

set -e

PROJECT_PATH="./Neba.Web.Server/Neba.Web.Server.csproj"
ENV_FILE=".env"

echo "Extracting secrets from user secrets..."

# Get the subscription key from user secrets
SUBSCRIPTION_KEY=$(dotnet user-secrets list --project "$PROJECT_PATH" | grep "AzureMaps:SubscriptionKey" | cut -d'=' -f2- | xargs)

# Check if we got a value
if [ -z "$SUBSCRIPTION_KEY" ]; then
    echo "Error: AzureMaps:SubscriptionKey not found in user secrets"
    echo "Please set it with: dotnet user-secrets set 'AzureMaps:SubscriptionKey' 'your-key' --project $PROJECT_PATH"
    exit 1
fi

# Create .env file
cat > "$ENV_FILE" << EOF
# Auto-generated from user secrets - DO NOT COMMIT
# Generated on: $(date)

AZURE_MAPS_SUBSCRIPTION_KEY=$SUBSCRIPTION_KEY
AZURE_MAPS_ACCOUNT_ID=
EOF

echo "✓ Created $ENV_FILE with secrets from user secrets"
echo "✓ You can now run: docker compose up"
