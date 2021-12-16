using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Infrastructure.Persistence.Database
{
    static class SnakeMapping
    {
        public static ConcurrentDictionary<string, string> Mapping { get; set; }
    }
    public class DbManager : IDisposable
    {
        private NpgsqlConnection connection;
        public DbManager(NpgsqlConnection connection)
        {
            this.connection = connection;
            SnakeMapping.Mapping = new ConcurrentDictionary<string, string>();
        }

        public async Task<T> ExecuteNonQueryAsync<T>(string command,
        CancellationToken cancellationToken,
        dynamic inputParam = null,
        dynamic outpuParam = null,
        ParameterDirection outputDirection = ParameterDirection.InputOutput,
        CommandType? commandType = null,
        string paramPrefix = "@p_") where T : class, new()
        {
            using (var cmd = new NpgsqlCommand(command, this.connection))
            {
                if (commandType.HasValue) cmd.CommandType = commandType.Value;

                AddInputParameters(inputParam, paramPrefix, cmd);

                AddOutputParameters(outpuParam, outputDirection, paramPrefix, cmd);

                if (connection?.State == ConnectionState.Closed) await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                await connection.CloseAsync();

                dynamic dyn = null;
                if (outpuParam != null)
                {
                    dyn = new ExpandoObject();
                    var o = (object)outpuParam;
                    var properties = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var expandoDict = dyn as IDictionary<string, object>;

                    foreach (var property in properties)
                    {
                        var paramValue = cmd.Parameters[ToSnakeCase(property.Name, paramPrefix)].Value;
                        var castedValue = Convert.ChangeType(paramValue, Type.GetType(property.PropertyType.FullName));
                        AddProperty(expandoDict, property.Name, castedValue);
                    }
                }
                return await Task.FromResult(dyn);
            }
        }

        public async Task<IEnumerable<T>> ExecuteReaderAsListAsync<T>(string command,
        CancellationToken cancellationToken,
        dynamic inputParam = null,
        CommandType? commandType = null,
        string paramPrefix = "p_") where T : class, new()
        {
            IList<T> list = null;

            using (var cmd = new NpgsqlCommand(command, this.connection))
            {
                if (commandType.HasValue) cmd.CommandType = commandType.Value;
                AddInputParameters(inputParam, paramPrefix, cmd);

                if (connection?.State == ConnectionState.Closed) await connection.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (!reader.HasRows) return await Task.FromResult<IEnumerable<T>>(Enumerable.Empty<T>());
                    list = new List<T>();
                    while (reader.Read())
                    {
                        var obj = new T();
                        var length = reader.FieldCount;
                        for (int i = 0; i < length; i++)
                        {
                            var propertyName = FromSnakeCase(reader.GetName(i));
                            var property = obj.GetType().GetProperty(propertyName);
                            var castedValue = Convert.ChangeType(reader.GetValue(i), Type.GetType(property.PropertyType.FullName));
                            property.SetValue(obj, castedValue);
                        }
                        list.Add(obj);
                    }
                }
                await connection.CloseAsync();
            }

            return await Task.FromResult<IEnumerable<T>>(list);
        }

        public async Task<T> ExecuteReaderAsync<T>(string command,
        CancellationToken cancellationToken,
        dynamic inputParam = null,
        CommandType? commandType = null,
        string paramPrefix = "p_") where T : class, new()
        {
            dynamic dyn = new ExpandoObject();

            using (var cmd = new NpgsqlCommand(command, this.connection))
            {
                if (commandType.HasValue) cmd.CommandType = commandType.Value;
                AddInputParameters(inputParam, paramPrefix, cmd);

                if (connection?.State == ConnectionState.Closed) await connection.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (!reader.HasRows || !await reader.ReadAsync()) return await Task.FromResult<T>(null);
                    var length = reader.FieldCount;

                    var expandoDict = dyn as IDictionary<string, object>;
                    for (int i = 0; i < length; i++)
                    {
                        var propertyName = FromSnakeCase(reader.GetName(i));
                        AddProperty(expandoDict, propertyName, reader.GetValue(i));
                    }
                }
                await connection.CloseAsync();
            }

            var obj = new T();
            if (obj.GetType() == typeof(System.Object))
            {
                return await Task.FromResult(dyn);
            }

            var keyValue = dyn as IDictionary<string, object>;
            foreach (var key in keyValue.Keys)
            {
                var property = obj.GetType().GetProperty(key);
                var castedValue = Convert.ChangeType(keyValue[key], Type.GetType(property.PropertyType.FullName));
                property.SetValue(obj, castedValue);
            }
            return await Task.FromResult(obj);
        }

        private void AddInputParameters(dynamic inputParam, string paramPrefix, NpgsqlCommand cmd)
        {
            if (inputParam != null)
            {
                var o = (object)inputParam;
                var properties = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    var value = property.GetValue(o);
                    if (value != null)
                    {
                        if (property.PropertyType == typeof(int))
                        {
                            if ((int)value < 0 || (int)value > 0)
                                cmd.Parameters.AddWithValue(ToSnakeCase(property.Name, paramPrefix), value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue(ToSnakeCase(property.Name, paramPrefix), value);
                        }
                    }
                }
            }
        }

        private void AddOutputParameters(dynamic outpuParam, ParameterDirection outputDirection, string paramPrefix, NpgsqlCommand cmd)
        {
            if (outpuParam != null)
            {
                var o = (object)outpuParam;
                var properties = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    var value = property.GetValue(o);
                    var param = new NpgsqlParameter(ToSnakeCase(property.Name, paramPrefix), value);
                    param.Direction = outputDirection;
                    cmd.Parameters.Add(param);
                }
            }
        }


        public void Dispose()
        {
            if (this.connection != null && this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }
        }

        private string FromSnakeCase(string value, string prefix = null)
        {
            var name = SnakeMapping.Mapping.Values.FirstOrDefault(v => v == value);
            if (!string.IsNullOrWhiteSpace(name)) return name;

            if (prefix != null)
                value = value.Remove(prefix.Length);

            var sb = new StringBuilder();
            for (var i = 0; i < value.Length; i++)
            {
                if (i == 0 && char.IsLower(value[i]))
                {
                    sb.Append(char.ToUpper(value[i]));
                }
                else if (i > 0 && value[i] == '_' && i + 1 <= value.Length)
                {
                    sb.Append(char.ToUpper(value[i + 1]));
                    i++;
                }
                else if (i > 0 && value[i] != '_')
                {
                    sb.Append(value[i]);
                }

            }
            return sb.ToString();
        }
        private string ToSnakeCase(string value, string prefix)
        {
            SnakeMapping.Mapping.TryGetValue(value, out var snakeCaseValue);
            if (!string.IsNullOrWhiteSpace(snakeCaseValue)) return snakeCaseValue;

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(prefix)) sb.Append(prefix);
            for (var i = 0; i < value.Length; i++)
            {
                if (i > 0 && char.IsUpper(value[i]))
                {
                    sb.Append("_");
                    sb.Append(char.ToLower(value[i]));
                }
                else
                {
                    sb.Append(value[i]);
                }
            }

            var snakeValue = sb.ToString().ToLower();
            SnakeMapping.Mapping.TryAdd(value, snakeValue);
            return snakeValue;
        }

        private static void AddProperty(IDictionary<string, object> expandoDict, string propertyName, object propertyValue)
        {
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}