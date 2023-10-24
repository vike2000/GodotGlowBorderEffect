using Godot;
using System.Collections.Generic;
namespace addons.glow_border_effect {
	interface ICanGlow { // where this: Node
		[Export]
		public	Color	GlowColor	{get;set;}
		[Export(PropertyHint.Layers3DRender)]
		public	uint	GlowLayer	{get;set;}
		public	bool	UseGlow		{get;set;}
		
		public bool SetUseGlow(bool value) {
			foreach(var glowObject in GlowInstances)
				glowObject.Visible = value;
			return value;
		}
		
		public List<GeometryInstance3D> GlowInstances {get;set;}
		
		public void _ReadyToGlow() {
			var glowMaterial = new StandardMaterial3D();
			glowMaterial.AlbedoColor = GlowColor;
			
			GlowCreateShadowMeshes((this as Node)!, glowMaterial);
		}
		// Create shadow meshes for all GeometryInstances
		// for glow effect rendering.
		public void GlowCreateShadowMeshes(Node node, Material material) {
			// Recurse down the structure in case
			// GeometryInstance3D exists as children
			foreach(var child in node.GetChildren())
				GlowCreateShadowMeshes(child, material);
			
			// Create shadow meshes for GeometryInstances
			if (node is GeometryInstance3D giNode) {
				var newName = "Glow" + giNode.Name;
				var existing = node.GetNodeOrNull(newName);
				if (existing != null)
					GlowInstances.Add((existing as GeometryInstance3D)!);
				else {
					var glowObject = (giNode.Duplicate(0) as GeometryInstance3D)!;
					glowObject.Name = newName;
					glowObject.Layers = GlowLayer;
					glowObject.MaterialOverride = material;
					
					// Clean up and remove children
					foreach (var child in glowObject.GetChildren())
						glowObject.RemoveChild(child);
					
					// Remove scripts
					glowObject.SetScript(new Variant());
					
					// Remove transformation
					glowObject.Transform = Transform3D.Identity;
					
					// Ensure objects glow according setting
					glowObject.Visible = UseGlow;
					
					// Now add the new shadow object to the tree
					node.AddChild(glowObject);
					GlowInstances.Add(glowObject);
				}
			}
		}
	}
}