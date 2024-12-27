// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024 Jiamu Sun <barroit@linux.com>
 */

/*
 * Core Keeper resets our state every time we enter a world. We rely on this
 * mechanism to implement the systems, as the system chain requires at least
 * one Walk state (current and next) to exist in PlayerState before the player
 * goes fishing.
 */

using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

using PlayerState;

using static CommandInputButtonNames;
using static Unity.Collections.LowLevel.Unsafe.UnsafeUtility;

public struct AutofishCD : IComponentData
{
	public TickTimer cd;
	public TickTimer clicking;
	public TickTimer pullup_delay;
	public bool enabled;
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(RunSimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(SendClientInputSystem))]
public partial class Autofish : SystemBase
{

protected override void OnCreate()
{
	Entity ent = EntityManager.CreateSingleton<AutofishCD>();
	uint rate = (uint)NetworkingManager.GetSimulationTickRateForPlatform();

	/*
	 * See Pug.ECS.Conversion/PlayerAuthoringConverter.cs for the first
	 * argument of TickTimer constructor.
	 * For 1.0.1.11
	 *	castTimer = new TickTimer(1f, rate),
	 *	throwTimer = new TickTimer(1f / 3f, rate),
	 *	pullUpTimer = new TickTimer(0.75f, rate),
	 *	allowedToLeaveStateTimer = new TickTimer(0.1f, rate),
	 */
	AutofishCD fisher = new AutofishCD {
		cd           = new TickTimer(0.5f, rate),
		clicking     = new TickTimer(0.2f, rate),
		pullup_delay = new TickTimer(0.3f, rate),
		enabled      = false,
	};

	EntityManager.SetComponentData(ent, fisher);
}

private void autofish(ref ClientInput input,
		      in FishingStateCD fishing,
		      in PlayerStateCD player,
		      ref AutofishCD fisher,
		      in NetworkTick tick)
{
	if (input.IsButtonSet(SecondInteract_HeldDown) &&
	    (!fisher.cd.isRunning || fisher.cd.IsTimerElapsed(tick))) {
		fisher.cd.Start(tick);
		fisher.enabled = !fisher.enabled;

		if (!fisher.enabled) {
			fisher.pullup_delay.Stop(tick);
		}
	}

	if (!fisher.enabled)
		return;

	if (fisher.clicking.isRunning) {
		if (!fisher.clicking.IsTimerElapsed(tick)) {
			input.SetButton(SecondInteract_HeldDown, true);
			return;
		}

		fisher.clicking.Stop(tick);
	}

	if (player.currentState != PlayerStateEnum.Fishing) {
		fisher.clicking.Start(tick);
		return;
	}

	if (!fishing.fishIsNibbling || fishing.isFishingAtOctopusBoss)
		return;

	if (!fisher.pullup_delay.isRunning) {
		fisher.pullup_delay.Start(tick);
		return;
	}

	if (fisher.pullup_delay.isRunning &&
	     fisher.pullup_delay.IsTimerElapsed(tick)) {
		fisher.pullup_delay.Stop(tick);
		fisher.clicking.Start(tick);
	}
}

[BurstCompile]
protected override void OnUpdate()
{
	NetworkTime time = SystemAPI.GetSingleton<NetworkTime>();
	NetworkTick tick = time.ServerTick;

	RefRW<AutofishCD> __fisher = SystemAPI.GetSingletonRW<AutofishCD>();
	AutofishCD fisher = __fisher.ValueRW;

	foreach (var (__input, __fishing, __player) in
		 SystemAPI.Query<RefRW<ClientInputData>,
				 RefRO<FishingStateCD>,
				 RefRO<PlayerStateCD>>()
			  .WithAll<GhostOwnerIsLocal>()) {
		ClientInput input = As<ClientInputData,
				       ClientInput>(ref __input.ValueRW);
		FishingStateCD fishing = __fishing.ValueRO;
		PlayerStateCD player = __player.ValueRO;

		autofish(ref input, in fishing,
			 in player, ref fisher, in tick);
		__input.ValueRW = As<ClientInput, ClientInputData>(ref input);
	}

	__fisher.ValueRW = fisher;
}

}
