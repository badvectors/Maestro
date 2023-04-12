using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using vatsys;
using vatsys.Plugin;

namespace MaestroPlugin
{
    [Export(typeof(IPlugin))]
    public class MaestroPlugin : IPlugin
    {
        public string Name => nameof(MaestroPlugin);
        private static BindingList<MaestroAircraft> Aircraft { get; set; } = new BindingList<MaestroAircraft>();
        private static System.Timers.Timer Timer { get; set; } = new System.Timers.Timer();
        private static HttpClient Client { get; set; } = new HttpClient();
        private static string Url => "https://localhost:7258/Updates";

        public MaestroPlugin()
        {
            Log.Start();

            Timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            Timer.Interval = 3000;
            Timer.Enabled = false;
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Remove();
        }

        public async void OnFDRUpdate(FDP2.FDR updated)
        {
            await FDRUpdate(updated);
        }

        public async void OnRadarTrackUpdate(RDP.RadarTrack updated)
        {
            if (updated.CoupledFDR != null) await FDRUpdate(updated.CoupledFDR);

            var existing = Aircraft.FirstOrDefault(x => x.Callsign == updated.ActualAircraft.Callsign);

            if (existing == null) return;

            existing.RadarUpdate(updated);

            await Send(existing);
        }

        private async Task FDRUpdate(FDP2.FDR updated)
        {
            var existing = Aircraft.FirstOrDefault(x => x.Callsign == updated.Callsign);

            if (existing != null)
            {
                existing.FDRUpdate(updated);

                await Send(existing);
            }
            else
            {
                var aircraft = new MaestroAircraft(updated);

                Aircraft.Add(aircraft);

                await Send(aircraft);
            }
        }

        private static async Task Send(MaestroAircraft maestroAircraft)
        {
            try
            {
                var json = JsonConvert.SerializeObject(maestroAircraft);

                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                Log.This(json, maestroAircraft.Callsign);

                await Client.PostAsync(Url, httpContent);
            }
            catch (Exception ex)
            {
                Log.This(ex.Message);

                if (ex.InnerException != null) Log.This(ex.InnerException.Message);
            }
        }

        private static void Remove()
        {
            foreach (var aircraft in Aircraft)
            {
                var target = Aircraft.FirstOrDefault(x => x.Callsign == aircraft.Callsign);

                if (target == null) continue;

                if (FDP2.GetFDRIndex(aircraft.Callsign) == -1)
                {
                    Aircraft.Remove(target);
                    continue;
                }

                if (aircraft.GroundSpeed <= 30)
                {
                    Aircraft.Remove(target);
                    continue;
                }

                if (DateTime.UtcNow.Subtract(aircraft.LastSeen) > TimeSpan.FromMinutes(1))
                {
                    Aircraft.Remove(target);
                    continue;
                }
            }
        }
    }
}
