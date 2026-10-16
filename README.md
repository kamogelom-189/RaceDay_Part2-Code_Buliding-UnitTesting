# RaceDay - Part 2: Code Building & Unit Testing

RaceDay is a C# project focused on practicing build automation and unit testing techniques. This repository contains the code, unit tests, and instructions to build, run, and test the RaceDay project using the .NET CLI.

## Youtube
https://youtu.be/SWEUkmfFz-g


## What this repo contains

- Source code for the RaceDay application (C#).
- Unit tests covering core functionality.
- Build and test configuration suitable for .NET SDK (dotnet CLI).

## Prerequisites

- .NET SDK 6.0 or later installed: https://dotnet.microsoft.com/download
- A code editor such as Visual Studio, Visual Studio Code, or Rider.

Verify your SDK installation by running:

```bash
dotnet --version
```

## Build

To restore dependencies and build the solution or projects:

```bash
dotnet restore
dotnet build --configuration Release
```

## Run

If the repository contains a runnable project (for example `RaceDay` console app), run it with:

```bash
dotnet run --project ./src/YourProjectName
```

Replace `./src/YourProjectName` with the actual path to the executable project in this repository.

## Tests

Run the unit tests with the .NET test runner:

```bash
dotnet test --configuration Release
```

This will discover and execute the test projects included in the repository and report results.

## Project structure (example)

- src/           - Application source code
- tests/         - Unit test projects
- README.md      - This file

Adjust paths above to match the actual layout in this repository.

## Contributing

Contributions, bug reports, and improvements are welcome. Typical workflow:

1. Fork the repository.
2. Create a feature branch: `git checkout -b feat/my-feature`
3. Make changes and add tests where appropriate.
4. Run `dotnet test` and ensure all tests pass.
5. Open a pull request with a clear description of changes.

## Code style

- Follow common C# conventions and .NET best practices.
- Use meaningful names and small, focused methods.
- Keep unit tests isolated and deterministic.

## License
