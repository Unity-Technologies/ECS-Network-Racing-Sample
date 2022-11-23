using Dots.Racing;
using Gameplay.UI;
using Unity.Burst;
using Unity.Entities.Racing.Common;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine.UIElements;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InitializeCarSelectionSystem : ISystem
    {
        private bool m_Initialized;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CarSelection>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            if (m_Initialized)
            {
                state.Enabled = false;
                return;
            }

            if (CarSelectionUI.Instance == null)
            {
                return;
            }

            var entityManager = state.EntityManager;
            var carSelectionEntity = GetSingletonEntity<CarSelection>();
            for (var i = 0; i < CarSelectionUI.SkinButtons.Count; i++)
            {
                var skinButton = CarSelectionUI.SkinButtons[i];
                var index = i;
                skinButton.RegisterCallback<ClickEvent>(evt =>
                {
                    entityManager.SetComponentData(carSelectionEntity, new CarSelectionUpdate
                    {
                        ShouldUpdate = true,
                        NewSkinId = index
                    });
                    skinButton.Focus();
                });

                skinButton.RegisterCallback<FocusInEvent>(evt =>
                {
                    entityManager.SetComponentData(carSelectionEntity, new CarSelectionUpdate
                    {
                        ShouldUpdate = true,
                        NewSkinId = index
                    });
                });
            }

            entityManager.SetComponentData(carSelectionEntity, new CarSelectionUpdate
            {
                ShouldUpdate = true,
                NewSkinId = 0
            });

            m_Initialized = true;
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct UpdateCarSelectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CarSelection>();
            state.RequireForUpdate<CarSelectionUpdate>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var carSelectionUpdate = GetSingleton<CarSelectionUpdate>();

            if (!carSelectionUpdate.ShouldUpdate)
            {
                return;
            }

            var carSelection = GetSingleton<CarSelection>();

            if (Exists(carSelection.CurrentSkin))
            {
                state.EntityManager.DestroyEntity(carSelection.CurrentSkin);
            }

            var carSkinBuffer = GetSingletonBuffer<CarSelectionSkinData>();
            var newSkin = state.EntityManager.Instantiate(carSkinBuffer[carSelectionUpdate.NewSkinId].Prefab);
            state.EntityManager.SetComponentData(newSkin, new LocalTransform
            {
                Position = carSelection.SkinPosition,
                Rotation = carSelection.SkinRotation,
                Scale = 1
            });

            carSelection.CurrentSkin = newSkin;
            carSelection.SelectedId = carSelectionUpdate.NewSkinId;
            carSelectionUpdate.ShouldUpdate = false;

            SetSingleton(carSelection);
            SetSingleton(carSelectionUpdate);
        }
    }


    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct CarSelectionReadySystem : ISystem
    {
        private bool m_CarSelectionReady;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            if (m_CarSelectionReady)
            {
                state.Enabled = false;
                return;
            }

            TryGetSingletonEntity<CarSelection>(out var carSelectionEntity);
            var entityManager = state.EntityManager;
            if (CarSelectionUI.Instance != null)
            {
                CarSelectionUI.Instance.StartGameEvent += () =>
                {
                    var name = MainMenu.Instance.NameField.value;
                    var skinId = entityManager.GetComponentData<CarSelection>(carSelectionEntity).SelectedId;
                    var e = entityManager.CreateEntity(typeof(SendRpcCommandRequestComponent));
                    entityManager.AddComponentData(e, new SpawnPlayerRequest { Name = name, Id = skinId });
                };
            }
            else
            {
                var e = entityManager.CreateEntity(typeof(SendRpcCommandRequestComponent));
                entityManager.AddComponentData(e, new SpawnPlayerRequest());
            }

            m_CarSelectionReady = true;
        }
    }
}