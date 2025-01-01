// SPDX-License-Identifier: GPL-3.0-or-later
/*
 * Copyright 2024, 2025 Jiamu Sun <barroit@linux.com>
 */

using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

using PlayerEquipment;
using PlayerState;

using static CommandInputButtonNames;
using static Unity.Collections.LowLevel.Unsafe.UnsafeUtility;

struct FisherCD : IComponentData {
	public TickTimer tmr;
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(RunSimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(SendClientInputSystem))]
public partial class Fisher : SystemBase {

public static Entity entity;
public static EntityManager manager;

protected override void OnCreate()
{
	manager = EntityManager;
	entity = manager.CreateSingleton<FisherCD>();

	uint rate = (uint)NetworkingManager.GetSimulationTickRateForPlatform();
	FisherCD fisher = new FisherCD {
		tmr    = new TickTimer(0.2f, rate),
	};

	manager.SetComponentData(entity, fisher);
}

[BurstCompile]
protected override void OnUpdate()
{
	NetworkTime time = SystemAPI.GetSingleton<NetworkTime>();
	NetworkTick tick = time.ServerTick;

	RefRW<FisherCD> __fisher = SystemAPI.GetSingletonRW<FisherCD>();
	FisherCD fisher = __fisher.ValueRW;

	foreach (var (__input, __fishing, __slot) in
		 SystemAPI.Query<RefRW<ClientInputData>,
				 RefRO<FishingStateCD>,
				 RefRO<EquipmentSlotCD>>()
			  .WithAll<GhostOwnerIsLocal>()) {
		ClientInput input = As<ClientInputData,
				       ClientInput>(ref __input.ValueRW);
		FishingStateCD fishing = __fishing.ValueRO;
		EquipmentSlotCD slot = __slot.ValueRO;

		if (slot.slotType != EquipmentSlotType.FishingRodSlot)
			continue;

		input.useFishingMiniGame = false;

		if (Manager.ui.isAnyInventoryShowing ||
		    Manager.menu.IsAnyMenuActive()) {
			if (fisher.tmr.isRunning)
				fisher.tmr.Stop(tick);
			continue;
		}

		if (input.IsButtonSet(SecondInteract_HeldDown))
			goto next;

		if (fisher.tmr.isRunning) {
			if (!fisher.tmr.IsTimerElapsed(tick)) {
				input.SetButton(SecondInteract_HeldDown, true);
				goto next;
			}

			fisher.tmr.Stop(tick);
		}

		if (fishing.fishIsNibbling &&!fishing.isFishingAtOctopusBoss)
			fisher.tmr.Start(tick);

next:
		__input.ValueRW = As<ClientInput, ClientInputData>(ref input);
	}

	__fisher.ValueRW = fisher;
}

} /* partial class Fisher */
