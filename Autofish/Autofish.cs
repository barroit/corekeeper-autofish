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
	public TickTimer delay;
	public TickTimer pullup;

	public bool is_active;
	public bool need_pull;
	public bool need_thrw;
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
		cd        = new TickTimer(0.5f, rate),
		delay     = new TickTimer(0.6f, rate),
		pullup    = new TickTimer(0.9f, rate),
		is_active = false,
		need_pull = false,
		need_thrw = false,
	};

	EntityManager.SetComponentData(ent, fisher);
}

[BurstCompile]
protected override void OnUpdate()
{
	NetworkTime time = SystemAPI.GetSingleton<NetworkTime>();
	NetworkTick tick = time.ServerTick;

	RefRW<AutofishCD> __fisher = SystemAPI.GetSingletonRW<AutofishCD>();
	AutofishCD fisher = __fisher.ValueRW;

	foreach (var (__input, __state) in
		 SystemAPI.Query<RefRW<ClientInputData>,
				 RefRO<FishingStateCD>>()
			  .WithAll<GhostOwnerIsLocal>()) {
		ClientInput input = As<ClientInputData,
				       ClientInput>(ref __input.ValueRW);
		FishingStateCD state = __state.ValueRO;

		if (input.IsButtonSet(SecondInteract_HeldDown) &&
		    (!fisher.cd.isRunning || fisher.cd.IsTimerElapsed(tick))) {
			fisher.cd.Start(tick);
			fisher.is_active = !fisher.is_active;

			if (!fisher.is_active) {
				fisher.need_pull = false;
				fisher.need_thrw = false;

				fisher.delay.Stop(tick);
				fisher.pullup.Stop(tick);
			}
		}

		if (!fisher.is_active)
			continue;

		bool pulled = fisher.pullup.isRunning &&
			      fisher.pullup.IsTimerElapsed(tick);

		if (fisher.need_pull || pulled) {
			fisher.pullup.Stop(tick);
			fisher.need_pull = !fisher.need_pull;
			goto emit_click;
		}

		if (!state.fishIsNibbling || state.isFishingAtOctopusBoss)
			continue;

		if (!fisher.pullup.isRunning && !fisher.delay.isRunning) {
			fisher.delay.Start(tick);
			continue;
		}

		bool delayed = fisher.delay.isRunning &&
			       fisher.delay.IsTimerElapsed(tick);

		if (fisher.need_thrw || delayed) {
			fisher.delay.Stop(tick);
			fisher.pullup.Start(tick);
			fisher.need_thrw = !fisher.need_thrw;
			goto emit_click;
		}

		continue;
emit_click:
		input.SetButton(SecondInteract_HeldDown, true);
		__input.ValueRW = As<ClientInput, ClientInputData>(ref input);
	}

	__fisher.ValueRW = fisher;
}

}
