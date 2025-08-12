using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using System;
using System.Collections.Generic;

namespace Paulov.Tarkov.MP2
{
    public class PaulovMPGameServer : EftNetworkGame
    {
        public BotsController BotsController { get; set; }

        public RaidSettings _raidSettings;

        private NonWavesSpawnScenario nonWavesSpawnScenario;

        public static PaulovMPGameServer Create(ISession session, ClientGameWorld gameWorld, ProfileStatusClass profileStatus, RaidSettings raidSettings, Profile savageProfile, InsuranceCompanyClass insurance, GInterface221 inputTree, GameUI gameUI, GClass2514 metricsEvents, GClass2503 metricsCollector, EUpdateQueue updateQueue, TimeSpan sessionTime, Action runCallback, Callback<ExitStatus, TimeSpan, MetricsClass> callback)
        {
            PaulovMPGameServer eftNetworkGame = CommonNetworkGame<ISession, EftGamePlayerOwner, RaidSettings>.smethod_1<PaulovMPGameServer>(session, gameWorld, profileStatus, raidSettings, savageProfile, insurance, inputTree, gameUI, metricsEvents, metricsCollector, updateQueue, sessionTime, runCallback, callback);
            eftNetworkGame.gparam_1 = session;
            eftNetworkGame.BotsController = new BotsController();
            eftNetworkGame.Create(raidSettings);

            WorldInteractiveObject.InteractionShouldBeConfirmed = false;
            return eftNetworkGame;
        }

        public void Create(RaidSettings raidSettings)
        {
            _raidSettings = raidSettings;
            BotsController = new BotsController();
            WildSpawnWave[] waves = CreateWildSpawnWaves();
            nonWavesSpawnScenario = NonWavesSpawnScenario.smethod_0(this, this.location_0, BotsController);
            nonWavesSpawnScenario.ImplementWaveSettings(new WavesSettings(EFT.Bots.EBotAmount.AsOnline, EFT.Bots.EBotDifficulty.AsOnline, true, false));
        }

        WildSpawnWave[] CreateWildSpawnWaves()
        {
            List<WildSpawnWave> waves = new List<WildSpawnWave>();
            foreach (WildSpawnWave wildSpawnWave in location_0.waves)
            {
                wildSpawnWave.slots_min = 1;
                wildSpawnWave.slots_max = Math.Max(1, wildSpawnWave.slots_max);
            }
            return waves.ToArray();
        }
    }
}
