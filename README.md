# dev-fest-sdq-2023 Workshop

# Prerequisites
## dotnet 7 SDK or Higher
1. [Windows](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
2. [MacOS](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
3. [Linux](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## Test Tools
1. [NUnit 3](https://docs.nunit.org/articles/nunit/intro.html): Testing Framework
2. [Moq](https://github.com/devlooped/moq/wiki/Quickstart) : Mocking / test doubles library
3. [FluentAssertions](https://fluentassertions.com/introduction): Assertions library using Fluent APIs

## Design 
Simplified [_Command and Query Responsibility Segregation_ (CQRS)](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs) Implementation. No UI, no real Infrastructure layer (just Interfaces here). So we have Commands and Queries with business logic, when those need to communicate with external services such as Databases, they go to an Interface without implementation. 

Test code creates doubles, using Moq, for all Infrastructure elements so Commands and Queries can be validated without using any real external dependency.


## Directory Structure
One solution (sln file on root), one class library project (`AirBnBDevFest23.Domain`), and one test project (`AirBnBDevFest23.Facts`).

Inside `Domain` we have app-specific business logic (Commands and Queries), also we have Models (mostly simple DTOs), Infrastructure contracts definitions (C# Interfaces), and some supporting contracts, such as the base Queries and Commands contracts.

```
/dev-fest-sdq-2023
├── AirBnBDevFest23.Domain
│   ├── Commands
│   ├── Infrastructure
│   ├── Models
│   │── Queries
│
├── AirBnBDevFest23.Domain.Facts
├── dev-fest-sdq-2023.sln
```

## Check You Are Good To Go
1. `dotnet --version` outputs something like `7.x.yyy` (V7 SDK)
2. From root run `dotnet test` and expected something like 
```
  Determining projects to restore...
  All projects are up-to-date for restore.
  AirBnB.DevFest23.Domain -> ~/dev-fest-sdq-2023/AirBnB.DevFest23.Domain/bin/Debug/net7.0/AirBnB.DevFest23.Domain.dll
  AirBnB.DevFest23.Domain.Facts -> ~/dev-fest-sdq-2023/AirBnB.DevFest23.Domain.Facts/bin/Debug/net7.0/AirBnB.DevFest23.Domain.Facts.dll
Test run for ~/dev-fest-sdq-2023/AirBnB.DevFest23.Domain.Facts/bin/Debug/net7.0/AirBnB.DevFest23.Domain.Facts.dll (.NETCoreApp,Version=v7.0)
Microsoft (R) Test Execution Command Line Tool Version 17.7.0-preview-23364-03+bc17bb9693cfc4778ded51aa0ab7f1065433f989 (arm64)
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:     999, Skipped:     0, Total:     1, Duration: 28 ms - AirBnB.DevFest23.Domain.Facts.dll (net7.0)
```

The last line should output no failing tests, and should not see any compilation errors.

🎉 Happy Coding! 🎉