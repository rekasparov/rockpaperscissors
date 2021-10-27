using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPS.WebServer
{
    public class GameHub : Hub
    {
        private static Dictionary<string, string> connections = new Dictionary<string, string>();
        private static List<RoundModel> rounds = new List<RoundModel>();

        public static string errorFile { get => @"C:\Error.txt"; }

        private static string playerIConnectionId { get; set; }
        private static string playerIIConnectionId { get; set; }

        public override async Task OnConnectedAsync()
        {
            try
            {
                string connectionId = Context.ConnectionId;

                if (connections.Count >= 2) await Clients.Client(connectionId).SendAsync("ServerNotAvailable");
                else
                {
                    if (string.IsNullOrEmpty(playerIConnectionId)) playerIConnectionId = connectionId;
                    else playerIIConnectionId = connectionId;

                    connections.Add(connectionId, string.Empty);
                }
            }
            catch (Exception ex)
            {
                await File.WriteAllTextAsync(errorFile, ex.Message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                string connectionId = Context.ConnectionId;
                connections.Remove(connectionId);
                if (connections.Count < 2)
                {
                    rounds.Clear();
                    rounds = new List<RoundModel>();

                    await Clients.All.SendAsync("ServerAvailable");
                }
            }
            catch (Exception ex)
            {
                await File.WriteAllTextAsync(errorFile, ex.Message);
            }
        }

        public async Task MayIPlay(string nickname)
        {
            try
            {
                string connectionId = Context.ConnectionId;
                if (connections.Any(x => x.Value == nickname)) await Clients.Client(connectionId).SendAsync("NicknameAlreadyUsing");
                else
                {
                    connections[connectionId] = nickname;
                    if (connections.Count >= 2 && !connections.Any(x => string.IsNullOrEmpty(x.Value))) await Clients.All.SendAsync("LetsPlay");
                    else await Clients.Client(connectionId).SendAsync("WaitingForOpponent");
                }
            }
            catch (Exception ex)
            {
                await File.WriteAllTextAsync(errorFile, ex.Message);
            }
        }

        public async Task MySelection(int selection)
        {
            try
            {
                string connectionId = Context.ConnectionId;

                if (rounds.LastOrDefault() == null)
                {
                    rounds.Add(new RoundModel()
                    {
                        Round = 1,
                        PlayerIConnectionId = connectionId == playerIConnectionId ? connectionId : null,
                        PlayerIIConnectionId = connectionId == playerIIConnectionId ? connectionId : null,
                        PlayerISelection = connectionId == playerIConnectionId ? selection : null,
                        PlayerIISelection = connectionId == playerIIConnectionId ? selection : null,
                    });
                }
                else if (rounds.LastOrDefault(x => string.IsNullOrEmpty(x.PlayerIConnectionId) || string.IsNullOrEmpty(x.PlayerIIConnectionId)) != null)
                {
                    var lastGame = rounds.LastOrDefault();

                    if (string.IsNullOrEmpty(lastGame.PlayerIConnectionId))
                    {
                        lastGame.PlayerIConnectionId = connectionId;
                        lastGame.PlayerISelection = selection;
                    }
                    else
                    {
                        lastGame.PlayerIIConnectionId = connectionId;
                        lastGame.PlayerIISelection = selection;
                    }

                    //rock-scissors
                    if (lastGame.PlayerISelection == 0 && lastGame.PlayerIISelection == 2)
                    {
                        lastGame.PlayerIWon = true;
                        await Clients.Client(lastGame.PlayerIConnectionId).SendAsync("YouWin", rounds.Where(x => x.PlayerIWon).Count(), rounds.Where(x => x.PlayerIIWon).Count(), lastGame.PlayerIISelection);
                        await Clients.Client(lastGame.PlayerIIConnectionId).SendAsync("YouLost", rounds.Where(x => x.PlayerIIWon).Count(), rounds.Where(x => x.PlayerIWon).Count(), lastGame.PlayerISelection);
                    }
                    else if (lastGame.PlayerIISelection == 0 && lastGame.PlayerISelection == 2)
                    {
                        lastGame.PlayerIIWon = true;
                        await Clients.Client(lastGame.PlayerIIConnectionId).SendAsync("YouWin", rounds.Where(x => x.PlayerIIWon).Count(), rounds.Where(x => x.PlayerIWon).Count(), lastGame.PlayerISelection);
                        await Clients.Client(lastGame.PlayerIConnectionId).SendAsync("YouLost", rounds.Where(x => x.PlayerIWon).Count(), rounds.Where(x => x.PlayerIIWon).Count(), lastGame.PlayerIISelection);
                    }
                    //scissors-paper
                    else if (lastGame.PlayerISelection == 2 && lastGame.PlayerIISelection == 1)
                    {
                        lastGame.PlayerIWon = true;
                        await Clients.Client(lastGame.PlayerIConnectionId).SendAsync("YouWin", rounds.Where(x => x.PlayerIWon).Count(), rounds.Where(x => x.PlayerIIWon).Count(), lastGame.PlayerIISelection);
                        await Clients.Client(lastGame.PlayerIIConnectionId).SendAsync("YouLost", rounds.Where(x => x.PlayerIIWon).Count(), rounds.Where(x => x.PlayerIWon).Count(), lastGame.PlayerISelection);
                    }
                    else if (lastGame.PlayerIISelection == 2 && lastGame.PlayerISelection == 1)
                    {
                        lastGame.PlayerIIWon = true;
                        await Clients.Client(lastGame.PlayerIIConnectionId).SendAsync("YouWin", rounds.Where(x => x.PlayerIIWon).Count(), rounds.Where(x => x.PlayerIWon).Count(), lastGame.PlayerISelection);
                        await Clients.Client(lastGame.PlayerIConnectionId).SendAsync("YouLost", rounds.Where(x => x.PlayerIWon).Count(), rounds.Where(x => x.PlayerIIWon).Count(), lastGame.PlayerIISelection);
                    }
                    //paper-rock
                    else if (lastGame.PlayerISelection == 1 && lastGame.PlayerIISelection == 0)
                    {
                        lastGame.PlayerIWon = true;
                        await Clients.Client(lastGame.PlayerIConnectionId).SendAsync("YouWin", rounds.Where(x => x.PlayerIWon).Count(), rounds.Where(x => x.PlayerIIWon).Count(), lastGame.PlayerIISelection);
                        await Clients.Client(lastGame.PlayerIIConnectionId).SendAsync("YouLost", rounds.Where(x => x.PlayerIIWon).Count(), rounds.Where(x => x.PlayerIWon).Count(), lastGame.PlayerISelection);
                    }
                    else if (lastGame.PlayerIISelection == 1 && lastGame.PlayerISelection == 0)
                    {
                        lastGame.PlayerIIWon = true;
                        await Clients.Client(lastGame.PlayerIIConnectionId).SendAsync("YouWin", rounds.Where(x => x.PlayerIIWon).Count(), rounds.Where(x => x.PlayerIWon).Count(), lastGame.PlayerISelection);
                        await Clients.Client(lastGame.PlayerIConnectionId).SendAsync("YouLost", rounds.Where(x => x.PlayerIWon).Count(), rounds.Where(x => x.PlayerIIWon).Count(), lastGame.PlayerIISelection);
                    }
                    else
                    {
                        await Clients.Client(lastGame.PlayerIIConnectionId).SendAsync("NoWin", rounds.Where(x => x.PlayerIIWon).Count(), rounds.Where(x => x.PlayerIWon).Count(), lastGame.PlayerISelection);
                        await Clients.Client(lastGame.PlayerIConnectionId).SendAsync("NoWin", rounds.Where(x => x.PlayerIWon).Count(), rounds.Where(x => x.PlayerIIWon).Count(), lastGame.PlayerIISelection);
                    }
                }
                else
                {
                    var lastRound = rounds.LastOrDefault().Round;
                    var newRound = lastRound + 1;

                    rounds.Add(new RoundModel()
                    {
                        Round = newRound,
                        PlayerIConnectionId = connectionId == playerIConnectionId ? connectionId : null,
                        PlayerIIConnectionId = connectionId == playerIIConnectionId ? connectionId : null,
                        PlayerISelection = connectionId == playerIConnectionId ? selection : null,
                        PlayerIISelection = connectionId == playerIIConnectionId ? selection : null,
                    });
                }

                if (rounds.Count == 5 &&
                    !rounds.Any(x => string.IsNullOrEmpty(x.PlayerIConnectionId) && string.IsNullOrEmpty(x.PlayerIIConnectionId)) &&
                    !rounds.Any(x => x.PlayerISelection == null || x.PlayerIISelection == null))
                {
                    await Clients.Client(playerIConnectionId).SendAsync("GameFinish", rounds.Where(x => x.PlayerIConnectionId == playerIConnectionId && x.PlayerIWon).Count(), rounds.Where(x => x.PlayerIIConnectionId == playerIIConnectionId && x.PlayerIIWon).Count());
                    await Clients.Client(playerIIConnectionId).SendAsync("GameFinish", rounds.Where(x => x.PlayerIIConnectionId == playerIIConnectionId && x.PlayerIIWon).Count(), rounds.Where(x => x.PlayerIConnectionId == playerIConnectionId && x.PlayerIWon).Count());

                    rounds = new List<RoundModel>();
                }
                else if (rounds.Count < 5 && !rounds.Any(x => string.IsNullOrEmpty(x.PlayerIConnectionId) || string.IsNullOrEmpty(x.PlayerIIConnectionId)))
                {
                    await Clients.All.SendAsync("NewRound");
                }
            }
            catch (Exception ex)
            {
                await File.WriteAllTextAsync(errorFile, ex.Message);
            }
        }
    }

    public class RoundModel
    {
        public int Round { get; set; }
        public string PlayerIConnectionId { get; set; }
        public string PlayerIIConnectionId { get; set; }
        public int? PlayerISelection { get; set; }
        public int? PlayerIISelection { get; set; }
        public bool PlayerIWon { get; set; }
        public bool PlayerIIWon { get; set; }
    }
}
