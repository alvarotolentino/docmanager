using System;
using System.Collections;
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
    public class DbManager : IDisposable
    {
        private NpgsqlConnection connection;
        public DbManager(NpgsqlConnection connection)
        {
            this.connection = connection;
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

                if (connection?.State != ConnectionState.Open) await connection.OpenAsync();
                await cmd.PrepareAsync();
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                dynamic dyn = null;
                if (outpuParam != null)
                {
                    dyn = new ExpandoObject();
                    var o = (object)outpuParam;
                    var properties = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var property in properties)
                    {
                        string sType = property.PropertyType.FullName;
                        var paramValue = cmd.Parameters[ToSnakeCase(property.Name, paramPrefix)].Value;
                        var value = Convert.ChangeType(paramValue, Type.GetType(sType));
                        AddProperty(dyn, property.Name, value);
                    }
                }
                await connection.CloseAsync();
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

                if (connection?.State != ConnectionState.Open) await connection.OpenAsync();
                await cmd.PrepareAsync();
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
                            var value = reader.GetValue(i);
                            var property = obj.GetType().GetProperty(propertyName);
                            string sType = property.PropertyType.FullName;
                            var castedValue = Convert.ChangeType(value, Type.GetType(sType));
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

                if (connection?.State != ConnectionState.Open) await connection.OpenAsync();
                await cmd.PrepareAsync();
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (!reader.HasRows || !await reader.ReadAsync()) return await Task.FromResult<T>(null);
                    var length = reader.FieldCount;
                    for (int i = 0; i < length; i++)
                    {
                        var propertyName = FromSnakeCase(reader.GetName(i));
                        var value = reader.GetValue(i);
                        AddProperty(dyn, propertyName, value);
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
                string sType = property.PropertyType.FullName;
                var castedValue = Convert.ChangeType(keyValue[key], Type.GetType(sType));
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
            return sb.ToString().ToLower();
        }

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}