build:
	@dotnet build

run: build
	@dotnet run --project src/EFCore.Console/EFCore.Console.csproj

down:
	@docker-compose down -v

up:
	@docker-compose up -d

test:
	@dotnet test