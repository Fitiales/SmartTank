using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using ProceduralParts;
using KSP.UI.Screens;

namespace SmartTank {

	using MonoBehavior = UnityEngine.MonoBehaviour;

	/// <summary>
	/// App-wide settings, mainly defaults for part-specific settings.
	/// Patterned after BasicDeltaV_Settings by DMagic.
	/// </summary>
	public class Settings : MonoBehavior {

		private Settings()
		{
			if (File.Exists(path)) {
				ConfigNode.LoadObjectFromConfig(this, ConfigNode.Load(path));
			}
		}

		public void Save()
		{
			ConfigNode.CreateConfigFromObject(this, new ConfigNode(GetType().Name)).Save(path);
		}

		private const           string   settingsSuffix   = "settings";
		private static readonly string   path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{SmartTank.Name}.{settingsSuffix}";
		public  static readonly Settings Instance         = new Settings();

		private const           string   fuelResourceName = "LiquidFuel";

		public void HideNonProceduralFuelTanksChanged()
		{
			PartCategories fromCat, toCat;
			if (HideNonProceduralFuelTanks) {
				fromCat = PartCategories.FuelTank;
   				toCat   = PartCategories.none;
			} else {
				fromCat = PartCategories.none;
				toCat   = PartCategories.FuelTank;
			}
			List<AvailablePart> parts = PartLoader.LoadedPartsList;
			for (int p = 0; p < parts.Count; ++p) {
				if (!parts[p].partPrefab.HasModule<ProceduralPart>()
						&& parts[p].category == fromCat) {
					for (int r = 0; r < parts[p].resourceInfos.Count; ++r) {
						if (parts[p].resourceInfos[r].resourceName == fuelResourceName) {
							parts[p].category = toCat;
							break;
						}
					}
				}
			}
			EditorPartList.Instance.Refresh();
		}

		// These are the actual settings.
		// Access them via Settings.Instance.PropertyName.
		[Persistent] public bool   DiameterMatching           = true;
		[Persistent] public bool   FuelMatching               = true;
		[Persistent] public bool   AutoScale                  = true;
		[Persistent] public bool   Atmospheric                = true;
		[Persistent] public bool   HideNonProceduralFuelTanks = true;
		[Persistent] public string BodyForTWR                 = Planetarium?.fetch?.Home?.name ?? "Kerbin";
		[Persistent] public string DefaultTexture             = "Original";
		[Persistent] public float  TargetTWR                  = 1.5f;
	}

}
