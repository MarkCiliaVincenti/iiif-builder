# iiif-builder

IIIF Presentation API Server and related services.

See [iiif-builder-infrastructure](https://github.com/wellcomecollection/iiif-builder-infrastructure) repo for related infrastructure.

## Projects

There are 4 main projects that serve as deployable artifacts, detailed below.

> All apps explose health-checks on `/management/healthcheck`

### Wellcome.Dds.Server

API. Used for serving IIIF resources (TODO) and provisioning roles to DLCS by authenticating users with Sierra via REST API.

### Wellcome.Dds.Dashboard

UI for monitoring and managing workflow process. Required wellcomecloud account for access.

### WorkflowProcessor

Background service for handling requested workflow jobs from Goobi. 

### DlcsJobProcessor

background service for handling requested dlcs jobs, created by WorkflowProcessor.

## Tech :robot:

* dotnet core 3.1
* EF Core using PostgreSQL
* [AWS SDK](https://github.com/aws/aws-sdk-net/)

Test projects use:
* [xUnit](https://xunit.net/)
* [FluentAssertions](https://fluentassertions.com/) for assertions
* [FakeItEasy](https://fakeiteasy.github.io/) for fakes/mocks

## Deploying :rocket:

All services are deployed as Docker containers via Jenkins. There is a Dockerfile-* and Jenkinsfile-* per entrypoint. 

All environments use the same Jenkinsfile, with parameters controlled in [Jenkins](https://jenkins.dlcs.io/) (not publicly available). All Jenkinsfiles use [pipeline syntax](https://www.jenkins.io/doc/book/pipeline/syntax/) use the same stages:

* "Fetch" - checkout from Git (branch controlled in Jenkins).
* "Image" - `docker build`, tagged `:jenkins`.
* "Tag" - `docker tag` - tagged with commit SHA1 and environment-specific tag.
* "Push" - `docker push` - push changes to ECR.
* "Bounce" - Update ECS service and wait until stable. Will fail if not stable after 10mins.

## Running

The [`dotnet cli`](https://docs.microsoft.com/en-us/dotnet/core/tools/) can be used to run the apps outside of an IDE.

> All of the following samples commands assume a starting folder of `/src/Wellcome.Dds`.

### Apps

Applications can be run using the following commands.

```bash
# API http://localhost:8084
dotnet run -p ./Wellcome.Dds.Server/Wellcome.Dds.Server.csproj

# Dashboard http://localhost:8085/dash
dotnet run -p ./Wellcome.Dds.Dashboard/Wellcome.Dds.Dashboard.csproj

# WorkflowProcessor http://localhost:8081 (http for healthchecks only)
dotnet run -p ./WorkflowProcessor/WorkflowProcessor.csproj

# DlcsJobProcessor http://localhost:8082 (http for healthchecks only)
dotnet run --project ./DlcsJobProcessor/DlcsJobProcessor.csproj
```

> To run applications some secrets will need to be added to relevant `appsettings.Development.json` (e.g. db-connection strings and API keys).

### Tests

Tests can be run using the following commands:

```bash
# Run ALL tests in solution
dotnet test

# Run all tests, except those with "Database" category.
# These can be slow as they run a postgres Docker container and apply migrations
dotnet test --filter Category\!=Database

# Run test for specific project
dotnet test ./Utils.Tests/Utils.Tests.csproj
```