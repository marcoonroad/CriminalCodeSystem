.PHONY: clean

clean:
	@ dotnet clean

restore:
	@ dotnet restore

build: restore
	@ dotnet build

migrations-vendor:
	@ dotnet tool install --global dotnet-ef || true

migrations-create:
	@ dotnet ef migrations add CreateStatuses                  --context StatusContext        --project CriminalCodeSystem
	@ dotnet ef migrations add CreateUsers          --no-build --context UserContext          --project CriminalCodeSystem
	@ dotnet ef migrations add CreateDisabledTokens --no-build --context DisabledTokenContext --project CriminalCodeSystem
	@ dotnet ef migrations add CreateCriminalCodes  --no-build --context CriminalCodeContext  --project CriminalCodeSystem

migrations-perform:
	@ dotnet ef database update --project CriminalCodeSystem --context StatusContext
	@ dotnet ef database update --project CriminalCodeSystem --context UserContext          --no-build
	@ dotnet ef database update --project CriminalCodeSystem --context DisabledTokenContext --no-build
	@ dotnet ef database update --project CriminalCodeSystem --context CriminalCodeContext  --no-build

migrations-generate-executables:
	@ rm -rf migrations/executables
	@ mkdir -p migrations && mkdir -p migrations/executables
	@ echo "=== Generating migration binaries under ./migrations/executables ==="
	@ dotnet ef migrations bundle            --force --project CriminalCodeSystem --context StatusContext        --self-contained --output migrations/executables/migration_001_status.bin
	@ dotnet ef migrations bundle --no-build --force --project CriminalCodeSystem --context UserContext          --self-contained --output migrations/executables/migration_002_user.bin
	@ dotnet ef migrations bundle --no-build --force --project CriminalCodeSystem --context DisabledTokenContext --self-contained --output migrations/executables/migration_003_disabled_token.bin
	@ dotnet ef migrations bundle --no-build --force --project CriminalCodeSystem --context CriminalCodeContext  --self-contained --output migrations/executables/migration_004_criminal_code.bin
	@ chmod a+x -R migrations/executables

migrations-generate-scripts:
	@ rm -rf migrations/scripts
	@ mkdir -p migrations &&  mkdir -p migrations/scripts
	@ echo "=== Generating migration SQL scripts under ./migrations/scripts ==="
	@ dotnet ef migrations script            --context StatusContext        --project CriminalCodeSystem --idempotent --output migrations/scripts/migration_001_status.sql
	@ dotnet ef migrations script --no-build --context UserContext          --project CriminalCodeSystem --idempotent --output migrations/scripts/migration_002_user.sql
	@ dotnet ef migrations script --no-build --context DisabledTokenContext --project CriminalCodeSystem --idempotent --output migrations/scripts/migration_003_disabled_token.sql
	@ dotnet ef migrations script --no-build --context CriminalCodeContext  --project CriminalCodeSystem --idempotent --output migrations/scripts/migration_004_criminal_code.sql

run: build
	@ dotnet run --project CriminalCodeSystem

test: build
	@ FORCE_IN_MEMORY_STORAGE=1 dotnet test
