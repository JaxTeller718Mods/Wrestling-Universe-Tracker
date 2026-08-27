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
        private const int CurrentSchemaVersion = 8;
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
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS teams (id TEXT PRIMARY KEY NOT NULL, universe_id TEXT NOT NULL, " +
                    "name TEXT NOT NULL, brand TEXT NOT NULL, disposition TEXT NOT NULL, created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL, " +
                    "FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE);");
                EnsureColumn(connection, transaction, "teams", "photo_path", "TEXT");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS team_members (team_id TEXT NOT NULL, wrestler_id TEXT NOT NULL, position INTEGER NOT NULL, " +
                    "PRIMARY KEY(team_id, wrestler_id), FOREIGN KEY(team_id) REFERENCES teams(id) ON DELETE CASCADE, " +
                    "FOREIGN KEY(wrestler_id) REFERENCES wrestlers(id) ON DELETE CASCADE);");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_teams_universe ON teams(universe_id, name);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS titles (id TEXT PRIMARY KEY NOT NULL, universe_id TEXT NOT NULL, name TEXT NOT NULL, " +
                    "brand TEXT NOT NULL, holder_wrestler_id TEXT, image_path TEXT, created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL, " +
                    "FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE, " +
                    "FOREIGN KEY(holder_wrestler_id) REFERENCES wrestlers(id) ON DELETE SET NULL);");
                EnsureColumn(connection, transaction, "titles", "division", "TEXT NOT NULL DEFAULT 'Men''s'");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_titles_universe ON titles(universe_id, name);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS locations (id TEXT PRIMARY KEY NOT NULL, universe_id TEXT NOT NULL, venue_name TEXT NOT NULL, " +
                    "venue_location TEXT NOT NULL, capacity INTEGER NOT NULL CHECK(capacity >= 0), created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL, " +
                    "FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE);");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_locations_universe ON locations(universe_id, venue_name);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS brands (id TEXT PRIMARY KEY NOT NULL, universe_id TEXT NOT NULL, name TEXT NOT NULL, " +
                    "image_path TEXT, color_hex TEXT NOT NULL, created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL, " +
                    "FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE, UNIQUE(universe_id, name));");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_brands_universe ON brands(universe_id, name);");

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

        public List<UI.TeamRecord> LoadTeams(string universeId)
        {
            var results = new List<UI.TeamRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id, universe_id, name, brand, disposition, created_utc, photo_path FROM teams " +
                                      "WHERE universe_id = @universeId ORDER BY name COLLATE NOCASE;";
                AddParameter(command, "@universeId", universeId);
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) results.Add(new UI.TeamRecord { id = reader.GetString(0), universeId = reader.GetString(1),
                        name = reader.GetString(2), brand = reader.GetString(3), disposition = reader.GetString(4), createdUtc = reader.GetString(5),
                        photoPath = ReadNullableString(reader, 6) });
            }

            foreach (var team in results)
            {
                using (var connection = OpenConnection())
                using (var command = CreateCommand(connection))
                {
                    command.CommandText = "SELECT w.id, w.name FROM team_members tm JOIN wrestlers w ON w.id = tm.wrestler_id " +
                                          "WHERE tm.team_id = @teamId ORDER BY tm.position;";
                    AddParameter(command, "@teamId", team.id);
                    using (var reader = command.ExecuteReader())
                        while (reader.Read()) { team.memberIds.Add(reader.GetString(0)); team.memberNames.Add(reader.GetString(1)); }
                }
            }
            return results;
        }

        public void SaveTeam(UI.TeamRecord team)
        {
            if (team.memberIds.Count > 5) throw new InvalidOperationException("A team cannot have more than five members.");
            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT OR REPLACE INTO teams(id, universe_id, name, brand, disposition, photo_path, created_utc, updated_utc) " +
                                          "VALUES(@id, @universeId, @name, @brand, @disposition, @photo, @created, @updated);";
                    AddParameter(command, "@id", team.id); AddParameter(command, "@universeId", team.universeId);
                    AddParameter(command, "@name", team.name); AddParameter(command, "@brand", team.brand);
                    AddParameter(command, "@disposition", team.disposition); AddParameter(command, "@photo", team.photoPath);
                    AddParameter(command, "@created", team.createdUtc);
                    AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
                }
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction; command.CommandText = "DELETE FROM team_members WHERE team_id = @teamId;";
                    AddParameter(command, "@teamId", team.id); command.ExecuteNonQuery();
                }
                for (var index = 0; index < team.memberIds.Count; index++)
                {
                    using (var command = CreateCommand(connection))
                    {
                        command.Transaction = transaction;
                        command.CommandText = "INSERT INTO team_members(team_id, wrestler_id, position) VALUES(@teamId, @wrestlerId, @position);";
                        AddParameter(command, "@teamId", team.id); AddParameter(command, "@wrestlerId", team.memberIds[index]);
                        AddParameter(command, "@position", index); command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }

        public List<UI.TitleRecord> LoadTitles(string universeId)
        {
            var results = new List<UI.TitleRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT t.id, t.universe_id, t.name, t.brand, t.holder_wrestler_id, " +
                                      "COALESCE(w.name, 'Vacant'), t.image_path, t.created_utc, t.division FROM titles t " +
                                      "LEFT JOIN wrestlers w ON w.id = t.holder_wrestler_id WHERE t.universe_id = @universeId " +
                                      "ORDER BY t.name COLLATE NOCASE;";
                AddParameter(command, "@universeId", universeId);
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) results.Add(new UI.TitleRecord { id = reader.GetString(0), universeId = reader.GetString(1),
                        name = reader.GetString(2), brand = reader.GetString(3), holderWrestlerId = ReadNullableString(reader, 4),
                        holderName = reader.GetString(5), imagePath = ReadNullableString(reader, 6), createdUtc = reader.GetString(7),
                        division = reader.GetString(8) });
            }
            return results;
        }

        public void SaveTitle(UI.TitleRecord title)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "INSERT OR REPLACE INTO titles(id, universe_id, name, brand, division, holder_wrestler_id, image_path, created_utc, updated_utc) " +
                                      "VALUES(@id, @universeId, @name, @brand, @division, @holder, @image, @created, @updated);";
                AddParameter(command, "@id", title.id); AddParameter(command, "@universeId", title.universeId);
                AddParameter(command, "@name", title.name); AddParameter(command, "@brand", title.brand);
                AddParameter(command, "@division", title.division);
                AddParameter(command, "@holder", string.IsNullOrEmpty(title.holderWrestlerId) ? null : title.holderWrestlerId);
                AddParameter(command, "@image", title.imagePath); AddParameter(command, "@created", title.createdUtc);
                AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
            }
        }

        public List<UI.LocationRecord> LoadLocations(string universeId)
        {
            var results = new List<UI.LocationRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id, universe_id, venue_name, venue_location, capacity, created_utc FROM locations " +
                                      "WHERE universe_id = @universeId ORDER BY venue_name COLLATE NOCASE;";
                AddParameter(command, "@universeId", universeId);
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) results.Add(new UI.LocationRecord { id = reader.GetString(0), universeId = reader.GetString(1),
                        venueName = reader.GetString(2), venueLocation = reader.GetString(3), capacity = reader.GetInt32(4), createdUtc = reader.GetString(5) });
            }
            return results;
        }

        public void SaveLocation(UI.LocationRecord location)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "INSERT OR REPLACE INTO locations(id, universe_id, venue_name, venue_location, capacity, created_utc, updated_utc) " +
                                      "VALUES(@id, @universeId, @name, @location, @capacity, @created, @updated);";
                AddParameter(command, "@id", location.id); AddParameter(command, "@universeId", location.universeId);
                AddParameter(command, "@name", location.venueName); AddParameter(command, "@location", location.venueLocation);
                AddParameter(command, "@capacity", location.capacity); AddParameter(command, "@created", location.createdUtc);
                AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
            }
        }

        public List<UI.BrandRecord> LoadBrands(string universeId)
        {
            var results = new List<UI.BrandRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id, universe_id, name, image_path, color_hex, created_utc FROM brands " +
                                      "WHERE universe_id = @universeId ORDER BY name COLLATE NOCASE;";
                AddParameter(command, "@universeId", universeId);
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) results.Add(new UI.BrandRecord { id = reader.GetString(0), universeId = reader.GetString(1),
                        name = reader.GetString(2), imagePath = ReadNullableString(reader, 3), colorHex = reader.GetString(4), createdUtc = reader.GetString(5) });
            }
            return results;
        }

        public void SaveBrand(UI.BrandRecord brand)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "INSERT INTO brands(id, universe_id, name, image_path, color_hex, created_utc, updated_utc) " +
                                      "VALUES(@id, @universeId, @name, @image, @color, @created, @updated) " +
                                      "ON CONFLICT(id) DO UPDATE SET name=excluded.name, image_path=excluded.image_path, " +
                                      "color_hex=excluded.color_hex, updated_utc=excluded.updated_utc;";
                AddParameter(command, "@id", brand.id); AddParameter(command, "@universeId", brand.universeId);
                AddParameter(command, "@name", brand.name); AddParameter(command, "@image", brand.imagePath);
                AddParameter(command, "@color", brand.colorHex); AddParameter(command, "@created", brand.createdUtc);
                AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
            }
        }

        public void RenameBrandAssignments(string universeId, string oldName, string newName)
        {
            if (string.Equals(oldName, newName, StringComparison.Ordinal)) return;
            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var table in new[] { "wrestlers", "teams", "titles" })
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "UPDATE " + table + " SET brand = @newName WHERE universe_id = @universeId AND brand = @oldName;";
                    AddParameter(command, "@newName", newName); AddParameter(command, "@oldName", oldName);
                    AddParameter(command, "@universeId", universeId); command.ExecuteNonQuery();
                }
                transaction.Commit();
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

        private static void EnsureColumn(IDbConnection connection, IDbTransaction transaction, string table, string column, string definition)
        {
            var exists = false;
            using (var command = CreateCommand(connection))
            {
                command.Transaction = transaction;
                command.CommandText = "PRAGMA table_info(" + table + ");";
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase)) exists = true;
            }
            if (!exists) Execute(connection, transaction, "ALTER TABLE " + table + " ADD COLUMN " + column + " " + definition + ";");
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
