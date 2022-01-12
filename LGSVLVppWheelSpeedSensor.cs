/**
 * Copyright (c) 2021-2022 Gaia Platform LLC
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using Simulator.Bridge;
using Simulator.Bridge.Data;
using Simulator.Utilities;
using UnityEngine;
using Simulator.Sensors.UI;
using System.Collections.Generic;
using Simulator.Analysis;
using System.Collections;
using VehiclePhysics;

namespace Simulator.Sensors
{
    public class LGSVLVppWheelSpeedData
    {
        public float x;
        public float y;
        public float width;
        public float height;
    }

    public class LGSVLVppWheelSpeedDataBridgePlugin : ISensorBridgePlugin
    {
        public void Register(IBridgePlugin plugin)
        {
            if (plugin?.GetBridgeNameAttribute()?.Name == "ROS" || plugin?.GetBridgeNameAttribute()?.Type == "ROS2")
            {
                plugin.Factory.RegPublisher(plugin,
                    (LGSVLVppWheelSpeedData data) => new Simulator.Bridge.Data.Lgsvl.BoundingBox2D()
                    {
                        x = data.x,
                        y = data.y,
                        width = data.width,
                        height = data.height
                    }
                );
            }
        }
    }

    [SensorType("LGSVL VPP Wheel Speed Sensor", new[] { typeof(LGSVLVppWheelSpeedData) })]
    public class LGSVLVppWheelSpeedSensor : SensorBase
    {
        private VehicleVPPControllerInput VPP;
        private BridgeInstance Bridge;
        private Publisher<LGSVLVppWheelSpeedData> Publish;

        private float FLWheelSpeed = 0f;
        private float FRWheelSpeed = 0f;
        private float RLWheelSpeed = 0f;
        private float RRWheelSpeed = 0f;

        private void Awake()
        {
            VPP = GetComponentInParent<VehicleVPPControllerInput>();
            if (VPP == null)
            {
                Debug.LogWarning("Could not find VehicleVPPControllerInput.");
            }
        }

        protected override void Initialize()
        {
        }

        protected override void Deinitialize()
        {

        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
            FLWheelSpeed = VPP.GetWheelAngularVelocity(VPP.GetWheelIndex(0, VehicleBase.WheelPos.Left));
            FRWheelSpeed = VPP.GetWheelAngularVelocity(VPP.GetWheelIndex(0, VehicleBase.WheelPos.Right));
            RLWheelSpeed = VPP.GetWheelAngularVelocity(VPP.GetWheelIndex(1, VehicleBase.WheelPos.Left));
            RRWheelSpeed = VPP.GetWheelAngularVelocity(VPP.GetWheelIndex(1, VehicleBase.WheelPos.Right));

            if (Bridge?.Status == Status.Connected && Publish != null)
            {
                Publish(new LGSVLVppWheelSpeedData()
                {
                    x = FLWheelSpeed,
                    y = FRWheelSpeed,
                    width = RLWheelSpeed,
                    height = RRWheelSpeed
                });
            }
        }

        public override void OnBridgeSetup(BridgeInstance bridge)
        {
            if (bridge?.Plugin?.GetBridgeNameAttribute()?.Type == "ROS2")
            {
                Bridge = bridge;
                Publish = Bridge.AddPublisher<LGSVLVppWheelSpeedData>(Topic);
            }
        }

        public override void OnVisualize(Visualizer visualizer)
        {
            Debug.Assert(visualizer != null);

            var graphData = new Dictionary<string, object>()
            {
                {"FL Wheel Speed", FLWheelSpeed},
                {"FR Wheel Speed", FRWheelSpeed},
                {"RL Wheel Speed", RLWheelSpeed},
                {"RR Wheel Speed", RRWheelSpeed},
            };

            visualizer.UpdateGraphValues(graphData);
        }

        public override void OnVisualizeToggle(bool state)
        {
        }
    }
}
