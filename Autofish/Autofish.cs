// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024 Jiamu Sun <barroit@linux.com>
 */

using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

using PlayerEquipment;
using PlayerState;

using static CommandInputButtonNames;
using static Unity.Collections.LowLevel.Unsafe.UnsafeUtility;

public struct AutofishCD : IComponentData
{
	public TickTimer clicking;
	public bool pulled;
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

	AutofishCD fisher = new AutofishCD {
		clicking = new TickTimer(0.1f, rate),
		pulled   = false,
	};

	EntityManager.SetComponentData(ent, fisher);
}

[BurstCompile]
private void autofish(ref ClientInput input,
		      in FishingStateCD fishing,
		      in PlayerStateCD player,
		      ref AutofishCD fisher,
		      in NetworkTick tick)
{
	if (fisher.clicking.isRunning) {
		if (!fisher.clicking.IsTimerElapsed(tick)) {
			input.SetButton(SecondInteract_HeldDown, true);
			return;
		}

		fisher.clicking.Stop(tick);
	}

	if (fisher.pulled && player.currentState != PlayerStateEnum.Fishing) {
		fisher.clicking.Start(tick);
		fisher.pulled = false;
	} else if (fishing.fishIsNibbling && !fishing.isFishingAtOctopusBoss) {
		fisher.clicking.Start(tick);
		fisher.pulled = true;
	}
}

[BurstCompile]
protected override void OnUpdate()
{
	NetworkTime time = SystemAPI.GetSingleton<NetworkTime>();
	NetworkTick tick = time.ServerTick;

	RefRW<AutofishCD> __fisher = SystemAPI.GetSingletonRW<AutofishCD>();
	AutofishCD fisher = __fisher.ValueRW;

	foreach (var (__input, __fishing, __player, __slot) in
		 SystemAPI.Query<RefRW<ClientInputData>,
				 RefRO<FishingStateCD>,
				 RefRO<PlayerStateCD>,
				 RefRO<EquipmentSlotCD>>()
			  .WithAll<GhostOwnerIsLocal>()) {
		ClientInput input = As<ClientInputData,
				       ClientInput>(ref __input.ValueRW);
		FishingStateCD fishing = __fishing.ValueRO;
		PlayerStateCD player = __player.ValueRO;
		EquipmentSlotCD slot = __slot.ValueRO;

		if (fishing.useFishingMiniGame ||
		    slot.slotType != EquipmentSlotType.FishingRodSlot)
			continue;

		if (Manager.ui.isAnyInventoryShowing ||
		    Manager.menu.IsAnyMenuActive()) {
			fisher.pulled = false;
			if (fisher.clicking.isRunning)
				fisher.clicking.Stop(tick);
			continue;
		}

		if (input.IsButtonSet(SecondInteract_HeldDown)) {
			fisher.pulled = false;
			continue;
		}

		autofish(ref input, in fishing,
			 in player, ref fisher, in tick);
		__input.ValueRW = As<ClientInput, ClientInputData>(ref input);
	}

	__fisher.ValueRW = fisher;
}

}
