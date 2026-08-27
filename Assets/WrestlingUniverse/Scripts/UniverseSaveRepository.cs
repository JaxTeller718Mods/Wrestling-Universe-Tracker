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
        private const int CurrentSchemaVersion = 12;
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
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS tv_shows (id TEXT PRIMARY KEY NOT NULL, universe_id TEXT NOT NULL, name TEXT NOT NULL, " +
                    "frequency TEXT NOT NULL, day_of_week TEXT NOT NULL, image_path TEXT, created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL, " +
                    "FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS tv_show_brands (show_id TEXT NOT NULL, brand_id TEXT NOT NULL, position INTEGER NOT NULL, " +
                    "PRIMARY KEY(show_id, brand_id), FOREIGN KEY(show_id) REFERENCES tv_shows(id) ON DELETE CASCADE, " +
                    "FOREIGN KEY(brand_id) REFERENCES brands(id) ON DELETE CASCADE);");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_tv_shows_universe ON tv_shows(universe_id, name);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS specials (id TEXT PRIMARY KEY NOT NULL, universe_id TEXT NOT NULL, name TEXT NOT NULL, " +
                    "month TEXT NOT NULL, week TEXT NOT NULL, day_of_week TEXT NOT NULL, image_path TEXT, created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL, " +
                    "FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS special_brands (special_id TEXT NOT NULL, brand_id TEXT NOT NULL, position INTEGER NOT NULL, " +
                    "PRIMARY KEY(special_id, brand_id), FOREIGN KEY(special_id) REFERENCES specials(id) ON DELETE CASCADE, " +
                    "FOREIGN KEY(brand_id) REFERENCES brands(id) ON DELETE CASCADE);");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_specials_universe ON specials(universe_id, month, name);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS booked_matches (id TEXT PRIMARY KEY NOT NULL, universe_id TEXT NOT NULL, source_id TEXT NOT NULL, " +
                    "source_type TEXT NOT NULL, calendar_year INTEGER NOT NULL, calendar_month TEXT NOT NULL, calendar_week INTEGER NOT NULL, " +
                    "day_of_week TEXT NOT NULL, card_position INTEGER NOT NULL, stipulation TEXT NOT NULL, format TEXT NOT NULL, title_id TEXT, " +
                    "created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL, FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE);");
                EnsureColumn(connection, transaction, "booked_matches", "stage_one_stipulation", "TEXT");
                EnsureColumn(connection, transaction, "booked_matches", "stage_two_stipulation", "TEXT");
                EnsureColumn(connection, transaction, "booked_matches", "stage_three_stipulation", "TEXT");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS booked_match_participants (match_id TEXT NOT NULL, wrestler_id TEXT NOT NULL, position INTEGER NOT NULL, " +
                    "PRIMARY KEY(match_id, wrestler_id), FOREIGN KEY(match_id) REFERENCES booked_matches(id) ON DELETE CASCADE, " +
                    "FOREIGN KEY(wrestler_id) REFERENCES wrestlers(id) ON DELETE CASCADE);");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_booked_matches_show ON booked_matches(universe_id, source_id, calendar_year, calendar_month, calendar_week, day_of_week, card_position);");

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

        public List<UI.TvShowRecord> LoadTvShows(string universeId)
        {
            var results = new List<UI.TvShowRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id, universe_id, name, frequency, day_of_week, image_path, created_utc FROM tv_shows " +
                                      "WHERE universe_id = @universeId ORDER BY name COLLATE NOCASE;";
                AddParameter(command, "@universeId", universeId);
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) results.Add(new UI.TvShowRecord { id = reader.GetString(0), universeId = reader.GetString(1),
                        name = reader.GetString(2), frequency = reader.GetString(3), dayOfWeek = reader.GetString(4),
                        imagePath = ReadNullableString(reader, 5), createdUtc = reader.GetString(6) });
            }
            foreach (var show in results)
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT b.id, b.name FROM tv_show_brands sb JOIN brands b ON b.id = sb.brand_id " +
                                      "WHERE sb.show_id = @showId ORDER BY sb.position;";
                AddParameter(command, "@showId", show.id);
                using (var reader = command.ExecuteReader()) while (reader.Read()) { show.brandIds.Add(reader.GetString(0)); show.brandNames.Add(reader.GetString(1)); }
            }
            return results;
        }

        public void SaveTvShow(UI.TvShowRecord show)
        {
            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT OR REPLACE INTO tv_shows(id, universe_id, name, frequency, day_of_week, image_path, created_utc, updated_utc) " +
                                          "VALUES(@id, @universeId, @name, @frequency, @day, @image, @created, @updated);";
                    AddParameter(command, "@id", show.id); AddParameter(command, "@universeId", show.universeId);
                    AddParameter(command, "@name", show.name); AddParameter(command, "@frequency", show.frequency);
                    AddParameter(command, "@day", show.dayOfWeek); AddParameter(command, "@image", show.imagePath);
                    AddParameter(command, "@created", show.createdUtc); AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
                }
                using (var command = CreateCommand(connection))
                { command.Transaction = transaction; command.CommandText = "DELETE FROM tv_show_brands WHERE show_id = @id;"; AddParameter(command, "@id", show.id); command.ExecuteNonQuery(); }
                for (var index = 0; index < show.brandIds.Count; index++)
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction; command.CommandText = "INSERT INTO tv_show_brands(show_id, brand_id, position) VALUES(@show, @brand, @position);";
                    AddParameter(command, "@show", show.id); AddParameter(command, "@brand", show.brandIds[index]); AddParameter(command, "@position", index); command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        public List<UI.SpecialRecord> LoadSpecials(string universeId)
        {
            var results = new List<UI.SpecialRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id, universe_id, name, month, week, day_of_week, image_path, created_utc FROM specials " +
                                      "WHERE universe_id = @universeId ORDER BY rowid;";
                AddParameter(command, "@universeId", universeId);
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) results.Add(new UI.SpecialRecord { id = reader.GetString(0), universeId = reader.GetString(1),
                        name = reader.GetString(2), month = reader.GetString(3), week = reader.GetString(4), dayOfWeek = reader.GetString(5),
                        imagePath = ReadNullableString(reader, 6), createdUtc = reader.GetString(7) });
            }
            foreach (var special in results)
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT b.id, b.name FROM special_brands sb JOIN brands b ON b.id = sb.brand_id " +
                                      "WHERE sb.special_id = @specialId ORDER BY sb.position;";
                AddParameter(command, "@specialId", special.id);
                using (var reader = command.ExecuteReader()) while (reader.Read()) { special.brandIds.Add(reader.GetString(0)); special.brandNames.Add(reader.GetString(1)); }
            }
            return results;
        }

        public void SaveSpecial(UI.SpecialRecord special)
        {
            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT OR REPLACE INTO specials(id, universe_id, name, month, week, day_of_week, image_path, created_utc, updated_utc) " +
                                          "VALUES(@id, @universeId, @name, @month, @week, @day, @image, @created, @updated);";
                    AddParameter(command, "@id", special.id); AddParameter(command, "@universeId", special.universeId);
                    AddParameter(command, "@name", special.name); AddParameter(command, "@month", special.month);
                    AddParameter(command, "@week", special.week); AddParameter(command, "@day", special.dayOfWeek);
                    AddParameter(command, "@image", special.imagePath); AddParameter(command, "@created", special.createdUtc);
                    AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
                }
                using (var command = CreateCommand(connection))
                { command.Transaction = transaction; command.CommandText = "DELETE FROM special_brands WHERE special_id = @id;"; AddParameter(command, "@id", special.id); command.ExecuteNonQuery(); }
                for (var index = 0; index < special.brandIds.Count; index++)
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction; command.CommandText = "INSERT INTO special_brands(special_id, brand_id, position) VALUES(@special, @brand, @position);";
                    AddParameter(command, "@special", special.id); AddParameter(command, "@brand", special.brandIds[index]); AddParameter(command, "@position", index); command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        public List<UI.BookedMatchRecord> LoadBookedMatches(string universeId, string sourceId, int year, string month, int week, string dayOfWeek)
        {
            var results = new List<UI.BookedMatchRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT m.id, m.universe_id, m.source_id, m.source_type, m.calendar_year, m.calendar_month, m.calendar_week, " +
                    "m.day_of_week, m.card_position, m.stipulation, m.format, COALESCE(m.title_id, ''), COALESCE(t.name, ''), " +
                    "COALESCE(m.stage_one_stipulation, ''), COALESCE(m.stage_two_stipulation, ''), COALESCE(m.stage_three_stipulation, ''), m.created_utc " +
                    "FROM booked_matches m LEFT JOIN titles t ON t.id = m.title_id WHERE m.universe_id=@universe AND m.source_id=@source " +
                    "AND m.calendar_year=@year AND m.calendar_month=@month AND m.calendar_week=@week AND m.day_of_week=@day ORDER BY m.card_position;";
                AddParameter(command, "@universe", universeId); AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year);
                AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek);
                using (var reader = command.ExecuteReader()) while (reader.Read()) results.Add(new UI.BookedMatchRecord {
                    id = reader.GetString(0), universeId = reader.GetString(1), sourceId = reader.GetString(2), sourceType = reader.GetString(3),
                    year = reader.GetInt32(4), month = reader.GetString(5), week = reader.GetInt32(6), dayOfWeek = reader.GetString(7),
                    cardPosition = reader.GetInt32(8), stipulation = reader.GetString(9), format = reader.GetString(10), titleId = reader.GetString(11),
                    titleName = reader.GetString(12), stageOneStipulation = reader.GetString(13), stageTwoStipulation = reader.GetString(14),
                    stageThreeStipulation = reader.GetString(15), createdUtc = reader.GetString(16) });
            }
            var wrestlers = LoadWrestlers(universeId);
            foreach (var match in results)
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT wrestler_id FROM booked_match_participants WHERE match_id=@match ORDER BY position;";
                AddParameter(command, "@match", match.id);
                using (var reader = command.ExecuteReader()) while (reader.Read())
                {
                    var id = reader.GetString(0); match.participantIds.Add(id);
                    var wrestler = wrestlers.Find(item => item.id == id); if (wrestler != null) match.participants.Add(wrestler);
                }
            }
            return results;
        }

        public void SaveBookedMatch(UI.BookedMatchRecord match)
        {
            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT OR REPLACE INTO booked_matches(id, universe_id, source_id, source_type, calendar_year, calendar_month, " +
                        "calendar_week, day_of_week, card_position, stipulation, format, title_id, stage_one_stipulation, stage_two_stipulation, " +
                        "stage_three_stipulation, created_utc, updated_utc) VALUES(@id,@universe,@source,@type,@year,@month,@week,@day,@position," +
                        "@stipulation,@format,@title,@stageOne,@stageTwo,@stageThree,@created,@updated);";
                    AddParameter(command, "@id", match.id); AddParameter(command, "@universe", match.universeId); AddParameter(command, "@source", match.sourceId);
                    AddParameter(command, "@type", match.sourceType); AddParameter(command, "@year", match.year); AddParameter(command, "@month", match.month);
                    AddParameter(command, "@week", match.week); AddParameter(command, "@day", match.dayOfWeek); AddParameter(command, "@position", match.cardPosition);
                    AddParameter(command, "@stipulation", match.stipulation); AddParameter(command, "@format", match.format);
                    AddParameter(command, "@title", string.IsNullOrEmpty(match.titleId) ? null : match.titleId);
                    AddParameter(command, "@stageOne", string.IsNullOrEmpty(match.stageOneStipulation) ? null : match.stageOneStipulation);
                    AddParameter(command, "@stageTwo", string.IsNullOrEmpty(match.stageTwoStipulation) ? null : match.stageTwoStipulation);
                    AddParameter(command, "@stageThree", string.IsNullOrEmpty(match.stageThreeStipulation) ? null : match.stageThreeStipulation);
                    AddParameter(command, "@created", match.createdUtc); AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
                }
                using (var command = CreateCommand(connection))
                { command.Transaction = transaction; command.CommandText = "DELETE FROM booked_match_participants WHERE match_id=@id;"; AddParameter(command, "@id", match.id); command.ExecuteNonQuery(); }
                for (var index = 0; index < match.participantIds.Count; index++)
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction; command.CommandText = "INSERT INTO booked_match_participants(match_id,wrestler_id,position) VALUES(@match,@wrestler,@position);";
                    AddParameter(command, "@match", match.id); AddParameter(command, "@wrestler", match.participantIds[index]); AddParameter(command, "@position", index); command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        public void DeleteBookedMatch(string matchId)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            { command.CommandText = "DELETE FROM booked_matches WHERE id=@id;"; AddParameter(command, "@id", matchId); command.ExecuteNonQuery(); }
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
