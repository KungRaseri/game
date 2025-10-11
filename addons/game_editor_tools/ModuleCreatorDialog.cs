#if TOOLS
using Godot;
using System;
using System.IO;

/// <summary>
/// Dialog for creating new game modules with CQS structure
/// </summary>
[Tool]
public partial class ModuleCreatorDialog : AcceptDialog
{
    private LineEdit? _moduleNameInput;
    private TextEdit? _moduleDescriptionInput;
    private CheckBox? _includeTestProjectCheck;
    private CheckBox? _includeExampleCQSCheck;
    private Label? _statusLabel;

    public override void _Ready()
    {
        Title = "Create New Game Module";
        MinSize = new Vector2I(500, 400);
        
        CreateUI();
        
        // Connect the dialog accepted signal
        Confirmed += OnConfirmed;
    }

    private void CreateUI()
    {
        var vbox = new VBoxContainer();
        AddChild(vbox);

        // Module name input
        vbox.AddChild(new Label { Text = "Module Name:" });
        _moduleNameInput = new LineEdit();
        _moduleNameInput.PlaceholderText = "e.g., Game.NewFeature";
        vbox.AddChild(_moduleNameInput);

        // Module description
        vbox.AddChild(new Label { Text = "Description:" });
        _moduleDescriptionInput = new TextEdit();
        _moduleDescriptionInput.PlaceholderText = "Brief description of the module's purpose...";
        _moduleDescriptionInput.CustomMinimumSize = new Vector2(0, 80);
        vbox.AddChild(_moduleDescriptionInput);

        // Options
        vbox.AddChild(new HSeparator());
        vbox.AddChild(new Label { Text = "Options:" });

        _includeTestProjectCheck = new CheckBox();
        _includeTestProjectCheck.Text = "Create corresponding test project";
        _includeTestProjectCheck.ButtonPressed = true;
        vbox.AddChild(_includeTestProjectCheck);

        _includeExampleCQSCheck = new CheckBox();
        _includeExampleCQSCheck.Text = "Include example CQS components";
        _includeExampleCQSCheck.ButtonPressed = true;
        vbox.AddChild(_includeExampleCQSCheck);

        // Status
        vbox.AddChild(new HSeparator());
        _statusLabel = new Label();
        _statusLabel.Text = "Ready to create module";
        _statusLabel.AddThemeColorOverride("font_color", Colors.Green);
        vbox.AddChild(_statusLabel);
    }

    private void OnConfirmed()
    {
        try
        {
            if (_moduleNameInput == null || _moduleDescriptionInput == null || 
                _includeTestProjectCheck == null || _includeExampleCQSCheck == null || _statusLabel == null)
                return;

            var moduleName = _moduleNameInput.Text.Trim();
            var description = _moduleDescriptionInput.Text.Trim();

            if (string.IsNullOrEmpty(moduleName))
            {
                _statusLabel.Text = "Error: Module name is required";
                _statusLabel.AddThemeColorOverride("font_color", Colors.Red);
                return;
            }

            if (!moduleName.StartsWith("Game."))
            {
                moduleName = "Game." + moduleName;
            }

            var creator = new ModuleCreator();
            var options = new ModuleCreationOptions
            {
                IncludeTestProject = _includeTestProjectCheck.ButtonPressed,
                IncludeExampleCQS = _includeExampleCQSCheck.ButtonPressed,
                Description = description
            };

            var success = creator.CreateModule(moduleName, options);

            if (success)
            {
                _statusLabel.Text = $"Successfully created module {moduleName}";
                _statusLabel.AddThemeColorOverride("font_color", Colors.Green);
                
                // Close dialog after a brief delay
                CallDeferred(nameof(QueueFree));
            }
            else
            {
                _statusLabel.Text = "Error: Failed to create module";
                _statusLabel.AddThemeColorOverride("font_color", Colors.Red);
            }
        }
        catch (Exception ex)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = $"Error: {ex.Message}";
                _statusLabel.AddThemeColorOverride("font_color", Colors.Red);
            }
            GD.PrintErr($"[Module Creator] Error: {ex.Message}");
        }
    }
}

/// <summary>
/// Options for module creation
/// </summary>
public class ModuleCreationOptions
{
    public bool IncludeTestProject { get; set; }
    public bool IncludeExampleCQS { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Creates new game modules with proper CQS structure
/// </summary>
[Tool]
public class ModuleCreator
{
    private readonly string _projectRoot;

    public ModuleCreator()
    {
        _projectRoot = ProjectSettings.GlobalizePath("res://");
    }

    public bool CreateModule(string moduleName, ModuleCreationOptions options)
    {
        try
        {
            var modulePath = Path.Combine(_projectRoot, moduleName);
            
            if (Directory.Exists(modulePath))
            {
                GD.PrintErr($"[Module Creator] Module {moduleName} already exists");
                return false;
            }

            // Create directory structure
            CreateDirectoryStructure(modulePath);
            
            // Create project file
            CreateProjectFile(modulePath, moduleName, options.Description);
            
            // Create README
            CreateReadmeFile(modulePath, moduleName, options.Description);
            
            // Create service registration
            CreateServiceRegistrationFile(modulePath, moduleName);

            if (options.IncludeExampleCQS)
            {
                CreateExampleCQSComponents(modulePath, moduleName);
            }

            if (options.IncludeTestProject)
            {
                CreateTestProject(moduleName, options);
            }

            GD.Print($"[Module Creator] Successfully created module {moduleName}");
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Module Creator] Error creating module: {ex.Message}");
            return false;
        }
    }

    private void CreateDirectoryStructure(string modulePath)
    {
        var directories = new[]
        {
            Path.Combine(modulePath, "Commands"),
            Path.Combine(modulePath, "Queries"),
            Path.Combine(modulePath, "Handlers"),
            Path.Combine(modulePath, "Models"),
            Path.Combine(modulePath, "Systems"),
            Path.Combine(modulePath, "Data"),
            Path.Combine(modulePath, "Extensions")
        };

        foreach (var dir in directories)
        {
            Directory.CreateDirectory(dir);
        }
    }

    private void CreateProjectFile(string modulePath, string moduleName, string description)
    {
        var content = $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <Description>{description}</Description>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""..\\Game.Core\\Game.Core.csproj"" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.Extensions.DependencyInjection.Abstractions"" Version=""8.0.0"" />
  </ItemGroup>

</Project>";

        var projectFileName = $"{moduleName}.csproj";
        File.WriteAllText(Path.Combine(modulePath, projectFileName), content);
    }

    private void CreateReadmeFile(string modulePath, string moduleName, string description)
    {
        var content = $@"# {moduleName}

{description}

## Architecture Overview

This module follows the CQS (Command Query Separation) pattern established in the game architecture.

### Directory Structure

```
{moduleName}/
├── Commands/           # State-changing operations
├── Queries/           # Data retrieval operations  
├── Handlers/          # Individual command/query handlers
├── Models/            # Domain models and entities
├── Systems/           # Core business logic systems
├── Data/              # Factories and templates
└── Extensions/        # Dependency injection setup
```

## Usage

Register the module services in your DI container:

```csharp
services.Add{moduleName.Split('.').Last()}Module();
```

## Commands

(Add your commands here)

## Queries

(Add your queries here)

## Development Notes

- Follow the established CQS patterns from other game modules
- Each command/query should have its own dedicated handler
- Use dependency injection for all service dependencies
- Include comprehensive unit tests for all handlers
";

        File.WriteAllText(Path.Combine(modulePath, "README.md"), content);
    }

    private void CreateServiceRegistrationFile(string modulePath, string moduleName)
    {
        var moduleShortName = moduleName.Split('.').Last();
        
        var content = $@"#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Game.Core.Extensions;

namespace {moduleName}.Extensions;

/// <summary>
/// Extension methods for registering {moduleName} module services and CQS handlers.
/// </summary>
public static class {moduleShortName}ServiceCollectionExtensions
{{
    /// <summary>
    /// Registers all {moduleName} module services, systems, and CQS handlers.
    /// </summary>
    /// <param name=""services"">The service collection to configure</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection Add{moduleShortName}Module(this IServiceCollection services)
    {{
        // Register core systems
        // services.AddSingleton<I{moduleShortName}System, {moduleShortName}System>();

        // Register command handlers
        // services.AddCommandHandler<ExampleCommand, ExampleCommandHandler>();

        // Register query handlers
        // services.AddQueryHandler<ExampleQuery, ExampleResult, ExampleQueryHandler>();

        return services;
    }}
}}";

        var extensionsPath = Path.Combine(modulePath, "Extensions");
        File.WriteAllText(Path.Combine(extensionsPath, $"{moduleShortName}ServiceCollectionExtensions.cs"), content);
    }

    private void CreateExampleCQSComponents(string modulePath, string moduleName)
    {
        var moduleShortName = moduleName.Split('.').Last();
        
        // Create example command
        var exampleCommand = $@"#nullable enable

using Game.Core.CQS;

namespace {moduleName}.Commands;

/// <summary>
/// Example command for {moduleName} module.
/// </summary>
public record ExampleCommand : ICommand
{{
    public string Data {{ get; init; }} = string.Empty;
}}";

        File.WriteAllText(Path.Combine(modulePath, "Commands", "ExampleCommand.cs"), exampleCommand);

        // Create example query
        var exampleQuery = $@"#nullable enable

using Game.Core.CQS;

namespace {moduleName}.Queries;

/// <summary>
/// Example query for {moduleName} module.
/// </summary>
public record ExampleQuery : IQuery<string>
{{
    public string Filter {{ get; init; }} = string.Empty;
}}";

        File.WriteAllText(Path.Combine(modulePath, "Queries", "ExampleQuery.cs"), exampleQuery);

        // Create example handlers
        var exampleCommandHandler = $@"#nullable enable

using Game.Core.CQS;
using {moduleName}.Commands;
using Game.Core.Utils;

namespace {moduleName}.Handlers;

/// <summary>
/// Handles the ExampleCommand.
/// </summary>
public class ExampleCommandHandler : ICommandHandler<ExampleCommand>
{{
    public Task HandleAsync(ExampleCommand command, CancellationToken cancellationToken = default)
    {{
        ArgumentNullException.ThrowIfNull(command);
        
        GameLogger.Info($""[{moduleShortName}] Executing ExampleCommand with data: {{command.Data}}"");
        
        // TODO: Implement command logic
        
        return Task.CompletedTask;
    }}
}}";

        File.WriteAllText(Path.Combine(modulePath, "Handlers", "ExampleCommandHandler.cs"), exampleCommandHandler);

        var exampleQueryHandler = $@"#nullable enable

using Game.Core.CQS;
using {moduleName}.Queries;
using Game.Core.Utils;

namespace {moduleName}.Handlers;

/// <summary>
/// Handles the ExampleQuery.
/// </summary>
public class ExampleQueryHandler : IQueryHandler<ExampleQuery, string>
{{
    public Task<string> HandleAsync(ExampleQuery query, CancellationToken cancellationToken = default)
    {{
        ArgumentNullException.ThrowIfNull(query);
        
        GameLogger.Info($""[{moduleShortName}] Executing ExampleQuery with filter: {{query.Filter}}"");
        
        // TODO: Implement query logic
        return Task.FromResult($""Example result for filter: {{query.Filter}}"");
    }}
}}";

        File.WriteAllText(Path.Combine(modulePath, "Handlers", "ExampleQueryHandler.cs"), exampleQueryHandler);
    }

    private void CreateTestProject(string moduleName, ModuleCreationOptions options)
    {
        var testModuleName = $"{moduleName}.Tests";
        var testModulePath = Path.Combine(_projectRoot, testModuleName);
        
        Directory.CreateDirectory(testModulePath);
        
        // Create test directories
        var testDirectories = new[]
        {
            Path.Combine(testModulePath, "CQS"),
            Path.Combine(testModulePath, "Handlers"),
            Path.Combine(testModulePath, "Systems")
        };
        
        foreach (var dir in testDirectories)
        {
            Directory.CreateDirectory(dir);
        }
        
        // Create test project file
        var testProjectContent = $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.8.0"" />
    <PackageReference Include=""xunit"" Version=""2.6.1"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.5.3"" />
    <PackageReference Include=""FluentAssertions"" Version=""6.12.0"" />
    <PackageReference Include=""Moq"" Version=""4.20.69"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\\{moduleName}\\{moduleName}.csproj"" />
    <ProjectReference Include=""..\\Game.Core.Tests\\Game.Core.Tests.csproj"" />
  </ItemGroup>

</Project>";

        File.WriteAllText(Path.Combine(testModulePath, $"{testModuleName}.csproj"), testProjectContent);
    }
}
#endif
