using System.Data;
using Microsoft.Data.SqlClient;
using FlotaLuchitoWeb.Clases;

namespace FlotaLuchitoWeb.Data
{
    // Esta wea de clase hace el nexo de pana entre las vistas y la BD local.
    // Pa no andar escribiendo SQL directo en los controladores, terrible perkin esa wea.
    public class DataBaseHelper
    {
        private readonly string _connectionString;

        public DataBaseHelper(IConfiguration configuration, IWebHostEnvironment environment)
        {
            // Creamos la carpeta App_Data pa tirar el archivo de la BD de una, no hay otra.
            // Con esto el comodin |DataDirectory| de appsettings.json lee el path al toque sin dramas.
            var appDataPath = Path.Combine(environment.ContentRootPath, "App_Data");
            Directory.CreateDirectory(appDataPath);
            AppDomain.CurrentDomain.SetData("DataDirectory", appDataPath);

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Falta la conexión DefaultConnection en appsettings.json");

            InicializarBaseDeDatos();
        }

        private void InicializarBaseDeDatos()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                
                // Oe washito, akí sapeamos si las columnas de geolocalización ya están en la tabla. Si no están, se las chantamos de una pa que no guatee la app.
                const string checkSql = "SELECT COUNT(1) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Viajes') AND name = 'LatitudOrigen';";
                using var checkCmd = new SqlCommand(checkSql, connection);
                var existeColumna = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                
                if (!existeColumna)
                {
                    const string alterSql = @"
                        ALTER TABLE dbo.Viajes ADD
                        LatitudOrigen FLOAT NULL,
                        LongitudOrigen FLOAT NULL,
                        LatitudDestino FLOAT NULL,
                        LongitudDestino FLOAT NULL,
                        TiempoEstimado INT NULL;";
                    using var alterCmd = new SqlCommand(alterSql, connection);
                    alterCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al inicializar base de datos: {ex.Message}");
            }
        }

        // =========================
        // LOGIN
        // =========================
        public async Task<bool> ValidarUsuarioAsync(string usuario, string password)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Usuarios
                WHERE UsuarioLogin = @UsuarioLogin AND Password = @Password;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            // Le metemos parametros pa que ningun logi nos haga SQL injection y nos cague la base de datos
            command.Parameters.AddWithValue("@UsuarioLogin", usuario.Trim());
            command.Parameters.AddWithValue("@Password", password.Trim());

            await connection.OpenAsync();
            var cantidad = Convert.ToInt32(await command.ExecuteScalarAsync());

            return cantidad > 0;
        }

        // =========================
        // CONDUCTORES
        // =========================
        public async Task<List<Conductorcs>> ObtenerConductoresAsync()
        {
            var lista = new List<Conductorcs>();

            const string sql = @"
                SELECT Id, Nombre, Apellido, Rut, Telefono, Correo, NumeroLicencia, TipoLicencia, FechaVencimientoLicencia, Estado
                FROM dbo.Conductores
                ORDER BY Id DESC;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Conductorcs
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = GetString(reader, "Nombre"),
                    Apellido = GetString(reader, "Apellido"),
                    Rut = GetString(reader, "Rut"),
                    Telefono = GetString(reader, "Telefono"),
                    Correo = GetString(reader, "Correo"),
                    NumeroLicencia = GetString(reader, "NumeroLicencia"),
                    TipoLicencia = GetString(reader, "TipoLicencia"),
                    FechaVencimientoLicencia = GetDate(reader, "FechaVencimientoLicencia"),
                    Estado = GetString(reader, "Estado")
                });
            }

            return lista;
        }

        public async Task<Conductorcs?> ObtenerConductorPorIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, Nombre, Apellido, Rut, Telefono, Correo, NumeroLicencia, TipoLicencia, FechaVencimientoLicencia, Estado
                FROM dbo.Conductores
                WHERE Id = @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new Conductorcs
            {
                Id = Convert.ToInt32(reader["Id"]),
                Nombre = GetString(reader, "Nombre"),
                Apellido = GetString(reader, "Apellido"),
                Rut = GetString(reader, "Rut"),
                Telefono = GetString(reader, "Telefono"),
                Correo = GetString(reader, "Correo"),
                NumeroLicencia = GetString(reader, "NumeroLicencia"),
                TipoLicencia = GetString(reader, "TipoLicencia"),
                FechaVencimientoLicencia = GetDate(reader, "FechaVencimientoLicencia"),
                Estado = GetString(reader, "Estado")
            };
        }

        public async Task<List<string>> ObtenerConductoresComboAsync()
        {
            var lista = new List<string>();

            const string sql = @"
                SELECT Nombre, Apellido, Rut
                FROM dbo.Conductores
                ORDER BY Nombre, Apellido;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add($"{reader["Nombre"]} {reader["Apellido"]} ({reader["Rut"]})");
            }

            return lista;
        }

        public async Task<bool> ExisteRutAsync(string rut, int idActual)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Conductores
                WHERE Rut = @Rut AND Id <> @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Rut", rut.Trim());
            command.Parameters.AddWithValue("@Id", idActual);

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }

        public async Task InsertarConductorAsync(Conductorcs conductor)
        {
            const string sql = @"
                INSERT INTO dbo.Conductores
                (Nombre, Apellido, Rut, Telefono, Correo, NumeroLicencia, TipoLicencia, FechaVencimientoLicencia, Estado)
                VALUES
                (@Nombre, @Apellido, @Rut, @Telefono, @Correo, @NumeroLicencia, @TipoLicencia, @FechaVencimientoLicencia, @Estado);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            AddConductorParameters(command, conductor);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task ActualizarConductorAsync(Conductorcs conductor)
        {
            const string sql = @"
                UPDATE dbo.Conductores
                SET Nombre = @Nombre,
                    Apellido = @Apellido,
                    Rut = @Rut,
                    Telefono = @Telefono,
                    Correo = @Correo,
                    NumeroLicencia = @NumeroLicencia,
                    TipoLicencia = @TipoLicencia,
                    FechaVencimientoLicencia = @FechaVencimientoLicencia,
                    Estado = @Estado
                WHERE Id = @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            AddConductorParameters(command, conductor);
            command.Parameters.AddWithValue("@Id", conductor.Id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task EliminarConductorAsync(int id)
        {
            const string sql = @"
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;
        DECLARE @NombreConductor NVARCHAR(160);
        SELECT @NombreConductor =
            CONCAT(Nombre, ' ', Apellido, ' (', Rut, ')')
        FROM dbo.Conductores
        WHERE Id = @Id;
        IF @NombreConductor IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END;
        DELETE FROM dbo.Viajes
        WHERE Conductor = @NombreConductor
           OR Vehiculo IN
           (
               SELECT CONCAT(Patente, ' - ', Marca, ' ', Modelo)
               FROM dbo.Vehiculos
               WHERE Conductor = @NombreConductor
           );
        DELETE FROM dbo.Vehiculos
        WHERE Conductor = @NombreConductor;
        DELETE FROM dbo.Conductores
        WHERE Id = @Id;
        COMMIT TRANSACTION;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private static void AddConductorParameters(SqlCommand command, Conductorcs conductor)
        {
            command.Parameters.AddWithValue("@Nombre", conductor.Nombre.Trim());
            command.Parameters.AddWithValue("@Apellido", conductor.Apellido.Trim());
            command.Parameters.AddWithValue("@Rut", conductor.Rut.Trim());
            command.Parameters.AddWithValue("@Telefono", conductor.Telefono.Trim());
            command.Parameters.AddWithValue("@Correo", conductor.Correo.Trim());
            command.Parameters.AddWithValue("@NumeroLicencia", conductor.NumeroLicencia.Trim());
            command.Parameters.AddWithValue("@TipoLicencia", conductor.TipoLicencia.Trim());
            command.Parameters.Add("@FechaVencimientoLicencia", SqlDbType.Date).Value = conductor.FechaVencimientoLicencia!.Value.Date;
            command.Parameters.AddWithValue("@Estado", conductor.Estado.Trim());
        }

        // =========================
        // VEHÍCULOS
        // =========================
        public async Task<List<Vehiculo>> ObtenerVehiculosAsync()
        {
            var lista = new List<Vehiculo>();

            const string sql = @"
                SELECT Id, TipoVehiculo, Conductor, Patente, Marca, Modelo, Anio, Color, Combustible, Kilometraje, Estado, TieneMultas, VaAMantencion
                FROM dbo.Vehiculos
                ORDER BY Id DESC;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(ReadVehiculo(reader));
            }

            return lista;
        }

        public async Task<Vehiculo?> ObtenerVehiculoPorIdAsync(int id)
        {
            const string sql = @"
                    SELECT Id, TipoVehiculo, Conductor, Patente, Marca, Modelo, Anio, Color, Combustible, Kilometraje, Estado, TieneMultas, VaAMantencion
                    FROM dbo.Vehiculos
                    WHERE Id = @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? ReadVehiculo(reader) : null;
        }

        private static Vehiculo ReadVehiculo(SqlDataReader reader)
        {
            return new Vehiculo
            {
                Id = Convert.ToInt32(reader["Id"]),
                TipoVehiculo = GetString(reader, "TipoVehiculo"),
                Conductor = GetString(reader, "Conductor"),
                Patente = GetString(reader, "Patente"),
                Marca = GetString(reader, "Marca"),
                Modelo = GetString(reader, "Modelo"),
                Anio = GetInt(reader, "Anio"),
                Color = GetString(reader, "Color"),
                Combustible = GetString(reader, "Combustible"),
                Kilometraje = GetDecimal(reader, "Kilometraje"),
                Estado = GetString(reader, "Estado"),
                TieneMultas = GetBool(reader, "TieneMultas"),
                VaAMantencion = GetBool(reader, "VaAMantencion")
            };
        }

        public async Task<List<string>> ObtenerVehiculosComboAsync()
        {
            var lista = new List<string>();

            const string sql = @"
                SELECT Patente, Marca, Modelo
                FROM dbo.Vehiculos
                ORDER BY Patente;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add($"{reader["Patente"]} - {reader["Marca"]} {reader["Modelo"]}");
            }

            return lista;
        }

        public async Task<bool> ExistePatenteAsync(string patente, int idActual)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Vehiculos
                WHERE Patente = @Patente AND Id <> @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Patente", patente.Trim().ToUpperInvariant());
            command.Parameters.AddWithValue("@Id", idActual);

            await connection.OpenAsync();
            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }

        public async Task InsertarVehiculoAsync(Vehiculo vehiculo)
        {
            const string sql = @"
                INSERT INTO dbo.Vehiculos
                (TipoVehiculo, Conductor, Patente, Marca, Modelo, Anio, Color, Combustible, Kilometraje, Estado, TieneMultas, VaAMantencion)
                VALUES
                (@TipoVehiculo, @Conductor, @Patente, @Marca, @Modelo, @Anio, @Color, @Combustible, @Kilometraje, @Estado, @TieneMultas, @VaAMantencion);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            AddVehiculoParameters(command, vehiculo);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task ActualizarVehiculoAsync(Vehiculo vehiculo)
        {
            const string sql = @"
                    UPDATE dbo.Vehiculos
                    SET TipoVehiculo = @TipoVehiculo,
                        Conductor = @Conductor,
                        Patente = @Patente,
                        Marca = @Marca,
                        Modelo = @Modelo,
                        Anio = @Anio,
                        Color = @Color,
                        Combustible = @Combustible,
                        Kilometraje = @Kilometraje,
                        Estado = @Estado,
                        TieneMultas = @TieneMultas,
                        VaAMantencion = @VaAMantencion
                    WHERE Id = @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            AddVehiculoParameters(command, vehiculo);
            command.Parameters.AddWithValue("@Id", vehiculo.Id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task EliminarVehiculoAsync(int id)
        {
            const string sql = "DELETE FROM dbo.Vehiculos WHERE Id = @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private static void AddVehiculoParameters(SqlCommand command, Vehiculo vehiculo)
        {
            command.Parameters.AddWithValue("@TipoVehiculo", vehiculo.TipoVehiculo.Trim());
            command.Parameters.AddWithValue("@Conductor", vehiculo.Conductor.Trim());
            command.Parameters.AddWithValue("@Patente", vehiculo.Patente.Trim().ToUpperInvariant());
            command.Parameters.AddWithValue("@Marca", vehiculo.Marca.Trim());
            command.Parameters.AddWithValue("@Modelo", vehiculo.Modelo.Trim());
            command.Parameters.AddWithValue("@Anio", vehiculo.Anio!.Value);
            command.Parameters.AddWithValue("@Color", vehiculo.Color.Trim());
            command.Parameters.AddWithValue("@Combustible", vehiculo.Combustible.Trim());
            command.Parameters.AddWithValue("@Kilometraje", vehiculo.EsNuevo ? 0 : (vehiculo.Kilometraje ?? 0));
            command.Parameters.AddWithValue("@Estado", vehiculo.Estado.Trim());
            command.Parameters.AddWithValue("@TieneMultas", vehiculo.TieneMultas);
            command.Parameters.AddWithValue("@VaAMantencion", vehiculo.VaAMantencion);
        }

        // =========================
        // VIAJES
        // =========================
        public async Task<List<Viajescs>> ObtenerViajesAsync()
        {
            var lista = new List<Viajescs>();

            const string sql = @"
                    SELECT Id, Vehiculo, Conductor, Origen, Destino, FechaSalida, FechaLlegada, Distancia, Estado, LatitudOrigen, LongitudOrigen, LatitudDestino, LongitudDestino, TiempoEstimado
                    FROM dbo.Viajes
                    ORDER BY Id DESC;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(ReadViaje(reader));
            }

            return lista;
        }

        public async Task<Viajescs?> ObtenerViajePorIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, Vehiculo, Conductor, Origen, Destino, FechaSalida, FechaLlegada, Distancia, Estado, LatitudOrigen, LongitudOrigen, LatitudDestino, LongitudDestino, TiempoEstimado
                FROM dbo.Viajes
                WHERE Id = @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? ReadViaje(reader) : null;
        }

        private static Viajescs ReadViaje(SqlDataReader reader)
        {
            return new Viajescs
            {
                Id = Convert.ToInt32(reader["Id"]),
                Vehiculo = GetString(reader, "Vehiculo"),
                Conductor = GetString(reader, "Conductor"),
                Origen = GetString(reader, "Origen"),
                Destino = GetString(reader, "Destino"),
                FechaSalida = GetDate(reader, "FechaSalida"),
                FechaLlegada = GetDate(reader, "FechaLlegada"),
                Distancia = GetDecimal(reader, "Distancia"),
                Estado = GetString(reader, "Estado"),
                LatitudOrigen = GetDouble(reader, "LatitudOrigen"),
                LongitudOrigen = GetDouble(reader, "LongitudOrigen"),
                LatitudDestino = GetDouble(reader, "LatitudDestino"),
                LongitudDestino = GetDouble(reader, "LongitudDestino"),
                TiempoEstimado = GetInt(reader, "TiempoEstimado")
            };
        }

        public async Task InsertarViajeAsync(Viajescs viaje)
        {
            const string sql = @"
                    INSERT INTO dbo.Viajes
                    (Vehiculo, Conductor, Origen, Destino, FechaSalida, FechaLlegada, Distancia, Estado, LatitudOrigen, LongitudOrigen, LatitudDestino, LongitudDestino, TiempoEstimado)
                    VALUES
                    (@Vehiculo, @Conductor, @Origen, @Destino, @FechaSalida, @FechaLlegada, @Distancia, @Estado, @LatitudOrigen, @LongitudOrigen, @LatitudDestino, @LongitudDestino, @TiempoEstimado);";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            AddViajeParameters(command, viaje);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task ActualizarViajeAsync(Viajescs viaje)
        {
            const string sql = @"
                UPDATE dbo.Viajes
                SET Vehiculo = @Vehiculo,
                    Conductor = @Conductor,
                    Origen = @Origen,
                    Destino = @Destino,
                    FechaSalida = @FechaSalida,
                    FechaLlegada = @FechaLlegada,
                    Distancia = @Distancia,
                    Estado = @Estado,
                    LatitudOrigen = @LatitudOrigen,
                    LongitudOrigen = @LongitudOrigen,
                    LatitudDestino = @LatitudDestino,
                    LongitudDestino = @LongitudDestino,
                    TiempoEstimado = @TiempoEstimado
                WHERE Id = @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            AddViajeParameters(command, viaje);
            command.Parameters.AddWithValue("@Id", viaje.Id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        public async Task EliminarViajeAsync(int id)
        {
            const string sql = "DELETE FROM dbo.Viajes WHERE Id = @Id;";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

        private static void AddViajeParameters(SqlCommand command, Viajescs viaje)
        {
            command.Parameters.AddWithValue("@Vehiculo", viaje.Vehiculo.Trim());
            command.Parameters.AddWithValue("@Conductor", viaje.Conductor.Trim());
            command.Parameters.AddWithValue("@Origen", viaje.Origen.Trim());
            command.Parameters.AddWithValue("@Destino", viaje.Destino.Trim());
            command.Parameters.Add("@FechaSalida", SqlDbType.DateTime2).Value = viaje.FechaSalida;
            command.Parameters.Add("@FechaLlegada", SqlDbType.DateTime2).Value = viaje.FechaLlegada;
            command.Parameters.AddWithValue("@Distancia", viaje.Distancia!.Value);
            command.Parameters.AddWithValue("@Estado", viaje.Estado.Trim());
            // Sapea acá: si la latitud, longitud o tiempo vienen vacíos, les metemos DBNull.Value pa que SQL no ande dando la hora.
            command.Parameters.Add("@LatitudOrigen", SqlDbType.Float).Value = (object?)viaje.LatitudOrigen ?? DBNull.Value;
            command.Parameters.Add("@LongitudOrigen", SqlDbType.Float).Value = (object?)viaje.LongitudOrigen ?? DBNull.Value;
            command.Parameters.Add("@LatitudDestino", SqlDbType.Float).Value = (object?)viaje.LatitudDestino ?? DBNull.Value;
            command.Parameters.Add("@LongitudDestino", SqlDbType.Float).Value = (object?)viaje.LongitudDestino ?? DBNull.Value;
            command.Parameters.Add("@TiempoEstimado", SqlDbType.Int).Value = (object?)viaje.TiempoEstimado ?? DBNull.Value;
        }

        // Helpers de pana pa no escribir el mismo código terrible pajero pa leer SQL.
        private static string GetString(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? string.Empty : reader[columnName].ToString() ?? string.Empty;
        }
        private static DateTime? GetDate(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? null : Convert.ToDateTime(reader[columnName]);
        }
        private static int? GetInt(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? null : Convert.ToInt32(reader[columnName]);
        }
        private static decimal? GetDecimal(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? null : Convert.ToDecimal(reader[columnName]);
        }
        private static bool GetBool(SqlDataReader reader, string columnName)
        {
            return reader[columnName] != DBNull.Value && Convert.ToBoolean(reader[columnName]);
        }
        private static double? GetDouble(SqlDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? null : Convert.ToDouble(reader[columnName]);
        }
    }
}
