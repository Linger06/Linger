namespace Linger.UnitTests.JsonConverter;

public class JsonConverterTest
{
#if NET7_0_OR_GREATER

    [Fact]
    public static void SystemObjectNewtonsoftCompatibleConverterDeserialize()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonObjectConverter());

        {
            const string Value = "null";

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            Assert.Null(obj);

            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            Assert.Null(newtonsoftObj);
        }

        {
            const string Value = """
                                 "mystring"
                                 """;

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            _ = Assert.IsType<string>(obj);
            Assert.Equal("mystring", obj);

            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            _ = Assert.IsType<string>(newtonsoftObj);
            Assert.Equal(newtonsoftObj, obj);
        }

        {
            const string Value = "true";

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            _ = Assert.IsType<bool>(obj);
            Assert.True((bool)obj);

            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            _ = Assert.IsType<bool>(newtonsoftObj);
            Assert.Equal(newtonsoftObj, obj);
        }

        {
            const string Value = "false";

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            _ = Assert.IsType<bool>(obj);
            Assert.False((bool)obj);

            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            _ = Assert.IsType<bool>(newtonsoftObj);
            Assert.Equal(newtonsoftObj, obj);
        }

        {
            const string Value = "123";

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            _ = Assert.IsType<long>(obj);
            Assert.Equal((long)123, obj);

            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            _ = Assert.IsType<long>(newtonsoftObj);
            Assert.Equal(newtonsoftObj, obj);
        }

        {
            const string Value = "123.45";

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            _ = Assert.IsType<double>(obj);
            Assert.Equal(123.45d, obj);

            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            _ = Assert.IsType<double>(newtonsoftObj);
            Assert.Equal(newtonsoftObj, obj);
        }

        {
            const string Value = """
                                 "2019-01-30T12:01:02Z"
                                 """;

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            _ = Assert.IsType<DateTime>(obj);
            Assert.Equal(new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc), obj);

            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            _ = Assert.IsType<DateTime>(newtonsoftObj);
            Assert.Equal(newtonsoftObj, obj);
        }

        {
            const string Value = """
                                 "2019-01-30T12:01:02+01:00"
                                 """;

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            _ = Assert.IsType<DateTime>(obj);

            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            _ = Assert.IsType<DateTime>(newtonsoftObj);
            Assert.Equal(newtonsoftObj, obj);
        }

        {
            const string Value = "{}";

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            _ = Assert.IsType<JsonElement>(obj);

            // Types are different.
            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            _ = Assert.IsType<Newtonsoft.Json.Linq.JObject>(newtonsoftObj);
        }

        {
            const string Value = "[]";

            var obj = JsonSerializer.Deserialize<object>(Value, options);
            _ = Assert.IsType<JsonElement>(obj);

            // Types are different.
            var newtonsoftObj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(Value);
            _ = Assert.IsType<Newtonsoft.Json.Linq.JArray>(newtonsoftObj);
        }
    }

    [Fact]
    public static void SystemObjectNewtonsoftCompatibleConverterSerialize()
    {
        static void Verify(JsonSerializerOptions options)
        {
            {
                var json = JsonSerializer.Serialize<object>(null!, options);
                Assert.Equal("null", json);

                var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(null);
                Assert.Equal(newtonsoftJson, json);
            }

            {
                const string Value = "mystring";

                var json = JsonSerializer.Serialize<object>(Value, options);
                Assert.Equal("""
                             "mystring"
                             """, json);

                var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(Value);
                Assert.Equal(newtonsoftJson, json);
            }

            {
                var json = JsonSerializer.Serialize<object>(true, options);
                Assert.Equal("true", json);

                var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(true);
                Assert.Equal(newtonsoftJson, json);
            }

            {
                var json = JsonSerializer.Serialize<object>(false, options);
                Assert.Equal("false", json);

                var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(false);
                Assert.Equal(newtonsoftJson, json);
            }

            {
                const long Value = 123;

                object json = JsonSerializer.Serialize<object>(123, options);
                Assert.Equal("123", json);

                object newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(Value);
                Assert.Equal(newtonsoftJson, json);
            }

            {
                const double Value = 123.45;

                object json = JsonSerializer.Serialize<object>(Value, options);
                Assert.Equal("123.45", json);

                object newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(Value);
                Assert.Equal(newtonsoftJson, json);
            }

            {
                var value = new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc);

                var json = JsonSerializer.Serialize<object>(value, options);
                Assert.Equal("""
                             "2019-01-30T12:01:02Z"
                             """, json);

                var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                Assert.Equal(newtonsoftJson, json);
            }

            {
                var value = new DateTimeOffset(2019, 1, 30, 12, 1, 2, new TimeSpan(1, 0, 0));

                var json = JsonSerializer.Serialize<object>(value, options);
                Assert.Equal("""
                             "2019-01-30T12:01:02+01:00"
                             """, json);

                var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                Assert.Equal(newtonsoftJson, json);
            }

            {
                var value = new object();

                var json = JsonSerializer.Serialize(new object(), options);
                Assert.Equal("{}", json);

                var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                Assert.Equal(newtonsoftJson, json);
            }

            {
                var value = new int[] { };

                var json = JsonSerializer.Serialize<object>(value, options);
                Assert.Equal("[]", json);

                var newtonsoftJson = Newtonsoft.Json.JsonConvert.SerializeObject(value);
                Assert.Equal(newtonsoftJson, json);
            }
        }

        // Results should be the same with or without the custom converter since the serializer
        // calls value.GetType() for every property value declared as System.Object.
        Verify(new JsonSerializerOptions());

        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonObjectConverter());
        Verify(options);
    }

    [Fact]
    public void JsonConvertorTest2()
    {
        var jsonObject = new JsonClass
        {
            Int = 1,
            NullableInt = null,
            String = "123",
            NullableGuid = null,
            DateTime = new DateTime(2020, 1, 1),
            NullableDateTime = null,
            Boolean = true,
            Int16 = 2,
            Int64 = 3,
            Decimal = new decimal(1.1),
            Single = 4,
            Double = 5
        };

        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonObjectConverter());

        var json = JsonSerializer.Serialize(jsonObject, options);

        var jsonString = """{"Int":1,"NullableInt":null,"String":"123","Guid":"00000000-0000-0000-0000-000000000000","NullableGuid":null,"DateTime":"2020-01-01T00:00:00","NullableDateTime":null,"Binary":null,"Boolean":true,"Int16":2,"Int64":3,"Decimal":1.1,"Single":4,"Double":5}""";

        Assert.Equal(json, jsonString);

        JsonClass? obj = JsonSerializer.Deserialize<JsonClass>(json, options);
        Assert.NotNull(obj);

        Assert.Equal(jsonObject.Int, obj.Int);
        Assert.Equal(jsonObject.NullableInt, obj.NullableInt);
        Assert.Equal(jsonObject.String, obj.String);
        Assert.Equal(jsonObject.NullableGuid, obj.NullableGuid);
        Assert.Equal(jsonObject.DateTime, obj.DateTime);
        Assert.Equal(jsonObject.NullableDateTime, obj.NullableDateTime);
        Assert.Equal(jsonObject.Boolean, obj.Boolean);
        Assert.Equal(jsonObject.Int16, obj.Int16);
        Assert.Equal(jsonObject.Int64, obj.Int64);
        Assert.Equal(jsonObject.Decimal, obj.Decimal);
        Assert.Equal(jsonObject.Single, obj.Single);
        Assert.Equal(jsonObject.Double, obj.Double);

        {
            options = new JsonSerializerOptions();
            options.Converters.Add(new JsonObjectConverter());
            options.Converters.Add(new DateTimeConverter());
            options.Converters.Add(new DateTimeNullConverter());

            var json2 = JsonSerializer.Serialize(jsonObject, options);

            var jsonString2 = """{"Int":1,"NullableInt":null,"String":"123","Guid":"00000000-0000-0000-0000-000000000000","NullableGuid":null,"DateTime":"2020-01-01","NullableDateTime":null,"Binary":null,"Boolean":true,"Int16":2,"Int64":3,"Decimal":1.1,"Single":4,"Double":5}""";
            Assert.Equal(json2, jsonString2);

            obj = JsonSerializer.Deserialize<JsonClass>(json2, options);
            Assert.NotNull(obj);
            Assert.Equal(jsonObject.Int, obj.Int);
            Assert.Equal(jsonObject.NullableInt, obj.NullableInt);
            Assert.Equal(jsonObject.String, obj.String);
            Assert.Equal(jsonObject.NullableGuid, obj.NullableGuid);
            Assert.Equal(jsonObject.DateTime, obj.DateTime);
            Assert.Equal(jsonObject.NullableDateTime, obj.NullableDateTime);
            Assert.Equal(jsonObject.Boolean, obj.Boolean);
            Assert.Equal(jsonObject.Int16, obj.Int16);
            Assert.Equal(jsonObject.Int64, obj.Int64);
            Assert.Equal(jsonObject.Decimal, obj.Decimal);
            Assert.Equal(jsonObject.Single, obj.Single);
            Assert.Equal(jsonObject.Double, obj.Double);

            jsonObject.DateTime = new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc);

            var json3 = JsonSerializer.Serialize(jsonObject, options);

            var jsonString3 = """{"Int":1,"NullableInt":null,"String":"123","Guid":"00000000-0000-0000-0000-000000000000","NullableGuid":null,"DateTime":"2019-01-30 12:01:02","NullableDateTime":null,"Binary":null,"Boolean":true,"Int16":2,"Int64":3,"Decimal":1.1,"Single":4,"Double":5}""";
            Assert.Equal(json3, jsonString3);

            obj = JsonSerializer.Deserialize<JsonClass>(json3, options);
            Assert.NotNull(obj);
            Assert.Equal(jsonObject.Int, obj.Int);
            Assert.Equal(jsonObject.NullableInt, obj.NullableInt);
            Assert.Equal(jsonObject.String, obj.String);
            Assert.Equal(jsonObject.NullableGuid, obj.NullableGuid);
            Assert.Equal(jsonObject.DateTime, obj.DateTime);
            Assert.Equal(jsonObject.NullableDateTime, obj.NullableDateTime);
            Assert.Equal(jsonObject.Boolean, obj.Boolean);
            Assert.Equal(jsonObject.Int16, obj.Int16);
            Assert.Equal(jsonObject.Int64, obj.Int64);
            Assert.Equal(jsonObject.Decimal, obj.Decimal);
            Assert.Equal(jsonObject.Single, obj.Single);
            Assert.Equal(jsonObject.Double, obj.Double);
        }

        {
            options = ExtensionMethodSetting.DefaultJsonSerializerOptions;

            var json2 = JsonSerializer.Serialize(jsonObject, options);

            //var jsonString2 = """{"Int":1,"NullableInt":null,"String":"123","Guid":"00000000-0000-0000-0000-000000000000","NullableGuid":null,"DateTime":"2020-01-01","NullableDateTime":null,"Binary":null,"Boolean":true,"Int16":2,"Int64":3,"Decimal":1.1,"Single":4,"Double":5}""";
            //Assert.Equal(jsonString2,json2);

            obj = JsonSerializer.Deserialize<JsonClass>(json2, options);
            Assert.NotNull(obj);
            Assert.Equal(jsonObject.Int, obj.Int);
            Assert.Equal(jsonObject.NullableInt, obj.NullableInt);
            Assert.Equal(jsonObject.String, obj.String);
            Assert.Equal(jsonObject.NullableGuid, obj.NullableGuid);
            Assert.Equal(jsonObject.DateTime, obj.DateTime);
            Assert.Equal(jsonObject.NullableDateTime, obj.NullableDateTime);
            Assert.Equal(jsonObject.Boolean, obj.Boolean);
            Assert.Equal(jsonObject.Int16, obj.Int16);
            Assert.Equal(jsonObject.Int64, obj.Int64);
            Assert.Equal(jsonObject.Decimal, obj.Decimal);
            Assert.Equal(jsonObject.Single, obj.Single);
            Assert.Equal(jsonObject.Double, obj.Double);

            jsonObject.DateTime = new DateTime(2019, 1, 30, 12, 1, 2, DateTimeKind.Utc);

            var json3 = JsonSerializer.Serialize(jsonObject, options);

            //var jsonString3 = """{"Int":1,"NullableInt":null,"String":"123","Guid":"00000000-0000-0000-0000-000000000000","NullableGuid":null,"DateTime":"2019-01-30 12:01:02","NullableDateTime":null,"Binary":null,"Boolean":true,"Int16":2,"Int64":3,"Decimal":1.1,"Single":4,"Double":5}""";
            //Assert.Equal(json3, jsonString3);

            obj = JsonSerializer.Deserialize<JsonClass>(json3, options);
            Assert.NotNull(obj);
            Assert.Equal(jsonObject.Int, obj.Int);
            Assert.Equal(jsonObject.NullableInt, obj.NullableInt);
            Assert.Equal(jsonObject.String, obj.String);
            Assert.Equal(jsonObject.NullableGuid, obj.NullableGuid);
            Assert.Equal(jsonObject.DateTime, obj.DateTime);
            Assert.Equal(jsonObject.NullableDateTime, obj.NullableDateTime);
            Assert.Equal(jsonObject.Boolean, obj.Boolean);
            Assert.Equal(jsonObject.Int16, obj.Int16);
            Assert.Equal(jsonObject.Int64, obj.Int64);
            Assert.Equal(jsonObject.Decimal, obj.Decimal);
            Assert.Equal(jsonObject.Single, obj.Single);
            Assert.Equal(jsonObject.Double, obj.Double);
        }
    }

#endif
}

public class JsonClass
{
    public int Int { get; set; }
    public int? NullableInt { get; set; }
    public string? String { get; set; }
    public Guid Guid { get; set; }
    public Guid? NullableGuid { get; set; }
    public DateTime DateTime { get; set; }
    public DateTime? NullableDateTime { get; set; }
    public byte[]? Binary { get; set; }
    public bool Boolean { get; set; }
    public short Int16 { get; set; }
    public long Int64 { get; set; }
    public decimal Decimal { get; set; }
    public float Single { get; set; }
    public double Double { get; set; }
}