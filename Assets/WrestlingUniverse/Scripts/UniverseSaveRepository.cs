using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using UnityEngine;

namespace WrestlingUniverse.Persistence
{
    /// <summary>Owns the local SQLite database used by all universe saves.</summary>
    public sealed class UniverseSaveRepository
    {
        private const int CurrentSchemaVersion = 2;
        private readonly string connectionString;

        public string DatabasePath { get; }

        public UniverseSaveRepository()
        {
            DatabasePath = Path.Combine(Application.persistentDataPath, "wrestling-universe.db");
            connectionString = "URI=file:" + DatabasePath;
        }

        public void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath));
            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS app_metadata (key TEXT PRIMARY KEY NOT NULL, value TEXT NOT NULL);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS universes (" +
                    "id TEXT PRIMARY KEY NOT NULL, owner_name TEXT NOT NULL, promotion_name TEXT NOT NULL, " +
                    "promotion_initials TEXT NOT NULL, start_date TEXT NOT NULL, owner_image_path TEXT, " +
                    "promotion_image_path TEXT, created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL);");
                Execute(connection, transaction,
                    "CREATE INDEX IF NOT EXISTS idx_universes_created_utc ON universes(created_utc);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS wrestlers (" +
                    "id TEXT PRIMARY KEY NOT NULL, universe_id TEXT NOT NULL, name TEXT NOT NULL, brand TEXT NOT NULL, " +
                    "disposition TEXT NOT NULL, gender TEXT NOT NULL, tier TEXT NOT NULL, overall INTEGER NOT NULL " +
                    "CHECK(overall BETWEEN 1 AND 100), photo_path TEXT, created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL, " +
                    "FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE);");
                Execute(connection, transaction,
                    "CREATE INDEX IF NOT EXISTS idx_wrestlers_universe ON wrestlers(universe_id, name);");

                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT OR REPLACE INTO app_metadata(key, value) VALUES('schema_version', @version);";
                    AddParameter(command, "@version", CurrentSchemaVersion.ToString());
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public List<UI.UniverseDraft> LoadAll()
        {
            var results = new List<UI.UniverseDraft>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id, owner_name, promotion_name, promotion_initials, start_date, " +
                                      "owner_image_path, promotion_image_path, created_utc " +
                                      "FROM universes ORDER BY created_utc ASC;";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new UI.UniverseDraft
                        {
                            id = reader.GetString(0),
                            ownerName = reader.GetString(1),
                            promotionName = reader.GetString(2),
                            promotionInitials = reader.GetString(3),
                            startDate = reader.GetString(4),
                            ownerImagePath = ReadNullableString(reader, 5),
                            promotionImagePath = ReadNullableString(reader, 6),
                            createdUtc = reader.GetString(7)
                        });
                    }
                }
            }
            return results;
        }

        public UI.UniverseDraft LoadById(string universeId)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id, owner_name, promotion_name, promotion_initials, start_date, " +
                                      "owner_image_path, promotion_image_path, created_utc " +
                                      "FROM universes WHERE id = @id LIMIT 1;";
                AddParameter(command, "@id", universeId);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new UI.UniverseDraft
                    {
                        id = reader.GetString(0),
                        ownerName = reader.GetString(1),
                        promotionName = reader.GetString(2),
                        promotionInitials = reader.GetString(3),
                        startDate = reader.GetString(4),
                        ownerImagePath = ReadNullableString(reader, 5),
                        promotionImagePath = ReadNullableString(reader, 6),
                        createdUtc = reader.GetString(7)
                    };
                }
            }
        }

        public void Save(UI.UniverseDraft universe)
        {
            if (string.IsNullOrEmpty(universe.id)) universe.id = Guid.NewGuid().ToString("N");
            if (string.IsNullOrEmpty(universe.createdUtc)) universe.createdUtc = DateTime.UtcNow.ToString("O");

            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText =
                    "INSERT OR REPLACE INTO universes " +
                    "(id, owner_name, promotion_name, promotion_initials, start_date, owner_image_path, " +
                    "promotion_image_path, created_utc, updated_utc) VALUES " +
                    "(@id, @owner, @promotion, @initials, @startDate, @ownerImage, @promotionImage, @created, @updated);";
                AddParameter(command, "@id", universe.id);
                AddParameter(command, "@owner", universe.ownerName);
                AddParameter(command, "@promotion", universe.promotionName);
                AddParameter(command, "@initials", universe.promotionInitials);
                AddParameter(command, "@startDate", universe.startDate);
                AddParameter(command, "@ownerImage", universe.ownerImagePath);
                AddParameter(command, "@promotionImage", universe.promotionImagePath);
                AddParameter(command, "@created", universe.createdUtc);
                AddParameter(command, "@updated", DateTime.UtcNow.ToString("O"));
                command.ExecuteNonQuery();
            }
        }

        public void Delete(string universeId)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "DELETE FROM universes WHERE id = @id;";
                AddParameter(command, "@id", universeId);
                command.ExecuteNonQuery();
            }
        }

        public List<UI.WrestlerRecord> LoadWrestlers(string universeId)
        {
            var results = new List<UI.WrestlerRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id, universe_id, name, brand, disposition, gender, tier, overall, photo_path, created_utc " +
                                      "FROM wrestlers WHERE universe_id = @universeId ORDER BY name COLLATE NOCASE;";
                AddParameter(command, "@universeId", universeId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new UI.WrestlerRecord
                        {
                            id = reader.GetString(0), universeId = reader.GetString(1), name = reader.GetString(2),
                            brand = reader.GetString(3), disposition = reader.GetString(4), gender = reader.GetString(5),
                            tier = reader.GetString(6), overall = reader.GetInt32(7), photoPath = ReadNullableString(reader, 8),
                            createdUtc = reader.GetString(9)
                        });
                    }
                }
            }
            return results;
        }

        public void SaveWrestler(UI.WrestlerRecord wrestler)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "INSERT OR REPLACE INTO wrestlers " +
                    "(id, universe_id, name, brand, disposition, gender, tier, overall, photo_path, created_utc, updated_utc) " +
                    "VALUES (@id, @universeId, @name, @brand, @disposition, @gender, @tier, @overall, @photo, @created, @updated);";
                AddParameter(command, "@id", wrestler.id); AddParameter(command, "@universeId", wrestler.universeId);
                AddParameter(command, "@name", wrestler.name); AddParameter(command, "@brand", wrestler.brand);
                AddParameter(command, "@disposition", wrestler.disposition); AddParameter(command, "@gender", wrestler.gender);
                AddParameter(command, "@tier", wrestler.tier); AddParameter(command, "@overall", wrestler.overall);
                AddParameter(command, "@photo", wrestler.photoPath); AddParameter(command, "@created", wrestler.createdUtc);
                AddParameter(command, "@updated", DateTime.UtcNow.ToString("O"));
                command.ExecuteNonQuery();
            }
        }

        private SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "PRAGMA foreign_keys = ON; PRAGMA busy_timeout = 5000;";
                command.ExecuteNonQuery();
            }
            return connection;
        }

        private static void Execute(IDbConnection connection, IDbTransaction transaction, string sql)
        {
            using (var command = CreateCommand(connection))
            {
                command.Transaction = transaction;
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        private static IDbCommand CreateCommand(IDbConnection connection)
        {
            // Unity's Mono profile contains a broken compatibility implementation
            // of SqliteConnection.CreateCommand(). Constructing the command avoids it.
            var command = new SqliteCommand();
            command.Connection = (SqliteConnection)connection;
            return command;
        }

        private static void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private static string ReadNullableString(IDataRecord reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }
    }
}
