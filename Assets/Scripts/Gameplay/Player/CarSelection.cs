using Unity.Burst;
using Unity.Entities.Racing.Common;
using Unity.Transforms;
using UnityEngine.UIElements;
using static Unity.Entities.SystemAPI;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Sets events for player skin buttons
    /// </summary>
    public partial struct InitializeCarSelectionSystem : ISystem
    {
        private bool m_Initialized;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CarSelection>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Set callbacks and initialize Car Selection once
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
                skinButton.RegisterCallback<ClickEvent>(_ => skinButton.Focus());
                skinButton.RegisterCallback<FocusInEvent>(_ =>
                {
                    entityManager.SetComponentData(carSelectionEntity, new CarSelectionUpdate
                    {
                        ShouldUpdate = true,
                        NewSkinId = index
                    });
                    PlayerInfoController.Instance.SkinId = index;
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

    /// <summary>
    /// Destroy the previous skin if there is
    /// and instantiate the new prefab skin.
    /// </summary>
    public partial struct UpdateCarSelectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CarSelection>();
            state.RequireForUpdate<CarSelectionUpdate>();
        }

        [BurstCompile]
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
            carSelectionUpdate.ShouldUpdate = false;

            SetSingleton(carSelection);
            SetSingleton(carSelectionUpdate);
        }
    }
}