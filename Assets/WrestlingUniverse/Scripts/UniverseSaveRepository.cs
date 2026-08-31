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
        private const int CurrentSchemaVersion = 18;
        private readonly string connectionString;
        private readonly string isolatedUniverseId;

        public string DatabasePath { get; }

        public UniverseSaveRepository(string universeId = null)
        {
            isolatedUniverseId = universeId;
            if (string.IsNullOrEmpty(universeId)) DatabasePath = UniverseStoragePaths.CatalogDatabase;
            else
            {
                UniverseStoragePaths.EnsureDirectories(universeId);
                DatabasePath = UniverseStoragePaths.GetDatabase(universeId);
            }
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
                    "CREATE TABLE IF NOT EXISTS title_reigns (id TEXT PRIMARY KEY NOT NULL, title_id TEXT NOT NULL, universe_id TEXT NOT NULL, " +
                    "reign_number INTEGER NOT NULL, holder_wrestler_id TEXT, holder_name TEXT NOT NULL, won_match_id TEXT NOT NULL UNIQUE, " +
                    "won_show_name TEXT NOT NULL, won_year INTEGER NOT NULL, won_month TEXT NOT NULL, won_week INTEGER NOT NULL, won_day_of_week TEXT NOT NULL, " +
                    "lost_show_name TEXT, lost_year INTEGER, lost_month TEXT, lost_week INTEGER, lost_day_of_week TEXT, created_utc TEXT NOT NULL, " +
                    "FOREIGN KEY(title_id) REFERENCES titles(id) ON DELETE CASCADE, FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE, " +
                    "FOREIGN KEY(holder_wrestler_id) REFERENCES wrestlers(id) ON DELETE SET NULL, UNIQUE(title_id,reign_number));");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_title_reigns_title ON title_reigns(title_id,reign_number DESC);");
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
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS booked_segments (id TEXT PRIMARY KEY NOT NULL, universe_id TEXT NOT NULL, source_id TEXT NOT NULL, " +
                    "source_type TEXT NOT NULL, calendar_year INTEGER NOT NULL, calendar_month TEXT NOT NULL, calendar_week INTEGER NOT NULL, " +
                    "day_of_week TEXT NOT NULL, card_position INTEGER NOT NULL, title TEXT NOT NULL, summary TEXT NOT NULL, created_utc TEXT NOT NULL, " +
                    "updated_utc TEXT NOT NULL, FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS booked_segment_participants (segment_id TEXT NOT NULL, wrestler_id TEXT NOT NULL, position INTEGER NOT NULL, " +
                    "PRIMARY KEY(segment_id, wrestler_id), FOREIGN KEY(segment_id) REFERENCES booked_segments(id) ON DELETE CASCADE, " +
                    "FOREIGN KEY(wrestler_id) REFERENCES wrestlers(id) ON DELETE CASCADE);");
                Execute(connection, transaction, "CREATE INDEX IF NOT EXISTS idx_booked_segments_show ON booked_segments(universe_id, source_id, calendar_year, calendar_month, calendar_week, day_of_week, card_position);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS booked_show_cards (universe_id TEXT NOT NULL, source_id TEXT NOT NULL, calendar_year INTEGER NOT NULL, " +
                    "calendar_month TEXT NOT NULL, calendar_week INTEGER NOT NULL, day_of_week TEXT NOT NULL, is_locked INTEGER NOT NULL DEFAULT 0, " +
                    "updated_utc TEXT NOT NULL, PRIMARY KEY(universe_id,source_id,calendar_year,calendar_month,calendar_week,day_of_week), " +
                    "FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE);");
                EnsureColumn(connection, transaction, "booked_show_cards", "results_finalized", "INTEGER NOT NULL DEFAULT 0");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS booked_show_venues (universe_id TEXT NOT NULL, source_id TEXT NOT NULL, calendar_year INTEGER NOT NULL, " +
                    "calendar_month TEXT NOT NULL, calendar_week INTEGER NOT NULL, day_of_week TEXT NOT NULL, location_id TEXT NOT NULL, " +
                    "updated_utc TEXT NOT NULL, PRIMARY KEY(universe_id,source_id,calendar_year,calendar_month,calendar_week,day_of_week), " +
                    "FOREIGN KEY(universe_id) REFERENCES universes(id) ON DELETE CASCADE, " +
                    "FOREIGN KEY(location_id) REFERENCES locations(id) ON DELETE CASCADE);");
                Execute(connection, transaction,
                    "CREATE TABLE IF NOT EXISTS booked_match_results (match_id TEXT PRIMARY KEY NOT NULL, winner_wrestler_id TEXT NOT NULL, " +
                    "finish_type TEXT NOT NULL, rating INTEGER NOT NULL DEFAULT 0 CHECK(rating BETWEEN 0 AND 100), duration TEXT, notes TEXT, " +
                    "title_changed INTEGER NOT NULL DEFAULT 0, created_utc TEXT NOT NULL, updated_utc TEXT NOT NULL, " +
                    "FOREIGN KEY(match_id) REFERENCES booked_matches(id) ON DELETE CASCADE, " +
                    "FOREIGN KEY(winner_wrestler_id) REFERENCES wrestlers(id) ON DELETE RESTRICT);");
                EnsureColumn(connection, transaction, "booked_match_results", "is_draw", "INTEGER NOT NULL DEFAULT 0");

                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT OR REPLACE INTO app_metadata(key, value) VALUES('schema_version', @version);";
                    AddParameter(command, "@version", CurrentSchemaVersion.ToString());
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            if (!string.IsNullOrEmpty(isolatedUniverseId)) MigrateUniverseFromCatalogIfNeeded();
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
                    "INSERT INTO universes " +
                    "(id, owner_name, promotion_name, promotion_initials, start_date, owner_image_path, " +
                    "promotion_image_path, created_utc, updated_utc) VALUES " +
                    "(@id, @owner, @promotion, @initials, @startDate, @ownerImage, @promotionImage, @created, @updated) " +
                    "ON CONFLICT(id) DO UPDATE SET owner_name=excluded.owner_name,promotion_name=excluded.promotion_name," +
                    "promotion_initials=excluded.promotion_initials,start_date=excluded.start_date,owner_image_path=excluded.owner_image_path," +
                    "promotion_image_path=excluded.promotion_image_path,updated_utc=excluded.updated_utc;";
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
                command.CommandText = "INSERT INTO wrestlers " +
                    "(id, universe_id, name, brand, disposition, gender, tier, overall, photo_path, created_utc, updated_utc) " +
                    "VALUES (@id, @universeId, @name, @brand, @disposition, @gender, @tier, @overall, @photo, @created, @updated) " +
                    "ON CONFLICT(id) DO UPDATE SET name=excluded.name,brand=excluded.brand,disposition=excluded.disposition," +
                    "gender=excluded.gender,tier=excluded.tier,overall=excluded.overall,photo_path=excluded.photo_path,updated_utc=excluded.updated_utc;";
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
                    command.CommandText = "INSERT INTO teams(id, universe_id, name, brand, disposition, photo_path, created_utc, updated_utc) " +
                        "VALUES(@id,@universeId,@name,@brand,@disposition,@photo,@created,@updated) ON CONFLICT(id) DO UPDATE SET " +
                        "name=excluded.name,brand=excluded.brand,disposition=excluded.disposition,photo_path=excluded.photo_path,updated_utc=excluded.updated_utc;";
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
                command.CommandText = "INSERT INTO titles(id, universe_id, name, brand, division, holder_wrestler_id, image_path, created_utc, updated_utc) " +
                    "VALUES(@id,@universeId,@name,@brand,@division,@holder,@image,@created,@updated) " +
                    "ON CONFLICT(id) DO UPDATE SET name=excluded.name,brand=excluded.brand,division=excluded.division," +
                    "holder_wrestler_id=CASE WHEN EXISTS(SELECT 1 FROM title_reigns WHERE title_id=excluded.id) " +
                    "THEN titles.holder_wrestler_id ELSE excluded.holder_wrestler_id END,image_path=excluded.image_path,updated_utc=excluded.updated_utc;";
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
                command.CommandText = "INSERT INTO locations(id, universe_id, venue_name, venue_location, capacity, created_utc, updated_utc) " +
                    "VALUES(@id,@universeId,@name,@location,@capacity,@created,@updated) ON CONFLICT(id) DO UPDATE SET " +
                    "venue_name=excluded.venue_name,venue_location=excluded.venue_location,capacity=excluded.capacity,updated_utc=excluded.updated_utc;";
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
                    command.CommandText = "INSERT INTO tv_shows(id, universe_id, name, frequency, day_of_week, image_path, created_utc, updated_utc) " +
                        "VALUES(@id,@universeId,@name,@frequency,@day,@image,@created,@updated) ON CONFLICT(id) DO UPDATE SET " +
                        "name=excluded.name,frequency=excluded.frequency,day_of_week=excluded.day_of_week,image_path=excluded.image_path,updated_utc=excluded.updated_utc;";
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
                    command.CommandText = "INSERT INTO specials(id, universe_id, name, month, week, day_of_week, image_path, created_utc, updated_utc) " +
                        "VALUES(@id,@universeId,@name,@month,@week,@day,@image,@created,@updated) ON CONFLICT(id) DO UPDATE SET " +
                        "name=excluded.name,month=excluded.month,week=excluded.week,day_of_week=excluded.day_of_week," +
                        "image_path=excluded.image_path,updated_utc=excluded.updated_utc;";
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

        private void MigrateUniverseFromCatalogIfNeeded()
        {
            var catalog = UniverseStoragePaths.CatalogDatabase;
            if (!File.Exists(catalog) || string.Equals(Path.GetFullPath(catalog), Path.GetFullPath(DatabasePath), StringComparison.OrdinalIgnoreCase)) return;
            using (var connection = OpenConnection())
            using (var count = CreateCommand(connection))
            {
                count.CommandText = "SELECT COUNT(*) FROM universes WHERE id=@id;";
                AddParameter(count, "@id", isolatedUniverseId);
                if (Convert.ToInt32(count.ExecuteScalar()) > 0) return;
                Execute(connection, null, "ATTACH DATABASE '" + catalog.Replace("'", "''") + "' AS catalog;");
                using (var transaction = connection.BeginTransaction())
                {
                    var id = isolatedUniverseId.Replace("'", "''");
                    Execute(connection, transaction, "INSERT OR IGNORE INTO universes SELECT * FROM catalog.universes WHERE id='" + id + "';");
                    foreach (var table in new[] { "wrestlers", "teams", "titles", "title_reigns", "locations", "brands", "tv_shows", "specials", "booked_matches", "booked_segments", "booked_show_cards", "booked_show_venues" })
                        Execute(connection, transaction, "INSERT OR IGNORE INTO " + table + " SELECT * FROM catalog." + table + " WHERE universe_id='" + id + "';");
                    Execute(connection, transaction, "INSERT OR IGNORE INTO team_members SELECT tm.* FROM catalog.team_members tm JOIN catalog.teams t ON t.id=tm.team_id WHERE t.universe_id='" + id + "';");
                    Execute(connection, transaction, "INSERT OR IGNORE INTO tv_show_brands SELECT sb.* FROM catalog.tv_show_brands sb JOIN catalog.tv_shows s ON s.id=sb.show_id WHERE s.universe_id='" + id + "';");
                    Execute(connection, transaction, "INSERT OR IGNORE INTO special_brands SELECT sb.* FROM catalog.special_brands sb JOIN catalog.specials s ON s.id=sb.special_id WHERE s.universe_id='" + id + "';");
                    Execute(connection, transaction, "INSERT OR IGNORE INTO booked_match_participants SELECT p.* FROM catalog.booked_match_participants p JOIN catalog.booked_matches m ON m.id=p.match_id WHERE m.universe_id='" + id + "';");
                    Execute(connection, transaction, "INSERT OR IGNORE INTO booked_segment_participants SELECT p.* FROM catalog.booked_segment_participants p JOIN catalog.booked_segments s ON s.id=p.segment_id WHERE s.universe_id='" + id + "';");
                    Execute(connection, transaction, "INSERT OR IGNORE INTO booked_match_results SELECT r.* FROM catalog.booked_match_results r JOIN catalog.booked_matches m ON m.id=r.match_id WHERE m.universe_id='" + id + "';");
                    transaction.Commit();
                }
                Execute(connection, null, "DETACH DATABASE catalog;");
            }
            var backup = Path.Combine(UniverseStoragePaths.GetBackups(isolatedUniverseId), "universe-migrated-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".db");
            File.Copy(DatabasePath, backup, false);
        }

        public List<UI.TitleReignRecord> LoadTitleReigns(string titleId)
        {
            var results = new List<UI.TitleReignRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id,title_id,holder_wrestler_id,holder_name,reign_number,won_show_name,won_year,won_month,won_week,won_day_of_week," +
                    "lost_show_name,lost_year,lost_month,lost_week,lost_day_of_week FROM title_reigns WHERE title_id=@title ORDER BY reign_number DESC;";
                AddParameter(command, "@title", titleId);
                using (var reader = command.ExecuteReader())
                    while (reader.Read()) results.Add(new UI.TitleReignRecord {
                        id = reader.GetString(0), titleId = reader.GetString(1), holderWrestlerId = ReadNullableString(reader, 2),
                        holderName = reader.GetString(3), reignNumber = reader.GetInt32(4), wonShowName = reader.GetString(5),
                        wonYear = reader.GetInt32(6), wonMonth = reader.GetString(7), wonWeek = reader.GetInt32(8), wonDayOfWeek = reader.GetString(9),
                        lostShowName = ReadNullableString(reader, 10), lostYear = reader.IsDBNull(11) ? (int?)null : reader.GetInt32(11),
                        lostMonth = ReadNullableString(reader, 12), lostWeek = reader.IsDBNull(13) ? (int?)null : reader.GetInt32(13),
                        lostDayOfWeek = ReadNullableString(reader, 14)
                    });
            }
            return results;
        }

        public List<UI.BookedMatchRecord> LoadBookedMatches(string universeId, string sourceId, int year, string month, int week, string dayOfWeek)
        {
            var results = new List<UI.BookedMatchRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT m.id, m.universe_id, m.source_id, m.source_type, m.calendar_year, m.calendar_month, m.calendar_week, " +
                    "m.day_of_week, m.card_position, m.stipulation, m.format, COALESCE(m.title_id, ''), COALESCE(t.name, ''), COALESCE(t.image_path, ''), " +
                    "COALESCE(m.stage_one_stipulation, ''), COALESCE(m.stage_two_stipulation, ''), COALESCE(m.stage_three_stipulation, ''), m.created_utc " +
                    "FROM booked_matches m LEFT JOIN titles t ON t.id = m.title_id WHERE m.universe_id=@universe AND m.source_id=@source " +
                    "AND m.calendar_year=@year AND m.calendar_month=@month AND m.calendar_week=@week AND m.day_of_week=@day ORDER BY m.card_position;";
                AddParameter(command, "@universe", universeId); AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year);
                AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek);
                using (var reader = command.ExecuteReader()) while (reader.Read()) results.Add(new UI.BookedMatchRecord {
                    id = reader.GetString(0), universeId = reader.GetString(1), sourceId = reader.GetString(2), sourceType = reader.GetString(3),
                    year = reader.GetInt32(4), month = reader.GetString(5), week = reader.GetInt32(6), dayOfWeek = reader.GetString(7),
                    cardPosition = reader.GetInt32(8), stipulation = reader.GetString(9), format = reader.GetString(10), titleId = reader.GetString(11),
                    titleName = reader.GetString(12), titleImagePath = reader.GetString(13), stageOneStipulation = reader.GetString(14),
                    stageTwoStipulation = reader.GetString(15), stageThreeStipulation = reader.GetString(16), createdUtc = reader.GetString(17) });
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
                    command.CommandText = "INSERT INTO booked_matches(id, universe_id, source_id, source_type, calendar_year, calendar_month, " +
                        "calendar_week, day_of_week, card_position, stipulation, format, title_id, stage_one_stipulation, stage_two_stipulation, " +
                        "stage_three_stipulation, created_utc, updated_utc) VALUES(@id,@universe,@source,@type,@year,@month,@week,@day,@position," +
                        "@stipulation,@format,@title,@stageOne,@stageTwo,@stageThree,@created,@updated) ON CONFLICT(id) DO UPDATE SET " +
                        "source_id=excluded.source_id,source_type=excluded.source_type,calendar_year=excluded.calendar_year," +
                        "calendar_month=excluded.calendar_month,calendar_week=excluded.calendar_week,day_of_week=excluded.day_of_week," +
                        "card_position=excluded.card_position,stipulation=excluded.stipulation,format=excluded.format,title_id=excluded.title_id," +
                        "stage_one_stipulation=excluded.stage_one_stipulation,stage_two_stipulation=excluded.stage_two_stipulation," +
                        "stage_three_stipulation=excluded.stage_three_stipulation,updated_utc=excluded.updated_utc;";
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

        public List<UI.BookedSegmentRecord> LoadBookedSegments(string universeId, string sourceId, int year, string month, int week, string dayOfWeek)
        {
            var results = new List<UI.BookedSegmentRecord>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT id, universe_id, source_id, source_type, calendar_year, calendar_month, calendar_week, day_of_week, " +
                    "card_position, title, summary, created_utc FROM booked_segments WHERE universe_id=@universe AND source_id=@source " +
                    "AND calendar_year=@year AND calendar_month=@month AND calendar_week=@week AND day_of_week=@day ORDER BY card_position;";
                AddParameter(command, "@universe", universeId); AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year);
                AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek);
                using (var reader = command.ExecuteReader()) while (reader.Read()) results.Add(new UI.BookedSegmentRecord {
                    id = reader.GetString(0), universeId = reader.GetString(1), sourceId = reader.GetString(2), sourceType = reader.GetString(3),
                    year = reader.GetInt32(4), month = reader.GetString(5), week = reader.GetInt32(6), dayOfWeek = reader.GetString(7),
                    cardPosition = reader.GetInt32(8), title = reader.GetString(9), summary = reader.GetString(10), createdUtc = reader.GetString(11) });
            }
            var wrestlers = LoadWrestlers(universeId);
            foreach (var segment in results)
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT wrestler_id FROM booked_segment_participants WHERE segment_id=@segment ORDER BY position;";
                AddParameter(command, "@segment", segment.id);
                using (var reader = command.ExecuteReader()) while (reader.Read())
                {
                    var id = reader.GetString(0); segment.participantIds.Add(id);
                    var wrestler = wrestlers.Find(item => item.id == id); if (wrestler != null) segment.participants.Add(wrestler);
                }
            }
            return results;
        }

        public void SaveBookedSegment(UI.BookedSegmentRecord segment)
        {
            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT INTO booked_segments(id,universe_id,source_id,source_type,calendar_year,calendar_month," +
                        "calendar_week,day_of_week,card_position,title,summary,created_utc,updated_utc) VALUES(@id,@universe,@source,@type,@year," +
                        "@month,@week,@day,@position,@title,@summary,@created,@updated) ON CONFLICT(id) DO UPDATE SET " +
                        "source_id=excluded.source_id,source_type=excluded.source_type,calendar_year=excluded.calendar_year," +
                        "calendar_month=excluded.calendar_month,calendar_week=excluded.calendar_week,day_of_week=excluded.day_of_week," +
                        "card_position=excluded.card_position,title=excluded.title,summary=excluded.summary,updated_utc=excluded.updated_utc;";
                    AddParameter(command, "@id", segment.id); AddParameter(command, "@universe", segment.universeId);
                    AddParameter(command, "@source", segment.sourceId); AddParameter(command, "@type", segment.sourceType);
                    AddParameter(command, "@year", segment.year); AddParameter(command, "@month", segment.month); AddParameter(command, "@week", segment.week);
                    AddParameter(command, "@day", segment.dayOfWeek); AddParameter(command, "@position", segment.cardPosition);
                    AddParameter(command, "@title", segment.title); AddParameter(command, "@summary", segment.summary);
                    AddParameter(command, "@created", segment.createdUtc); AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
                }
                using (var command = CreateCommand(connection))
                { command.Transaction = transaction; command.CommandText = "DELETE FROM booked_segment_participants WHERE segment_id=@id;"; AddParameter(command, "@id", segment.id); command.ExecuteNonQuery(); }
                for (var index = 0; index < segment.participantIds.Count; index++)
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "INSERT INTO booked_segment_participants(segment_id,wrestler_id,position) VALUES(@segment,@wrestler,@position);";
                    AddParameter(command, "@segment", segment.id); AddParameter(command, "@wrestler", segment.participantIds[index]);
                    AddParameter(command, "@position", index); command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        public void DeleteBookedSegment(string segmentId)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            { command.CommandText = "DELETE FROM booked_segments WHERE id=@id;"; AddParameter(command, "@id", segmentId); command.ExecuteNonQuery(); }
        }

        public bool IsShowCardLocked(string universeId, string sourceId, int year, string month, int week, string dayOfWeek)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT is_locked FROM booked_show_cards WHERE universe_id=@universe AND source_id=@source AND " +
                    "calendar_year=@year AND calendar_month=@month AND calendar_week=@week AND day_of_week=@day LIMIT 1;";
                AddParameter(command, "@universe", universeId); AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year);
                AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek);
                var value = command.ExecuteScalar(); return value != null && value != DBNull.Value && Convert.ToInt32(value) != 0;
            }
        }

        public void SetShowCardLocked(string universeId, string sourceId, int year, string month, int week, string dayOfWeek, bool locked)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "INSERT OR REPLACE INTO booked_show_cards(universe_id,source_id,calendar_year,calendar_month,calendar_week," +
                    "day_of_week,is_locked,updated_utc) VALUES(@universe,@source,@year,@month,@week,@day,@locked,@updated);";
                AddParameter(command, "@universe", universeId); AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year);
                AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek);
                AddParameter(command, "@locked", locked ? 1 : 0); AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
            }
        }

        public string GetShowVenueId(string universeId, string sourceId, int year, string month, int week, string dayOfWeek)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT location_id FROM booked_show_venues WHERE universe_id=@universe AND source_id=@source AND " +
                    "calendar_year=@year AND calendar_month=@month AND calendar_week=@week AND day_of_week=@day LIMIT 1;";
                AddParameter(command, "@universe", universeId); AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year);
                AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek);
                var value = command.ExecuteScalar(); return value == null || value == DBNull.Value ? string.Empty : Convert.ToString(value);
            }
        }

        public void SetShowVenue(string universeId, string sourceId, int year, string month, int week, string dayOfWeek, string locationId)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                if (string.IsNullOrEmpty(locationId))
                {
                    command.CommandText = "DELETE FROM booked_show_venues WHERE universe_id=@universe AND source_id=@source AND calendar_year=@year " +
                        "AND calendar_month=@month AND calendar_week=@week AND day_of_week=@day;";
                }
                else
                {
                    command.CommandText = "INSERT OR REPLACE INTO booked_show_venues(universe_id,source_id,calendar_year,calendar_month,calendar_week," +
                        "day_of_week,location_id,updated_utc) VALUES(@universe,@source,@year,@month,@week,@day,@location,@updated);";
                    AddParameter(command, "@location", locationId); AddParameter(command, "@updated", DateTime.UtcNow.ToString("O"));
                }
                AddParameter(command, "@universe", universeId); AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year);
                AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek); command.ExecuteNonQuery();
            }
        }

        public UI.MatchResultRecord LoadMatchResult(string matchId)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT match_id,winner_wrestler_id,finish_type,rating,COALESCE(duration,''),COALESCE(notes,'')," +
                    "title_changed,created_utc,is_draw FROM booked_match_results WHERE match_id=@match LIMIT 1;";
                AddParameter(command, "@match", matchId);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new UI.MatchResultRecord { matchId = reader.GetString(0), winnerWrestlerId = reader.GetString(1),
                        finishType = reader.GetString(2), rating = reader.GetInt32(3), duration = reader.GetString(4), notes = reader.GetString(5),
                        titleChanged = reader.GetInt32(6) != 0, createdUtc = reader.GetString(7), isDraw = reader.GetInt32(8) != 0 };
                }
            }
        }

        public void SaveMatchResult(UI.MatchResultRecord result)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "INSERT OR REPLACE INTO booked_match_results(match_id,winner_wrestler_id,finish_type,rating,duration,notes," +
                    "title_changed,created_utc,updated_utc,is_draw) VALUES(@match,@winner,@finish,@rating,@duration,@notes,@changed,@created,@updated,@draw);";
                AddParameter(command, "@match", result.matchId); AddParameter(command, "@winner", result.winnerWrestlerId);
                AddParameter(command, "@finish", result.finishType); AddParameter(command, "@rating", result.rating);
                AddParameter(command, "@duration", result.duration); AddParameter(command, "@notes", result.notes);
                AddParameter(command, "@changed", result.titleChanged ? 1 : 0); AddParameter(command, "@created", result.createdUtc);
                AddParameter(command, "@draw", result.isDraw ? 1 : 0);
                AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
            }
        }

        public bool AreShowResultsFinalized(string universeId, string sourceId, int year, string month, int week, string dayOfWeek)
        {
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT results_finalized FROM booked_show_cards WHERE universe_id=@universe AND source_id=@source AND " +
                    "calendar_year=@year AND calendar_month=@month AND calendar_week=@week AND day_of_week=@day LIMIT 1;";
                AddParameter(command, "@universe", universeId); AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year);
                AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek);
                var value = command.ExecuteScalar(); return value != null && value != DBNull.Value && Convert.ToInt32(value) != 0;
            }
        }

        public long GetLatestFinalizedShowOrdinal(string universeId)
        {
            long latest = -1;
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT calendar_year,calendar_month,calendar_week,day_of_week FROM booked_show_cards " +
                    "WHERE universe_id=@universe AND results_finalized=1;";
                AddParameter(command, "@universe", universeId);
                using (var reader = command.ExecuteReader()) while (reader.Read())
                {
                    var month = CalendarMonthIndex(reader.GetString(1)); var day = CalendarDayIndex(reader.GetString(3));
                    if (month < 0 || day < 0) continue;
                    var value = (((long)reader.GetInt32(0) * 12 + month) * 4 + Math.Max(0, reader.GetInt32(2) - 1)) * 7 + day;
                    if (value > latest) latest = value;
                }
            }
            return latest;
        }

        private static int CalendarMonthIndex(string month)
        {
            return Array.FindIndex(new[] { "January", "February", "March", "April", "May", "June", "July", "August",
                "September", "October", "November", "December" }, value => string.Equals(value, month, StringComparison.OrdinalIgnoreCase));
        }

        private static int CalendarDayIndex(string day)
        {
            return Array.FindIndex(new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" },
                value => string.Equals(value, day, StringComparison.OrdinalIgnoreCase));
        }

        public void FinalizeShowResults(string universeId, string sourceId, string sourceName, int year, string month, int week, string dayOfWeek)
        {
            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                var titleChanges = new List<Tuple<string, string, string>>();
                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "SELECT m.id,m.title_id,r.winner_wrestler_id FROM booked_matches m JOIN booked_match_results r ON r.match_id=m.id " +
                        "WHERE m.universe_id=@universe AND m.source_id=@source AND m.calendar_year=@year AND m.calendar_month=@month " +
                        "AND m.calendar_week=@week AND m.day_of_week=@day AND m.title_id IS NOT NULL AND m.title_id<>'' " +
                        "AND r.title_changed=1 AND r.is_draw=0 ORDER BY m.card_position;";
                    AddParameter(command, "@universe", universeId); AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year);
                    AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek);
                    using (var reader = command.ExecuteReader()) while (reader.Read())
                        titleChanges.Add(Tuple.Create(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
                }

                foreach (var change in titleChanges)
                {
                    string currentHolder = null; string winnerName = null; var alreadyApplied = false;
                    using (var command = CreateCommand(connection))
                    {
                        command.Transaction = transaction;
                        command.CommandText = "SELECT EXISTS(SELECT 1 FROM title_reigns WHERE won_match_id=@match)," +
                            "(SELECT holder_wrestler_id FROM titles WHERE id=@title),(SELECT name FROM wrestlers WHERE id=@winner);";
                        AddParameter(command, "@match", change.Item1); AddParameter(command, "@title", change.Item2); AddParameter(command, "@winner", change.Item3);
                        using (var reader = command.ExecuteReader()) if (reader.Read()) {
                            alreadyApplied = reader.GetInt32(0) != 0; currentHolder = ReadNullableString(reader, 1); winnerName = ReadNullableString(reader, 2);
                        }
                    }
                    if (alreadyApplied || string.IsNullOrEmpty(winnerName) || currentHolder == change.Item3) continue;

                    using (var command = CreateCommand(connection))
                    {
                        command.Transaction = transaction;
                        command.CommandText = "UPDATE title_reigns SET lost_show_name=@show,lost_year=@year,lost_month=@month,lost_week=@week,lost_day_of_week=@day " +
                            "WHERE title_id=@title AND lost_year IS NULL;";
                        AddParameter(command, "@show", sourceName); AddParameter(command, "@year", year); AddParameter(command, "@month", month);
                        AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek); AddParameter(command, "@title", change.Item2); command.ExecuteNonQuery();
                    }
                    using (var command = CreateCommand(connection))
                    {
                        command.Transaction = transaction;
                        command.CommandText = "INSERT INTO title_reigns(id,title_id,universe_id,reign_number,holder_wrestler_id,holder_name,won_match_id," +
                            "won_show_name,won_year,won_month,won_week,won_day_of_week,created_utc) VALUES(@id,@title,@universe," +
                            "COALESCE((SELECT MAX(reign_number)+1 FROM title_reigns WHERE title_id=@title),1),@winner,@name,@match,@show,@year,@month,@week,@day,@created);";
                        AddParameter(command, "@id", Guid.NewGuid().ToString("N")); AddParameter(command, "@title", change.Item2);
                        AddParameter(command, "@universe", universeId); AddParameter(command, "@winner", change.Item3); AddParameter(command, "@name", winnerName);
                        AddParameter(command, "@match", change.Item1); AddParameter(command, "@show", sourceName); AddParameter(command, "@year", year);
                        AddParameter(command, "@month", month); AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek);
                        AddParameter(command, "@created", DateTime.UtcNow.ToString("O")); command.ExecuteNonQuery();
                    }
                    using (var command = CreateCommand(connection))
                    {
                        command.Transaction = transaction; command.CommandText = "UPDATE titles SET holder_wrestler_id=@winner,updated_utc=@updated WHERE id=@title;";
                        AddParameter(command, "@winner", change.Item3); AddParameter(command, "@updated", DateTime.UtcNow.ToString("O"));
                        AddParameter(command, "@title", change.Item2); command.ExecuteNonQuery();
                    }
                }

                using (var command = CreateCommand(connection))
                {
                    command.Transaction = transaction;
                    command.CommandText = "UPDATE booked_show_cards SET is_locked=1,results_finalized=1,updated_utc=@updated WHERE universe_id=@universe " +
                        "AND source_id=@source AND calendar_year=@year AND calendar_month=@month AND calendar_week=@week AND day_of_week=@day;";
                    AddParameter(command, "@updated", DateTime.UtcNow.ToString("O")); AddParameter(command, "@universe", universeId);
                    AddParameter(command, "@source", sourceId); AddParameter(command, "@year", year); AddParameter(command, "@month", month);
                    AddParameter(command, "@week", week); AddParameter(command, "@day", dayOfWeek); command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }

        public UI.CompetitionRecord GetWrestlerCompetitionRecord(string universeId, string wrestlerId)
        {
            var record = new UI.CompetitionRecord();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT COALESCE(SUM(CASE WHEN r.is_draw=0 AND r.winner_wrestler_id=@wrestler THEN 1 ELSE 0 END),0), " +
                    "COALESCE(SUM(CASE WHEN r.is_draw=0 AND r.winner_wrestler_id<>@wrestler THEN 1 ELSE 0 END),0), " +
                    "COALESCE(SUM(CASE WHEN r.is_draw=1 THEN 1 ELSE 0 END),0) FROM booked_match_participants p " +
                    "JOIN booked_matches m ON m.id=p.match_id JOIN booked_match_results r ON r.match_id=m.id " +
                    "JOIN booked_show_cards c ON c.universe_id=m.universe_id AND c.source_id=m.source_id AND c.calendar_year=m.calendar_year " +
                    "AND c.calendar_month=m.calendar_month AND c.calendar_week=m.calendar_week AND c.day_of_week=m.day_of_week " +
                    "WHERE m.universe_id=@universe AND p.wrestler_id=@wrestler AND c.results_finalized=1;";
                AddParameter(command, "@universe", universeId); AddParameter(command, "@wrestler", wrestlerId);
                using (var reader = command.ExecuteReader()) if (reader.Read())
                { record.wins = Convert.ToInt32(reader.GetValue(0)); record.losses = Convert.ToInt32(reader.GetValue(1)); record.draws = Convert.ToInt32(reader.GetValue(2)); }
            }
            return record;
        }

        public UI.CompetitionRecord GetTeamCompetitionRecord(string universeId, List<string> teamMemberIds)
        {
            var record = new UI.CompetitionRecord();
            if (teamMemberIds == null || teamMemberIds.Count < 2) return record;
            var matchIds = new List<string>(); var formats = new List<string>(); var winners = new List<string>(); var draws = new List<bool>();
            using (var connection = OpenConnection())
            using (var command = CreateCommand(connection))
            {
                command.CommandText = "SELECT m.id,m.format,r.winner_wrestler_id,r.is_draw FROM booked_matches m JOIN booked_match_results r ON r.match_id=m.id " +
                    "JOIN booked_show_cards c ON c.universe_id=m.universe_id AND c.source_id=m.source_id AND c.calendar_year=m.calendar_year " +
                    "AND c.calendar_month=m.calendar_month AND c.calendar_week=m.calendar_week AND c.day_of_week=m.day_of_week " +
                    "WHERE m.universe_id=@universe AND c.results_finalized=1 ORDER BY m.created_utc;";
                AddParameter(command, "@universe", universeId);
                using (var reader = command.ExecuteReader()) while (reader.Read())
                { matchIds.Add(reader.GetString(0)); formats.Add(reader.GetString(1)); winners.Add(reader.GetString(2)); draws.Add(reader.GetInt32(3) != 0); }
            }
            for (var matchIndex = 0; matchIndex < matchIds.Count; matchIndex++)
            {
                var split = StatTeamSplitIndex(formats[matchIndex]); if (split <= 0) continue;
                var participants = new List<string>();
                using (var connection = OpenConnection())
                using (var command = CreateCommand(connection))
                {
                    command.CommandText = "SELECT wrestler_id FROM booked_match_participants WHERE match_id=@match ORDER BY position;";
                    AddParameter(command, "@match", matchIds[matchIndex]);
                    using (var reader = command.ExecuteReader()) while (reader.Read()) participants.Add(reader.GetString(0));
                }
                if (split >= participants.Count) continue;
                var firstSide = participants.GetRange(0, split); var secondSide = participants.GetRange(split, participants.Count - split);
                List<string> teamSide = null;
                if (firstSide.Count >= 2 && firstSide.TrueForAll(teamMemberIds.Contains)) teamSide = firstSide;
                else if (secondSide.Count >= 2 && secondSide.TrueForAll(teamMemberIds.Contains)) teamSide = secondSide;
                if (teamSide == null) continue;
                if (draws[matchIndex]) record.draws++;
                else if (teamSide.Contains(winners[matchIndex])) record.wins++; else record.losses++;
            }
            return record;
        }

        private static int StatTeamSplitIndex(string format)
        {
            if (format == "Two on Two" || format == "Two on Two - Mixed Tag" || format == "Two on Two - Tornado Tag" ||
                format == "Handicap - Two on Three") return 2;
            if (format == "Three on Three" || format == "Three on Three - Tornado Tag" || format == "Triple Threat Tornado Tag") return 3;
            if (format == "Four on Four" || format == "4-Way Tornado Tag") return 4;
            if (format == "Handicap - One on Two" || format == "Handicap - One on Two Tornado Tag" || format == "Handicap - One on Three") return 1;
            return 0;
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
