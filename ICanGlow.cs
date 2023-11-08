using Godot;
using System.Collections.Generic;
namespace addons.glow_border_effect {
	interface ICanGlow { // where this: Node
		[Export]
		public	StandardMaterial3D	GlowMaterial	{get;set;}
		[Export(PropertyHint.Layers3DRender)]
		public	uint				GlowLayer		{get;set;}
		public	bool				UseGlow			{get;set;}
		
		// the idea is for a UseGlow setter implementation to use its backing field (not implementable like DIM in this interface) like the following:
		// public bool UseGlow {get => _useGlow; set => _useGlow = (this as ICanGlow).UpdateGlow(value);}
		
		public bool UpdateGlow(bool value) {
			foreach(var glowObject in GlowInstances)
				glowObject.Visible = value;
			
			return value;
		}
		
		public List<GeometryInstance3D> GlowInstances {get;set;}
		
		public void _ReadyToGlow() {
			GlowCreateShadowMeshes((this as Node)!);
		}
		// Create shadow meshes for all GeometryInstances
		// for glow effect rendering.
		public void GlowCreateShadowMeshes(Node node) {
			// Recurse down the structure in case
			// GeometryInstance3D exists as children
			foreach(var child in node.GetChildren())
				GlowCreateShadowMeshes(child);
			
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
					glowObject.MaterialOverride = GlowMaterial;
					
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