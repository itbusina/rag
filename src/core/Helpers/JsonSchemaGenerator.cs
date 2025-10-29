using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace core.Helpers
{
    public static class JsonSchemaGenerator
    {
        public static object GenerateSchema(Type type)
        {
            if (type == typeof(string))
                return new { type = "string" };

            if (type == typeof(int) || type == typeof(long) ||
                type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return new { type = "number" };

            if (type == typeof(bool))
                return new { type = "boolean" };

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var innerType = type.GetGenericArguments()[0];
                var innerSchema = GenerateSchema(innerType);
                return new { type = new[] { ((JsonElement)JsonSerializer.SerializeToElement(innerSchema)).GetProperty("type").GetString(), "null" } };
            }

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                var elementType = type.IsArray
                    ? type.GetElementType()!
                    : type.GetGenericArguments().FirstOrDefault() ?? typeof(object);

                return new
                {
                    type = "array",
                    items = GenerateSchema(elementType)
                };
            }

            // Object type
            var properties = new Dictionary<string, object>();
            var required = new List<string>();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                properties[prop.Name[..1].ToLower() + prop.Name[1..]] = GenerateSchema(prop.PropertyType);
                if (!IsNullableProperty(prop))
                    required.Add(prop.Name[..1].ToLower() + prop.Name[1..]);
            }

            var schema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["additionalProperties"] = false
            };

            if (required.Count > 0)
                schema["required"] = required;

            return schema;
        }

        private static bool IsNullableProperty(PropertyInfo prop)
        {
            if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
                return true;

            // Optional: detect C# 8 nullable reference types
            var nullableAttr = prop.CustomAttributes
                .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            return nullableAttr != null;
        }
    }
}