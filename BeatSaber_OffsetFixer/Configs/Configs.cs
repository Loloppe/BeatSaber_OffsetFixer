using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace BeatSaber_OffsetFixer.Configs
{
	public class Configs
	{
		public static Configs Instance;
		public virtual bool Enabled { get; set; } = true;
		public virtual float ReactionTime { get; set; } = 450f;
		public virtual bool NJSMultiplier { get; set; } = true;
		public virtual float NJS { get; set; } = 20f;
		public virtual bool Snap { get; set; } = true;
		public virtual float Precision { get; set; } = 4f;
		public virtual bool Overwrite { get; set; } = true;
		public virtual float SS { get; set; } = 0.85f;
		public virtual float FS { get; set; } = 1.2f;
		public virtual float SF { get; set; } = 1.5f;

		/// <summary>
		/// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
		/// </summary>
		public virtual void OnReload()
		{
			// Do stuff after config is read from disk.
		}

		/// <summary>
		/// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
		/// </summary>
		public virtual void Changed()
		{
			// Do stuff when the config is changed.
		}

		/// <summary>
		/// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
		/// </summary>
		public virtual void CopyFrom(Configs other)
		{
			// This instance's members populated from other
		}
	}
}
