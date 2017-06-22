using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP;
using KSP.UI.Screens;
using KSP.Localization;
using SmartTank.Simulation;

namespace SmartTank {

	// Convert from en-UK to en-US
	using MonoBehavior = UnityEngine.MonoBehaviour;

	/// Our main plugin behavior.
	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class SmartTank : MonoBehavior {

		public SmartTank() : base() { }

		/// <summary>
		/// Machine-readable name for this mod.
		/// Use this for directory/file names, etc.
		/// </summary>
		public const string Name = "SmartTank";

		private static bool ProceduralPartsInstalled = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "ProceduralParts");

		public static float configuredTWR = 1.5f;

		/// <summary>
		/// This is called at creation
		/// </summary>
		public void Start()
		{
			if (ProceduralPartsInstalled) {
				SimManager.OnReady += OnSimUpdate;
			}
		}

		private void Update()
		{
			if (ProceduralPartsInstalled) {
				try {
					SimManager.Gravity = gravAccel(FlightGlobals.GetHomeBody());
					SimManager.Atmosphere = FlightGlobals.GetHomeBody().GetPressure(0) * PhysicsGlobals.KpaToAtmospheres;
					SimManager.Mach = 0;
					SimManager.RequestSimulation();
					SimManager.TryStartSimulation();
				} catch (Exception e) {
				}
			}
		}

		/// <summary>
		/// This is called at destroy
		/// </summary>
		public void OnDisable()
		{
			if (ProceduralPartsInstalled) {
				SimManager.OnReady -= OnSimUpdate;
			}
		}

		/// <summary>
		/// Fires when the simulator is updated
		/// </summary>
		private void OnSimUpdate()
		{
			double totalMassChange = 0;

			for (int st = 0; st < SimManager.Stages.Length; ++st) {
				Stage stage = SimManager?.Stages[st] ?? null;
				int numTanks = stage.drainedTanks.Count;

				if (stage != null && stage.thrust > 0 && numTanks > 0) {
					double dryMass = stage.totalMass - stage.resourceMass + totalMassChange;
					double targetFuelMass = optimalFuelMass(
						stage.thrust,
						stage.drainedTanks[0].bodyGravAccel,
						stage.drainedTanks[0].targetTWR,
						dryMass
					);
					double massChange = targetFuelMass > 0 ? targetFuelMass - stage.resourceMass : 0;
					// Assume we'll have our way if auto scaling,
					// otherwise use the existing mass
					if (stage.drainedTanks[0].AutoScale) {
						totalMassChange += massChange;
					}

					// Distribute the mass in the same proportions as it is now
					double massSum = 0;
					for (int t = 0; t < numTanks; ++t) {
						massSum += stage.drainedTanks[t].mass;
					}
					for (int t = 0; t < numTanks; ++t) {
						stage.drainedTanks[t].IdealWetMass = targetFuelMass * stage.drainedTanks[t].mass / massSum;
					}
				}
			}
		}

		// F = m * a
		// F = mu * m / r^2
		// a = F / m = mu / r^2
		public static double gravAccel(CelestialBody b)
		{
			return b.gravParameter / Math.Pow(b.Radius, 2);
		}

		/// <summary>
		/// Calculate the mass of fuel needed to achieve the desired TWR for a given thrust and dry mass.
		/// </summary>
		/// <param name="thrust">Thrust in kN</param>
		/// <param name="desiredTWR">Thrust weight ratio to aim for</param>
		/// <param name="dryMass">Mass in metric tons that will be left when this stage's fuel is gone</param>
		/// <returns>
		/// Mass in metric tons of fuel that should be used
		/// </returns>
		private static double optimalFuelMass(double thrust, double gravAccel, double desiredTWR, double dryMass)
		{
			if (desiredTWR > 0 && gravAccel > 0 && dryMass > 0) {
				return Math.Max(
					0 ,
					thrust / desiredTWR / gravAccel - dryMass
				);
			} else {
				return 0;
			}
		}

	}

}
