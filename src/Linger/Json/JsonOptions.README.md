# JSON Serialization Configuration

JsonOptions provides unified JSON serialization configuration for HTTP clients and ASP.NET Core WebAPI applications.

## Overview

JsonOptions provides centralized JSON serialization options that follow the "Be conservative in what you send, be liberal in what you accept" principle:

- Response/Input Options (CreateResponseOptions): Liberal configuration for deserializing API responses
- Request/Output Options (CreateRequestOptions): Conservative configuration for serializing API requests
- WebAPI Configuration (ApplyDefaultConfiguration): Applies optimized configuration to existing options, balancing input tolerance and output optimization

## Usage in ASP.NET Core WebAPI

### For Controllers (MVC/WebAPI)

Add this to your Program.cs or Startup.cs:

```
using Linger.JsonConverter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => 
        JsonOptions.ApplyDefaultConfiguration(options.JsonSerializerOptions));

var app = builder.Build();
```

### For Minimal API

```
using Linger.JsonConverter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    JsonOptions.ApplyDefaultConfiguration(options.SerializerOptions));

var app = builder.Build();
```

### Manual Configuration

If you need to customize further:

```
using Linger.JsonConverter;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        JsonOptions.ApplyDefaultConfiguration(options.JsonSerializerOptions);
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });
```

### Configure JsonSerializerOptions Directly

```
using Linger.JsonConverter;

var jsonOptions = new JsonSerializerOptions();
JsonOptions.ApplyDefaultConfiguration(jsonOptions);

var responseOptions = JsonOptions.CreateResponseOptions();
var requestOptions = JsonOptions.CreateRequestOptions();
```

## Features

### Response Options - CreateResponseOptions() (Liberal, for receiving)
- Accepts number strings ("123" as 123)
- Handles reference cycles
- Preserves null values
- Custom date/time converters
- DataTable support
- Camel case properties (Web defaults)

### Request Options - CreateRequestOptions() (Conservative, for sending)
- Standard JSON format (numbers without quotes)
- Omits null values (reduces payload size)
- Custom date/time converters
- Camel case properties (Web defaults)
- Only includes necessary converters (more lightweight)

### WebAPI Configuration - ApplyDefaultConfiguration() (Balanced, for WebAPI)
- Accepts number strings (input tolerance)
- Handles reference cycles
- Omits null values (output optimization)
- Custom date/time converters
- DataTable support
- Camel case properties (Web defaults)

## Benefits

1. Consistency: Same JSON behavior across HTTP clients and WebAPI
2. Maintainability: Single source of truth for JSON configuration
3. Best Practices: Follows "strict output, lenient input" principle
4. Interoperability: Handles common API variations (number strings, etc.)

## Built-in Converters

The following custom converters are included in the configuration:

- DateTimeConverter: Handles DateTime serialization/deserialization
- DateTimeNullConverter: Handles nullable DateTime
- DataTableJsonConverter: Converts between DataTable and JSON
- JsonObjectConverter: Handles dynamic JSON objects

## Related Types

- JsonOptions - JSON options factory class
- DateTimeConverter - DateTime serialization converter
- DateTimeNullConverter - Nullable DateTime converter
- DataTableJsonConverter - DataTable serialization converter
- JsonObjectConverter - Dynamic JSON object converter
- HttpClientBase - HTTP client base class that uses these options