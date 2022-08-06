[![CodeQL](https://github.com/msx752/sample-netcore-api/actions/workflows/codeql-analysis.yml/badge.svg?branch=master)](https://github.com/msx752/sample-netcore-api/actions/workflows/codeql-analysis.yml)

## sample dotnet project
Sample Online Movie Store Website built on microservice architecture with AngularUI using Ocelot Gateway

## How to run demo on the docker linux containers
``` css
cd netCoreAPITest
docker-compose down
docker-compose build --no-cache
docker-compose up

Website Endpoint: http//:localhost:2000
Gateway API Endpoint: http//:localhost:1010
```

```
Other services reachable over gateway service, checkout 'DockerUp.ps1' and 'docker-compose.yml' files
```

``` css
MongoDB : ports 27017-27019 (not used in the project yet)
RabbitMQ: ports 5672, 15672
```


### Project Contents
- Microservice Architecture 
- Messagging Protocol via MassTransit
  - Pipeline between microservices
- Angular Website Application
  - Base API Client
  - API Response Error Handling
  - Login via Bearer Token
  - Session Managing
- Ocelot Gateway
- Docker Compose File
- Docker Containers
  - Cart API
  - Ocelot Gateway
  - Identity API
  - Movie API
  - Payment API
  - Angular WebApplication
- RESTful API on Controller
- Contract Library
  - Messaging Models
- Core Library
  - Centralized Startup Configurations 
- Global Exception Handler
- Logging API Request / Response ( **NOT COMPLETED** )
  - MongoDb Logging ( **NOT COMPLETED** )
- Single API Response Model
  - Request Tracking Id
  - Measuring Response Time
- OAtuh2 Authorization (JWT Token)
- XUnit Integration Test Project
- Configuration
  - Shared AppSetting Structure
  - Environment Based Appsettings Configuration
- Middlewares
  - JWTMiddleware
  - ExceptionMiddleware
- AutoMapper
  - Dto
  - RequestModel
  - EntityModel
- EntityFrameworkCore
  - BaseEntity
  - Auditlog to SqlTable
- CustomizedDbContext
  - DbContextSeed ( **REQUIRE IMPROVEMENT / NOT COMPLETED** )
- Repository Pattern
  - UnitOfWork
- Dependency Injections
- Swagger
- RabbitMQ
- In Memory Database

**will continue to include the rest**
