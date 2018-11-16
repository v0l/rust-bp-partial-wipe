using Newtonsoft.Json;
using SilentOrbit.ProtocolBuffers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace rust_wipe
{
    class Program
    {
        static Task Main(string[] args)
        {
            Console.WriteLine("v0l partial rust blueprint wiper v0.1\n");
            return RunTasks();
        }

        private static async Task<List<ItemDefinition>> LoadItemDefs()
        {
            string url = "https://raw.githubusercontent.com/v0l/public/master/rust_items.json";

            var json = await new WebClient().DownloadStringTaskAsync(new Uri(url));

            return JsonConvert.DeserializeObject<List<ItemDefinition>>(json);
        }

        private static async Task RunTasks()
        {
            var cfg = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync("config.json"));

            await WipeBlueprints(cfg);
        }

        private static async Task<Dictionary<string, Player>> GetSteamProfileInfo(Config cfg, IEnumerable<string> ids)
        {
            var rsp = JsonConvert.DeserializeObject<SteamApiResponse>(await new WebClient().DownloadStringTaskAsync($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={cfg.SteamApiKey}&steamids={string.Join(",", ids)}"));

            return rsp.response.players.GroupBy(a => a.steamid).ToDictionary(a => a.Key, b => b.First());
        }

        private static async Task WipeBlueprints(Config cfg)
        {
            using (var con = new SQLiteConnection($"Data Source={cfg.DBPath}"))
            {
                await con.OpenAsync();

                var items = await LoadItemDefs();
                var item_dict = items.GroupBy(a => a.itemid).ToDictionary(a => a.Key, b => b.FirstOrDefault());

                Console.WriteLine($"Wiping: \n\t{string.Join("\n\t", cfg.WipeList)}\n");

                var wipe_items = item_dict.Where(a => cfg.WipeList.Contains(a.Value.shortname)).ToDictionary(a => a.Key, b => b.Value);

                var bps = await GetBlueprints(con, item_dict);

                //load user names if steamapikey is set
                Dictionary<string, string> names = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(cfg.SteamApiKey))
                {
                    var uids = bps.Keys.Distinct();
                    while (uids.Count() > 0)
                    {
                        var batch = uids.Take(100);
                        foreach (var profile in await GetSteamProfileInfo(cfg, batch))
                        {
                            names.Add(profile.Key, profile.Value.personaname);
                        }
                        uids = uids.Skip(100);
                    }
                }

                Console.WriteLine($"steamid{(!string.IsNullOrEmpty(cfg.SteamApiKey) ? ",username" : "")},blueprints_known,blueprints_removed");
                foreach (var userbp in bps.OrderByDescending(a => a.Value.Count))
                {
                    var removed = userbp.Value.RemoveWhere(a => wipe_items.ContainsKey(a));
                    Console.WriteLine($"{(!string.IsNullOrEmpty(cfg.SteamApiKey) ? $"{userbp.Key},{names[userbp.Key]}" : userbp.Key)},{userbp.Value.Count},{removed}");
                }

                await UpdateBlueprints(con, bps);

                con.Close();
                Console.ReadKey();
            }
        }

        private static async Task UpdateBlueprints(SQLiteConnection con, Dictionary<string, HashSet<int>> newIds)
        {
            using (var cmd = new SQLiteCommand("update data set info = @info where userid = @user", con))
            {
                foreach (var ubp in newIds)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("info", SerializeItemList(ubp.Value));
                    cmd.Parameters.AddWithValue("user", ubp.Key);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static byte[] SerializeItemList(HashSet<int> items)
        {
            using (var ms = new MemoryStream())
            {
                foreach (var item in items)
                {
                    ProtocolParser.WriteKey(ms, new Key(3, Wire.Varint));
                    ProtocolParser.WriteUInt64(ms, (ulong)item);
                }

                // i have no idea why this is on the end... but ok
                // field 100 wiretype variant with value 0
                // yep this makes sense, should be until EOF??? 
                ms.Write(new byte[]
                {
                    0xA0, 0x06, 0x00
                });

                return ms.ToArray();
            }
        }

        private static async Task<Dictionary<string, HashSet<int>>> GetBlueprints(SQLiteConnection con, Dictionary<int, ItemDefinition> item_dict)
        {
            var ret = new Dictionary<string, HashSet<int>>();

            using (var cmd_get = new SQLiteCommand("select * from data", con))
            {
                using (var r = await cmd_get.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        var uid = r[0] as string;
                        var bp_data = r[1] as byte[];
                        if (!string.IsNullOrEmpty(uid) && bp_data != null)
                        {
                            using (var bp_ms = new MemoryStream(bp_data))
                            {
                                Key k;
                                while ((k = ProtocolParser.ReadKey(bp_ms)).Field != 100)
                                {
                                    int itemid = 0;
                                    if (k.WireType == Wire.Varint)
                                    {
                                        itemid = (int)ProtocolParser.ReadUInt64(bp_ms);
                                    }

                                    //add to user
                                    if (itemid != 0 && item_dict.ContainsKey(itemid))
                                    {
                                        if (!ret.ContainsKey(uid))
                                        {
                                            ret.Add(uid, new HashSet<int>() { itemid });
                                        }
                                        else
                                        {
                                            if (!ret[uid].Contains(itemid))
                                            {
                                                ret[uid].Add(itemid);
                                            }
                                            else
                                            {
                                                Console.WriteLine($"{uid} had a duplicate item: {itemid}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }
    }

    internal class Config
    {
        public string DBPath { get; set; }
        public string SteamApiKey { get; set; }
        public string[] WipeList { get; set; }
    }

    internal class Player
    {
        public string steamid { get; set; }
        public int communityvisibilitystate { get; set; }
        public int profilestate { get; set; }
        public string personaname { get; set; }
        public int lastlogoff { get; set; }
        public string profileurl { get; set; }
        public string avatar { get; set; }
        public string avatarmedium { get; set; }
        public string avatarfull { get; set; }
        public int personastate { get; set; }
        public string primaryclanid { get; set; }
        public int timecreated { get; set; }
        public int personastateflags { get; set; }
        public string loccountrycode { get; set; }
    }

    internal class Response
    {
        public List<Player> players { get; set; }
    }

    internal class SteamApiResponse
    {
        public Response response { get; set; }
    }
}
